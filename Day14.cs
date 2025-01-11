using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
using Xunit;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

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

    var available = new Dictionary<string, long>();

    Produce(1, "FUEL", [], byOutput).Should().Be(expected);
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
    var ores = 0L;
    foreach(var re in reaction.Inputs)
    {
      if (re.Label == "ORE")
      {
        ores += re.Count;
        continue;
      }
      ores += Produce(re.Count, re.Label, available, byOutput);
    }
    available[label] = reaction.Output.Count;
    return ores + Produce(needed, label, available, byOutput);
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
