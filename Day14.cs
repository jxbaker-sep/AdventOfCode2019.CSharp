using System.Net.Http.Headers;
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

    Produce(1, "FUEL", [], byOutput).Should().Be(expected);
  }

  [Theory]                    
  [InlineData("Day14.Sample.3", 82892753)]
  // [InlineData("Day14.Sample.4", 5586022)]
  // [InlineData("Day14.Sample.5", 460664)]
  // [InlineData("Day14", 0)]
  public void Part2(string path, long expected)
  {
    var reactions = Convert(AoCLoader.LoadLines(path));

    var byOutput = reactions.ToDictionary(it => it.Output.Label, it => it);

    long targetOre = 1000000000000;
    
    var result = MiscUtils.BinarySearch(1000000000, (needle) => XProduce(needle, [], byOutput).ore > targetOre) ?? throw new ApplicationException();
    var x = XProduce(result-1, [], byOutput);
    x.fuel.Should().Be(expected);
  }

  readonly Dictionary<string, (long ore, long fuel, Dictionary<string, long> available)> cache = [];
  (long ore, long fuel, Dictionary<string, long> available) XProduce(long needed, Dictionary<string, long> available, Dictionary<string, Reaction> byOutput)
  {
    string key = $"{needed} {available.OrderBy(it=>it.Key).Select(it => $"{it.Key}:{it.Value}").Join(",")}";
    if (cache.TryGetValue(key, out var cached)) return cached;
    available = available.Clone();
    if (needed == 1)
    {
      var ore = Produce(1, "FUEL", available, byOutput);
      cache[key] = (ore, 1, available);
      return (ore, 1, available);
    }
    var n1 = XProduce(needed / 2 + (needed % 2), available, byOutput);
    available = n1.available;
    var netFuel = n1.fuel;
    var netOre = n1.ore;
    var n2 = XProduce(needed - n1.fuel, available, byOutput);
    netFuel += n2.fuel;
    netOre += n2.ore;
    available = n2.available;
    // var clone = available.Clone();
    // while (Produce(1, "FUEL", clone, byOutput) == 0)
    // { // TODO: might be never hit?
    //   available = clone;
    //   netFuel += 1;
    // }
    cache[key] = (netOre, netFuel, available);
    return cache[key];
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
