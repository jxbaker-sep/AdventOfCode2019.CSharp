using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Parser;

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
  [InlineData("Day18.Sample.4", 136)] // 4s
  [InlineData("Day18.Sample.5", 81)] 
  [InlineData("Day18", 4954)] // 39s
  public void Part1(string path, long expected)
  {
    var grid = Convert(AoCLoader.LoadLines(path));
    CollectKeys(grid).Should().Be(expected);
  }

  private static long CollectKeys(Dictionary<Point, char> grid)
  {
    var start = grid.Where(kv => kv.Value == '@').Select(it => it.Key).Single();
    var allKeys = grid.Values.Where(it => IsKey(it)).Aggregate(EmptyKeyset, KeysetAdd);
    var goal = KeysetCount(allKeys);
    long heuristic(Point position, Keyset keyset) { 
      return goal - KeysetCount(keyset);
      // var remainder = allKeys.Where(k3 => !k2.Contains(k3.key)).Select(it => it.position).ToList();
      // if (remainder.Count == 0) return 0;
      // if (remainder.Count == 1) return position.ManhattanDistance(remainder[0]);
      // return remainder.Pairs()
      //   .Min(pair => pair.First.ManhattanDistance(pair.Second) + Math.Min(position.ManhattanDistance(pair.First), position.ManhattanDistance(pair.Second)));
      // return remainder.Aggregate((position, 0L), (accum, n) => {return (n, accum.position.ManhattanDistance(n) + accum.Item2);}).Item2;
    }
    PriorityQueue<(long steps, Keyset keyset, Point position)> open = new((it) => it.steps + heuristic(it.position, it.keyset));
    open.Enqueue((0, EmptyKeyset, start));
    Dictionary<Point, List<(Keyset keyset, long steps)>> visited = [];
    while (open.TryDequeue(out var current))
    {
      if (KeysetCount(current.keyset) == goal) return current.steps;
      // if (visited.TryGetValue(current.position, out var cached2))
      // {
      //   if (cached2.Any(item => current.keys.All(ck => item.keys.Contains(ck)) && item.steps < current.steps)) continue;
      // }
      foreach(var (steps, key, nextPosition) in Open(grid, current.position, current.keyset)) {
        Keyset nextKeyset = KeysetAdd(current.keyset, key);
        var nextSteps = current.steps + steps;
        if (visited.TryGetValue(nextPosition, out var visits))
        {
          // Have we been here, with these keys, at the same distance or shorter?
          if (visits.Any(visit => KeysetIsSupersetOf(visit.keyset, nextKeyset) && visit.steps <= nextSteps)) continue;
          var n = visits.Where(visit => visit.steps < nextSteps || !KeysetIsSupersetOf(nextKeyset, visit.keyset)).ToList();
          n.Add((nextKeyset, nextSteps));
          visited[nextPosition] = n;
        }
        else {
          visited[nextPosition] = [(nextKeyset, nextSteps)];
        }
        open.Enqueue((nextSteps, nextKeyset, nextPosition));
      }
    }
    throw new ApplicationException();
  }

  
  private static IEnumerable<(long steps, char key, Point position)> Open(Dictionary<Point, char> grid, Point start, ulong keyset)
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
        if (IsDoor(c) && !KeysetContainsDoor(keyset, c)) continue;
        if (closed.ContainsKey(neighbor)) continue;
        closed[neighbor] = n + 1;
        open.Enqueue(neighbor);
        if (IsKey(c) && !KeysetContainsKey(keyset, c)) yield return (n+1, c, neighbor);
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
      if (((keyset & mask) == 1) && ((subset & mask) == 0)) output &= mask;
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



  private static Dictionary<Point, char> Convert(List<string> data) => data.Gridify();
}
