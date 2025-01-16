using System.Net;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.Z3;
using Parser;
using P = Parser.ParserBuiltins;


namespace AdventOfCode2019.CSharp;
public class Day19
{
  [Theory]
  [InlineData("Day19", 203)]
  public void Part1(string path, long expected)
  {
    var program = Convert(AoCLoader.LoadFile(path));

    LineLengths(program).Select(it => it.Item1).Take(50).Sum().Should().Be(expected);
  }

  static long GetPosition(List<long> program, long x, long y)
  {
    var icc = new IntCodeComputer(program);
    Queue<long> input = [];
    input.Enqueue(x);
    input.Enqueue(y);
    long output = -1;
    icc.Run(() => input.Dequeue(), v => output=v);
    return output;
  }

  [Theory]
  [InlineData("Day19", 8771057L)]
  public void Part2(string path, long expected)
  {
    var program = Convert(AoCLoader.LoadFile(path));
    foreach(var (length, first) in LineLengths(program))
    {
      if (first.Y < 99) continue;
      if (length < 100) continue;
      if (GetPosition(program, first.X + 99, first.Y - 99) == 1)
      {
        (first.X * 10000 + first.Y - 99).Should().Be(expected);
        return;
      }
    }
  }

  static IEnumerable<(long, Point)> LineLengths(List<long> program) {
    yield return (1, new(0,0));
    yield return (0, new(1,0));
    yield return (0, new(2,0));
    yield return (0, new(3,0));
    yield return (0, new(4,0));
    var first = new Point(4, 4);
    var last = new Point(4, 4);
    for(var y = 5; y < 1_000_000; y++) {
      first += Vector.South;
      last += Vector.South;
      if (GetPosition(program, first.X, first.Y) != 1) {
        first += Vector.East;
      }
      last += Vector.East;
      if (GetPosition(program, last.X, last.Y) != 1) {
        last -= Vector.East;
      }
      yield return (last.X + 1 - first.X, first);
    }
  }

  private static List<long> Convert(string data) => P.Long.Plus(",").Parse(data);

}
