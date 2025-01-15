using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2019.CSharp;

public class Day05
{
  [Theory]
  [InlineData("Day05", 16225258)]
  public void Part1(string path, long expected)
  {
    var icc = new IntCodeComputer(Convert(AoCLoader.LoadFile(path)));
    long input = 1;
    long output = 0;
    icc.Run(() => input, v => {output=v;});
    output.Should().Be(expected);
  }

  [Theory]
  [InlineData("Day05", 2808771)]
  public void Part2(string path, long expected)
  {
    var icc = new IntCodeComputer(Convert(AoCLoader.LoadFile(path)));
    long input = 5;
    long output = 0;
    icc.Run(() => input, v => {output=v;});
    output.Should().Be(expected);
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}
