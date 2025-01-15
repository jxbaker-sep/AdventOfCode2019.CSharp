using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Parser;
using Utils;
using Xunit.Sdk;

namespace AdventOfCode2019.CSharp;
using Keyset = ulong;

public class Day18
{
  const Keyset EmptyKeyset = 0ul;
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

  [Theory]
  [InlineData("Day18", 2334L)] // 2m 30s test run
  public void Part2(string path, long expected)
  {
    var grid = Convert(AoCLoader.LoadLines(path));
    var start = grid.Where(it => it.Value == '@').Single().Key;
    foreach (var point in start.CardinalNeighbors().Append(start))
    {
      grid[point] = Wall;
    }
    foreach (var point in start.InterCardinalNeighbors())
    {
      grid[point] = '@';
    }
    CollectKeys(grid).Should().Be(expected);
  }

  [Theory]
  [InlineData(@"#######
#a.#Cd#
##@#@##
#######
##@#@##
#cB#Ab#
#######", 8)]
  [InlineData(@"###############
#d.ABC.#.....a#
######@#@######
###############
######@#@######
#b.....#.....c#
###############", 24)]
  [InlineData(@"#############
#DcBa.#.GhKl#
#.###@#@#I###
#e#d#####j#k#
###C#@#@###J#
#fEbA.#.FgHi#
#############", 32)]
  [InlineData(@"#############
#g#f.D#..h#l#
#F###e#E###.#
#dCba@#@BcIJ#
#############
#nK.L@#@G...#
#M###N#H###.#
#o#m..#i#jk.#
#############", 72)]
  public void Sanity(string data, long expected)
  {
    var grid = data.Split("\n").ToList().Gridify();
    CollectKeys(grid).Should().Be(expected);
  }

  private static long CollectKeys(Dictionary<Point, char> grid)
  {
    var starts = grid.Where(kv => kv.Value == '@').Select(it => it.Key).ToList();
    Dictionary<(char, char), Edge> edges = [];
    foreach (var (position, c) in grid.Where(it => IsKey(it.Value)).Select(it => (it.Key, it.Value))
      .Concat(starts.Select((start, index) => (start, (char)('@' - index)))))
    {
      PopulateEdges(grid, position, c, edges);
    }
    var allKeys = grid.Values.Where(it => IsKey(it)).Aggregate(EmptyKeyset, KeysetAdd);
    var goal = KeysetCount(allKeys);
    long heuristic(Keyset keyset) => goal - KeysetCount(keyset);
    PriorityQueue<(long steps, Keyset keyset, List<char> items)> open = new((it) => it.steps + heuristic(it.keyset));
    open.Enqueue((0, EmptyKeyset, starts.Select((_, index) => (char)('@' - index)).ToList()));
    Dictionary<string, List<(Keyset keyset, long steps)>> visited = [];
    while (open.TryDequeue(out var current))
    {
      if (KeysetCount(current.keyset) == goal) return current.steps;
      for(var robot = 0; robot < starts.Count; robot++ )
      {
        var remaining = KeysetEnumerate(KeysetExcept(allKeys, current.keyset)).Select(next => (current.items[robot], next)).ToList();
        foreach (var next in remaining)
        {
          if (!edges.TryGetValue(next, out var edge)) continue;
          if (!KeysetIsSupersetOf(current.keyset, edge.KeysRequired)) continue;
          Keyset nextKeyset = current.keyset | edge.Acquired;
          var nextSteps = current.steps + edge.Steps;
          var visitKey = current.items.Select(it => $"{it}").Join();
          if (visited.TryGetValue(visitKey, out var visits))
          {
            // Have we been here, with these keys, at the same distance or shorter?
            if (visits.Any(visit => KeysetIsSupersetOf(visit.keyset, nextKeyset) && visit.steps <= nextSteps)) continue;
            var n = visits.Where(visit => visit.steps < nextSteps || !KeysetIsSupersetOf(nextKeyset, visit.keyset)).ToList();
            n.Add((nextKeyset, nextSteps));
            visited[visitKey] = n;
          }
          else
          {
            visited[visitKey] = [(nextKeyset, nextSteps)];
          }
          var nextItems = current.items.ToList();
          nextItems[robot] = edge.Second;
          open.Enqueue((nextSteps, nextKeyset, nextItems));
        }
      }
    }
    throw new ApplicationException();
  }

  public record Edge(char First, char Second, long Steps, Keyset KeysRequired, Keyset Acquired);

  private static void PopulateEdges(Dictionary<Point, char> grid, Point start, char startChar, Dictionary<(char, char), Edge> edges)
  {
    Queue<Point> open = [];
    open.Enqueue(start);
    Dictionary<Point, (long steps, Keyset keysRequired, Keyset Acquired)> closed = [];
    closed[start] = (0, EmptyKeyset, EmptyKeyset);
    while (open.TryDequeue(out var current))
    {
      var n = closed[current].steps;
      var keysRequired = closed[current].keysRequired;
      foreach (var neighbor in current.CardinalNeighbors())
      {
        var c = grid[neighbor];
        if (c == Wall) continue;
        if (closed.ContainsKey(neighbor)) continue;
        var nextRequiredKeyset = keysRequired;
        var acquired = closed[current].Acquired;
        if (IsDoor(c) && !KeysetContainsDoor(acquired, c)) nextRequiredKeyset = KeysetAdd(nextRequiredKeyset, (char)(c - 'A' + 'a'));
        if (IsKey(c))
        {
          acquired = KeysetAdd(acquired, c);
          edges[(startChar, c)] = new(startChar, c, n + 1, nextRequiredKeyset, acquired);
        }
        closed[neighbor] = (n + 1, nextRequiredKeyset, acquired);
        open.Enqueue(neighbor);
      }
    }
  }

  public static Keyset KeysetAdd(Keyset keyset, char key) => keyset | 1ul << key - 'a';
  public static bool KeysetContainsDoor(Keyset keyset, char door) => (keyset & 1ul << door - 'A') > 0;

  public static bool KeysetIsSupersetOf(Keyset keyset, Keyset subset) => (keyset & subset) == subset;
  public static bool KeysetContainsKey(Keyset keyset, char key) => (keyset & 1ul << key - 'a') > 0;

  public static Keyset KeysetExcept(Keyset keyset, Keyset subset)
  {
    Keyset output = EmptyKeyset;
    for (var n = 0; n < 32; n++)
    {
      Keyset mask = 1ul << n;
      if ((keyset & mask) > 0 && (subset & mask) == 0) output |= mask;
    }
    return output;
  }

  public static int KeysetCount(Keyset keyset)
  {
    int result = 0;
    while (keyset > 0)
    {
      if ((keyset & 1ul) == 1) result += 1;
      keyset >>= 1;
    }
    return result;
  }

  [Theory]
  [InlineData("abc", "", true)]
  [InlineData("", "", true)]
  [InlineData("abc", "a", true)]
  [InlineData("abc", "abc", true)]
  [InlineData("abc", "abcd", false)]
  [InlineData("abc", "z", false)]
  public void SupersetTest(string string1, string string2, bool expected)
  {
    var ks1 = string1.Aggregate(EmptyKeyset, KeysetAdd);
    var ks2 = string2.Aggregate(EmptyKeyset, KeysetAdd);
    KeysetIsSupersetOf(ks1, ks2).Should().Be(expected);
  }

  public static IEnumerable<char> KeysetEnumerate(Keyset keyset)
  {
    char result = 'a';
    while (keyset > 0)
    {
      if ((keyset & 1ul) == 1) yield return result;
      result++;
      keyset >>= 1;
    }
  }



  private static Dictionary<Point, char> Convert(List<string> data) => data.Gridify();
}
