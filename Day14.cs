using System.Net.Http.Headers;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
using Xunit;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2019.CSharp;

public class Day14
{

  [Theory]
  [InlineData("Day14.Sample.1", 31)]
  [InlineData("Day14.Sample.2", 165)]
  [InlineData("Day14.Sample.3", 13312)]
  [InlineData("Day14.Sample.4", 180697)]
  [InlineData("Day14.Sample.5", 2210736)]
  [InlineData("Day14", 532506)]
  public void Part1(string path, long expected)
  {
    var reactions = Convert(AoCLoader.LoadLines(path));

    var byOutput = reactions.ToDictionary(it => it.Output.Label, it => it);

    Produce(1, "FUEL", [], byOutput).Should().Be(expected);
  }

  [Theory]                    
  [InlineData("Day14.Sample.3", 82892753)]
  [InlineData("Day14.Sample.4", 5586022)]
  [InlineData("Day14.Sample.5", 460664)]
  [InlineData("Day14", 2595245)]
  public void Part2(string path, long expected)
  {
    var reactions = Convert(AoCLoader.LoadLines(path));

    var byOutput = reactions.ToDictionary(it => it.Output.Label, it => it);

    long targetOre = 1000000000000;
    
    var result = MiscUtils.BinarySearch(1000000000, (needle) => Produce(needle, "FUEL", [], byOutput) > targetOre) 
      ?? throw new ApplicationException();
    (result-1).Should().Be(expected);
  }

  static long Produce(long needed, string label, Dictionary<string, long> available, Dictionary<string, Reaction> byOutput)
  {
    if (available.GetValueOrDefault(label) >= needed)
    {
      available[label] -= needed;
      return 0;
    }

    needed -= available.GetValueOrDefault(label);
    available[label] = 0;

    var reaction = byOutput[label];
    var nReactionsNeeded = Math.DivRem(needed, reaction.Output.Count, out var remainder) + (remainder > 0 ? 1 : 0);
    var ores = 0L;
    foreach(var input in reaction.Inputs)
    {
      if (input.Label == "ORE")
      {
        ores += input.Count * nReactionsNeeded;
        continue;
      }
      ores += Produce(input.Count * nReactionsNeeded, input.Label, available, byOutput);
    }
    available[label] = reaction.Output.Count * nReactionsNeeded - needed;
    return ores;
  }

  public record Reactant(long Count, string Label);
  public record Reaction(List<Reactant> Inputs, Reactant Output);

  private static List<Reaction> Convert(List<string> data)
  {
    var rp = P.Format("{} {}", P.Long, P.Word).Select(it => new Reactant(it.First, it.Second));
    return P.Format("{} => {}", rp.Star(","), rp)
      .Select(it => new Reaction(it.First, it.Second))
      .ParseMany(data);
  }
}
