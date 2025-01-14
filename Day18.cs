using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Parser;
using Utils;

namespace AdventOfCode2018.CSharp;
using Keyset = ulong;

public class Day18
{
  const ulong EmptyKeyset = 0ul;
  const char Wall = '#';

  static bool IsKey(char c) => char.IsLower(c);
  static bool IsDoor(char c) => char.IsUpper(c);

  [Theory]
  [InlineData("Day18.Sample.1", 8)]
  [InlineData("Day18.Sample.2", 86)]
  [InlineData("Day18.Sample.3", 132)]
  [InlineData("Day18.Sample.4", 136)] // 4s, 2s
  [InlineData("Day18.Sample.5", 81)] 
  [InlineData("Day18", 4954)] // 39s, 3m, 55s; 40s w/ bitset & simple heuristic ;
  public void Part1(string path, long expected)
  {
    var grid = Convert(AoCLoader.LoadLines(path));
    CollectKeys(grid).Should().Be(expected);
  }

  private static long CollectKeys(Dictionary<Point, char> grid)
  {
    var start = grid.Where(kv => kv.Value == '@').Select(it => it.Key).Single();
    Dictionary<(char, char), Edge> edges = [];
    foreach(var (position, c) in grid.Where(it => IsKey(it.Value)).Select(it => (it.Key, it.Value)).Append((start, '@')))
    {
      PopulateEdges(grid, position, edges);
    }
    var allKeys = grid.Values.Where(it => IsKey(it)).Aggregate(EmptyKeyset, KeysetAdd);
    var goal = KeysetCount(allKeys);
    var keyToPosition = grid.Where(it => IsKey(it.Value)).ToDictionary(it => it.Value, it => it.Key);
    long heuristic(Keyset keyset) => goal - KeysetCount(keyset);
    PriorityQueue<(long steps, Keyset keyset, char item)> open = new((it) => it.steps + heuristic(it.keyset));
    open.Enqueue((0, EmptyKeyset, '@'));
    Dictionary<char, List<(Keyset keyset, long steps)>> visited = [];
    while (open.TryDequeue(out var current))
    {
      if (KeysetCount(current.keyset) == goal) return current.steps;
      // if (visited.TryGetValue(current.position, out var cached2))
      // {
      //   if (cached2.Any(item => current.keys.All(ck => item.keys.Contains(ck)) && item.steps < current.steps)) continue;
      // }
      var remaining = KeysetEnumerate(KeysetExcept(allKeys, current.keyset)).Select(next => (current.item, next)).ToList();
      foreach(var next in remaining) {
        if (!edges.TryGetValue(next, out var edge)) continue;
        if (!KeysetIsSupersetOf(current.keyset, edge.KeysRequired)) continue;
        Keyset nextKeyset = current.keyset | edge.Acquired;
        var nextSteps = current.steps + edge.Steps;
        if (visited.TryGetValue(current.item, out var visits))
        {
          // Have we been here, with these keys, at the same distance or shorter?
          if (visits.Any(visit => KeysetIsSupersetOf(visit.keyset, nextKeyset) && visit.steps <= nextSteps)) continue;
          var n = visits.Where(visit => visit.steps < nextSteps || !KeysetIsSupersetOf(nextKeyset, visit.keyset)).ToList();
          n.Add((nextKeyset, nextSteps));
          visited[current.item] = n;
        }
        else {
          visited[current.item] = [(nextKeyset, nextSteps)];
        }
        open.Enqueue((nextSteps, nextKeyset, edge.Second));
      }
    }
    throw new ApplicationException();
  }

  public record Edge(char First, char Second, long Steps, Keyset KeysRequired, Keyset Acquired);

  private static void PopulateEdges(Dictionary<Point, char> grid, Point start, Dictionary<(char,char), Edge> edges)
  {
    var startChar = grid[start];
    Queue<Point> open = [];
    open.Enqueue(start);
    Dictionary<Point, (long steps, Keyset keysRequired, Keyset Acquired)> closed = [];
    closed[start] = (0, EmptyKeyset, EmptyKeyset);
    while (open.TryDequeue(out var current))
    {
      var n = closed[current].steps;
      var keysRequired = closed[current].keysRequired;
      foreach(var neighbor in current.CardinalNeighbors()) {
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

  public static ulong KeysetAdd(ulong keyset, char key) => keyset | (1ul << (key - 'a'));
  public static bool KeysetContainsDoor(ulong keyset, char door) => (keyset & (1ul << (door - 'A'))) > 0;
  
  public static bool KeysetIsSupersetOf(ulong keyset, ulong subset) => (keyset & subset) == subset;
  public static bool KeysetContainsKey(ulong keyset, char key) => (keyset & (1ul << (key - 'a'))) > 0;

  public static ulong KeysetExcept(ulong keyset, ulong subset) {
    ulong output = EmptyKeyset;
    for(var n = 0; n < 32; n++) {
      ulong mask = 1ul << n;
      if (((keyset & mask) > 0) && ((subset & mask) == 0)) output |= mask;
    }
    return output;
  }

  public static int KeysetCount(ulong keyset) {
    int result = 0;
    while (keyset > 0) {
      if ((keyset & 1ul) == 1) result += 1;
      keyset >>= 1;
    }
    return result;
  }

  public static IEnumerable<char> KeysetEnumerate(ulong keyset) {
    char result = 'a';
    while (keyset > 0) {
      if ((keyset & 1ul) == 1) yield return result;
      result++;
      keyset >>= 1;
    }
  }



  private static Dictionary<Point, char> Convert(List<string> data) => data.Gridify();
}
