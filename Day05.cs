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
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9")]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1")]
  public void Sanity(string input)
  {

  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}
