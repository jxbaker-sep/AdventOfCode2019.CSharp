using System.Net;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.VisualBasic;
using Microsoft.Z3;
using Parser;
using Utils;
using P = Parser.ParserBuiltins;


namespace AdventOfCode2019.CSharp;
public class Day21
{
  [Theory]
  [InlineData("Day21", 19356971)]
  public void Part1(string path, long expected)
  {
    var program = Convert(AoCLoader.LoadFile(path));

    var icc = new IntCodeComputer(program);

    var input = new[]{
      "OR A J", 
      "AND B J",
      "AND C J",
      "NOT J J",
      "AND D J",
      "WALK\n"}.Join("\n").ToCharArray().ToQueue();
    List<long> output = [];

    icc.Run(() => input.Dequeue(), i => output.Add(i));

    output.Last().Should().Be(expected);

    // Console.WriteLine(output.Select(it => {if (it > 255) throw new ApplicationException(); return (char) it;})
    //   .Join());
  }

  [Theory]
  [InlineData("Day21", 1142600034)]
  public void Part2(string path, long expected)
  {
    var program = Convert(AoCLoader.LoadFile(path));

    var icc = new IntCodeComputer(program);

    // var input = new[]{
    //   "OR A J", 
    //   "AND B J",
    //   "AND C J",
    //   "NOT J J",
    //   "OR D T",
    //   "AND E T",
    //   "AND T J",
    //   "NOT A T",
    //   "OR T J",
    //   "RUN\n"}.Join("\n").ToCharArray().ToQueue();
    var input = new[]{
      "OR A J", 
      "AND B J",
      "AND C J",
      "NOT J J",
      "OR E T",
      "AND I T",
      "OR H T",
      "AND D T",
      "AND T J",
      "NOT A T",
      "OR T J",
      "RUN\n"}.Join("\n").ToCharArray().ToQueue();
    List<long> output = [];

    icc.Run(() => input.Dequeue(), i => output.Add(i));

    Console.WriteLine(output.Select(it => {if (it > 255) throw new ApplicationException(); return (char) it;})
      .Join());
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);

}

// #####.###########
// #####.##.########