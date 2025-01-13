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
  [InlineData("Day18", 4954)]
  public void Part1(string path, long expected)
  {
    var grid = Convert(AoCLoader.LoadLines(path));
    CollectKeys(grid).Should().Be(expected);
  }

  private static long CollectKeys(Dictionary<Point, char> grid)
  {
    var robots = grid.Where(kv => kv.Value == '@').Select(it => it.Key).ToList();
    var allKeys = grid.Where(it => IsKey(it.Value)).Select(it => (position: it.Key, key: it.Value)).ToList();
    var goal = allKeys.Count;
    long heuristic(Point position, HashSet<char> k2) { 
      // return goal - k2.Count;
      var remainder = allKeys.Where(k3 => !k2.Contains(k3.key)).Select(it => it.position).ToList();
      if (remainder.Count == 0) return 0;
      if (remainder.Count == 1) return position.ManhattanDistance(remainder[0]);
      // return remainder.Pairs()
      //   .Min(pair => pair.First.ManhattanDistance(pair.Second) + Math.Min(position.ManhattanDistance(pair.First), position.ManhattanDistance(pair.Second)));
      return remainder.Aggregate((start, 0L), (accum, n) => {return (start: n, accum.start.ManhattanDistance(n) + accum.Item2);}).Item2;
    }
    PriorityQueue<(long steps, HashSet<char> keys, Point position)> open = new((it) => it.steps + heuristic(it.position, it.keys));
    open.Enqueue((0, [], start));
    Dictionary<Point, List<(HashSet<char> keys, long steps)>> visited = [];
    while (open.TryDequeue(out var current))
    {
      if (current.keys.Count == goal) return current.steps;
      if (visited.TryGetValue(current.position, out var cached2))
      {
        if (cached2.Any(item => current.keys.All(ck => item.keys.Contains(ck)) && item.steps < current.steps)) continue;
      }
      foreach(var (steps, key, nextPosition) in Open(grid, current.position, current.keys)) {
        HashSet<char> nextKeys = [..current.keys, key];
        var nextSteps = current.steps + steps;
        if (visited.TryGetValue(nextPosition, out var visits))
        {
          // Have we been here, with these keys, at the same distance or shorter?
          if (visits.Any(visit => nextKeys.All(nk => visit.keys.Contains(nk)) && visit.steps <= nextSteps)) continue;
          var n = visits.Where(visit => visit.steps < nextSteps || visit.keys.Any(ik => !nextKeys.Contains(ik))).ToList();
          n.Add((nextKeys, nextSteps));
          visited[nextPosition] = n;
        }
        else {
          visited[nextPosition] = [(nextKeys, nextSteps)];
        }
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
