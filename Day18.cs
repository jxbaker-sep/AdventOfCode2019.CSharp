using System.Configuration;
using System.Net.Http.Headers;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.VisualBasic;
using Parser;
using Utils;
using Xunit;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day18
{

  const char Wall = '#';

  static bool IsKey(char c) => char.IsLower(c);
  static bool IsDoor(char c) => char.IsUpper(c);

  [Theory]
  [InlineData("Day18.Sample.1", 8)]
  [InlineData("Day18.Sample.2", 86)]
  [InlineData("Day18.Sample.3", 132)]
  [InlineData("Day18.Sample.4", 136)]
  [InlineData("Day18.Sample.5", 81)]
  // [InlineData("Day18", 0)]
  public void Part1(string path, long expected)
  {
    var grid = Convert(AoCLoader.LoadLines(path));
    var start = grid.First(kv => kv.Value == '@').Key;
    CollectKeys(grid, start).Should().Be(expected);
  }

  private static long CollectKeys(Dictionary<Point, char> grid, Point start)
  {
    var allKeys = grid.Where(it => IsKey(it.Value)).Select(it => (position: it.Key, key: it.Value)).ToList();
    var goal = allKeys.Count;
    long salt(Point position, HashSet<char> k2) { return goal - k2.Count; }
    PriorityQueue<(long steps, HashSet<char> keys, Point position)> open = new((it) => it.steps + salt(it.position, it.keys));
    open.Enqueue((0, [], start));
    Dictionary<string, long> closed = [];
    string CreateClosedKey(HashSet<char> hsc, Point p) => p.ToString() + hsc.OrderBy(it => it).Select(it => $"{it}").Join();
    closed[CreateClosedKey([], start)] = 0;
    while (open.TryDequeue(out var current))
    {
      if (current.keys.Count == goal) return current.steps;
      if (closed.TryGetValue(CreateClosedKey(current.keys, current.position), out var cached2) && cached2 < current.steps) continue;
      foreach(var (steps, key, nextPosition) in Open(grid, current.position, current.keys)) {
        HashSet<char> nextKeys = [..current.keys, key];
        var closedKey = CreateClosedKey(nextKeys, nextPosition);
        var nextSteps = current.steps + steps;
        if (closed.TryGetValue(closedKey, out var cached) && cached <= nextSteps) continue;
        closed[closedKey] = nextSteps;
        open.Enqueue((nextSteps, nextKeys, nextPosition));
      }
    }
    throw new ApplicationException();
  }

  private static IEnumerable<(long steps, char key, Point position)> Open(Dictionary<Point, char> grid, Point start, HashSet<char> haveKeys)
  {
    Queue<Point> open = [];
    open.Enqueue(start);
    Dictionary<Point, long> closed = [];
    closed[start] = 0;
    while (open.TryDequeue(out var current))
    {
      var n = closed[current];
      foreach(var neighbor in current.CardinalNeighbors()) {
        var c = grid[neighbor];
        if (c == Wall) continue;
        if (IsDoor(c) && !haveKeys.Contains(char.ToLower(c))) continue;
        if (closed.ContainsKey(neighbor)) continue;
        closed[neighbor] = n + 1;
        open.Enqueue(neighbor);
        if (IsKey(c) && !haveKeys.Contains(c)) yield return (n+1, c, neighbor);
      }
    }
  }


  private static Dictionary<Point, char> Convert(List<string> data) => data.Gridify();
}
