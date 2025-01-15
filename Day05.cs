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

  [Theory]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 0, 0)]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 1, 1)]
  [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", -01, 1)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 0, 0)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 123, 1)]
  [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", -345, 1)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    7, 999)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    8, 1000)]
  [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 
    9, 1001)]
  public void Sanity(string data, long input, long expected)
  {
    var icc = new IntCodeComputer(Convert(data));

    long actual = -1;
    icc.Run(() => input, v => {actual = v;});
    actual.Should().Be(expected);
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}
