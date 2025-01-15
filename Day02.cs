using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2019.CSharp;
public class Day02
{
  [Theory]
  [InlineData("Day02", 9706670)]
  public void Part1(string path, long expected)
  {
    var icc = new IntCodeComputer(Convert(AoCLoader.LoadFile(path)));
    icc.Program[1] = 12;
    icc.Program[2] = 2;
    icc.Run();
    icc.Program[0].Should().Be(expected);
  }

  [Theory]
  [InlineData("Day02", 2552)]
  public void Part2(string path, int expected)
  {
    var program = Convert(AoCLoader.LoadFile(path));
    foreach(var noun in Enumerable.Range(0, 100))
    foreach(var verb in Enumerable.Range(0, 100))
    {
      var icc = new IntCodeComputer(program.ToList());
      icc.Program[1] = noun;
      icc.Program[2] = verb;
      icc.Run();
      if (icc.Program[0] == 19690720)
      {
        (100 * noun + verb).Should().Be(expected);
        return;
      }
    }
  }

  

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}
