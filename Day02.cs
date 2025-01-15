using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Parser;
using Utils;
using Xunit.Sdk;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2019.CSharp;
using Keyset = ulong;

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
  [InlineData("1,0,0,0,99", "2,0,0,0,99")]
  [InlineData("2,3,0,3,99", "2,3,0,6,99")]
  [InlineData("2,4,4,5,99,0", "2,4,4,5,99,9801")]
  [InlineData("1,1,1,4,99,5,6,0,99", "30,1,1,4,2,5,6,0,99")]
  public void Sanity1(string input, string expected)
  {
    var icc = new IntCodeComputer(Convert(input));
    icc.Run();
    icc.Program.Should().BeEquivalentTo(Convert(expected));
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);
}
