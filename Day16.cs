using System.Net.Http.Headers;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
using Xunit;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day16
{
  public Day16()
  {
    FFT2 = Cached.Create((int index, long phases) =>
    {
      if (phases == 0) return data[index];
      long result = 0;
      foreach (var (First, Second) in Pattern(index + 1).Take(data.Count).Zip(data.Select((_, dx) => dx)))
      {
        if (First == 0) continue;
        result += FFT2!(Second, phases - 1) * First;
      }
      return Math.Abs((result % 10));
    });
  }

  [Theory]
  [InlineData("Day16", "30550349")]
  public void Part1(string path, string expected)
  {
    var data = Convert(AoCLoader.LoadFile(path));
    FFT(data, 100).Take(8).Join().Should().Be(expected);
  }

  [Theory]
  [InlineData("12345678", 1, "48226158")]
  [InlineData("12345678", 4, "01029498")]
  [InlineData("80871224585914546619083218645595", 100, "24176176")]
  [InlineData("19617804207202209144916044189917", 100, "73745418")]
  [InlineData("69317163492948606335995924319873", 100, "52432133")]
  public void Sanity1(string input, int phases, string expected)
  {
    var data = Convert(input);
    FFT(data, phases).Take(8).Join().Should().Be(expected);
  }

  List<long> data = [];
  [Theory]
  [InlineData("12345678", 1, "48226158")]
  [InlineData("12345678", 4, "01029498")]
  [InlineData("80871224585914546619083218645595", 100, "24176176")]
  [InlineData("19617804207202209144916044189917", 100, "73745418")]
  [InlineData("69317163492948606335995924319873", 100, "52432133")]
  public void Temp(string input, int phases, string expected)
  {
    data = Convert(input);
    Enumerable.Range(0, 8).Select(it => FFT2(it, phases)).Join().Should().Be(expected);
  }

  public Func<int, long, long> FFT2 { get; }

  [Theory]
  // [InlineData("03036732577212944063491565474664", 100, "84462026")]
  [InlineData("02935109699940807407585447034323", 100, "78725270")]
  // [InlineData("03081770884921959731165446850517", 100, "53553731")]
  public void Sanity2(string input, int phases, string expected)
  {
    data = Convert(input);
    data = Enumerable.Repeat(data, 10_000).SelectMany(it => it).ToList();
    var index = System.Convert.ToInt32(input[..7]);
    var temp = data[index..];
    foreach(var i in Enumerable.Range(0, phases))
    {
      List<long> result = [];
      foreach(var xxx in Enumerable.Range(0, temp.Count))
      {
        result.Add(Math.Abs(temp[xxx..].Sum() % 10));
      }
      temp = result;
    }
    // Enumerable.Range(index, 8).Select(it => FFT2(it, phases)).Join().Should().Be(expected);
  }

  public static List<long> FFT(List<long> input, int phases)
  {
    foreach (var _ in Enumerable.Range(1, phases))
    {
      var result = new List<long>();
      var i = 1;
      foreach (var __ in input)
      {
        var next = Pattern(i++).Take(input.Count).Zip(input).Select(it => it.First * it.Second).Sum();
        next = Math.Abs(next % 10);
        result.Add(next);
      }
      input = result;
    }
    return input;
  }

  public static IEnumerable<long> Pattern(int n)
  {
    var skip = 1;
    while (true)
    {
      foreach (var i in Enumerable.Repeat(0, n).Skip(skip)) yield return i;
      skip = 0;
      foreach (var i in Enumerable.Repeat(1, n)) yield return i;
      foreach (var i in Enumerable.Repeat(0, n)) yield return i;
      foreach (var i in Enumerable.Repeat(-1, n)) yield return i;
    }
  }

  private static List<long> Convert(string data)
  {
    return P.Digit.Select(it => System.Convert.ToInt64($"{it}")).Star().Before(P.EndOfInput).Parse(data);
  }
}
