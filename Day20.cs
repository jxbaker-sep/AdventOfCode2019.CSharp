using System.Linq.Expressions;
using System.Reflection.Metadata;
using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;
using Utils;


namespace AdventOfCode2019.CSharp;
public class Day20
{
  [Theory]
  [InlineData("Day20.Sample.1", 23)]
  [InlineData("Day20.Sample.2", 58)]
  [InlineData("Day20", 570)]
  public void Part1(string path, long expected)
  {
    var maze = Convert(AoCLoader.LoadFile(path));

    Queue<Point> open = [];
    var start = maze.PortalExits.Single(it => it.Value.Label == "AA").Key;
    var goal = maze.PortalExits.Single(it => it.Value.Label == "ZZ").Key;
    open.Enqueue(start);
    Dictionary<Point, long> closed = [];
    closed[start] = 0;

    while (open.TryDequeue(out var current))
    {
      var n = closed[current];
      var neighbors = current.CardinalNeighbors().ToList();
      if (maze.PortalExits.TryGetValue(current, out var portal)) {
        if (portal.InnerExit != Point.Zero) neighbors.Add(portal.InnerExit);
        neighbors.Add(portal.OuterExit);
      }
      foreach(var next in neighbors)
      {
        if (closed.ContainsKey(next)) continue;
        if (next == goal) {
          (n + 1).Should().Be(expected);
          return;
        }
        if (maze.OpenGrid.Contains(next)) {
          closed[next] = n + 1;
          open.Enqueue(next);
        }
      }
    }
    throw new ApplicationException();
  }

  [Theory]
  [InlineData("Day20.Sample.1", 26)]
  // [InlineData("Day20", 0)]
  public void Part2(string path, long expected)
  {
    var maze = Convert(AoCLoader.LoadFile(path));
    List<NodePair> edges = [];
    foreach(var label in maze.PortalExits.Values.Distinct().Select(it => it.Label))
    {
      var nodes = CreateSimpleNodesPairs(label, maze);
      edges.AddRange(nodes);
      edges.AddRange(nodes.Where(it => it.GoingDown).Select(it => new NodePair(it.Second, it.First, false, it.Steps)));
    }
    var downMap = edges.Where(edge => edge.GoingDown).GroupToDictionary(it => it.First, it => it);
    var upMap = edges.Where(edge => edge.GoingUp).GroupToDictionary(it => it.First, it => it);

    Dictionary<(string, int), long> seen = [];
    seen[("AA", 0)] = 0;
    PriorityQueue<(string Exit, int Depth, bool GoingDown)> open = new(it => seen[(it.Exit, it.Depth)] + it.Depth);
    open.Enqueue(("AA", 0, true));
    while (open.TryDequeue(out var current))
    {
      var n = seen[(current.Exit, current.Depth)];
      if (current.Exit == "ZZ" && current.Depth == 0)
      {
        n.Should().Be(expected);
        return;
      }
      var neighbors = (current.GoingDown ? downMap : upMap)[current.Exit];
      foreach(var next in neighbors)
      {
        var nextSteps = n + next.Steps;
        var nextDepth = current.Depth + (current.GoingDown ? 1 : -1);
        if (next.GoingUp && current.Depth == 0)
        {
          if (next.Second != "ZZ") continue;
          if (seen.TryGetValue(("ZZ", 0), out var zz) && zz <= nextSteps) continue;
          seen[("ZZ", 0)] = nextSteps;
          open.Enqueue(("ZZ", 0, next.GoingDown));
          continue;
        }
        if ((next.Second == "AA" || next.Second == "ZZ") && current.Depth > 0) continue;
        if (seen.TryGetValue((next.Second, nextDepth), out var already) && already <= nextSteps) continue;
        seen[(next.Second, nextDepth)] = nextSteps;
        open.Enqueue((next.Second, nextDepth, next.GoingDown));
      }
    }
    throw new ApplicationException();
  }

  public record NodePair(string First, string Second, bool GoingDown, long Steps)
  {
    public bool GoingUp => !GoingDown;
  }

  public static IEnumerable<NodePair> CreateSimpleNodesPairs(string label, Maze maze)
  {
    var start = maze.PortalExits.Values.Single(portal => portal.Label == label).OuterExit;
    Queue<Point> open = [];
    open.Enqueue(start);
    Dictionary<Point, long> closed = [];
    closed[start] = 0;

    while (open.TryDequeue(out var current))
    {
      var n = closed[current];
      var neighbors = current.CardinalNeighbors().ToList();
      foreach(var next in neighbors)
      {
        if (closed.ContainsKey(next)) continue;
        if (maze.OpenGrid.Contains(next))
        {
          closed[next] = n + 1;
          open.Enqueue(next);
          continue;
        }
        if (maze.PortalExits.TryGetValue(next, out var portal))
        {
          closed[next] = n + 1;
          var isOuter = portal.OuterExit == next;
          if ((label == "AA" || label == "ZZ") && isOuter) continue;
          if (portal.Label == label && isOuter) continue;
          yield return new NodePair(label, portal.Label, !isOuter, n + 1);
        }
      }
    }
  }

  public record Portal(string Label, Point OuterExit, Point InnerExit);
  public record Maze(HashSet<Point> OpenGrid, Dictionary<Point, Portal> PortalExits);

  private static Maze Convert(string data) {
    var grid = data.Split("\n").ToList().Gridify();

    HashSet<Point> openGrid = [];

    Dictionary<string, Portal> portals = [];
    foreach(var (point, value) in grid)
    {
      if (value == '.') openGrid.Add(point);
      if (char.IsLetter(value))
      {
        foreach(var (next, entrance, lookDirection) in new[]{ 
            (Vector.South, Vector.South * 2, Vector.North), 
            (Vector.South, Vector.North, Vector.South),
            (Vector.East, Vector.East * 2, Vector.West), 
            (Vector.East, Vector.West, Vector.East)}) {
            if (grid.TryGetValue(point + next, out var cn) && char.IsLetter(cn) &&
                grid.TryGetValue(point + entrance, out var cn2) && cn2 == '.')
            {
              var label = $"{value}{cn}";
              var exit = point + entrance;
              var n = point + lookDirection;
              bool isOuter = false;
              while (true)
              {
                if (!grid.TryGetValue(n, out var temp))
                {
                  isOuter = true; 
                  break;
                }
                if (temp == '.' || temp == '#') break;
                n += lookDirection;
              }

              var portal = portals.TryGetValue(label, out var ptemp) ? ptemp : new Portal(label, Point.Zero, Point.Zero);
              if (isOuter) portal = portal with { OuterExit = exit };
              else portal = portal with { InnerExit = exit };
              portals[label] = portal;
            }
        }
      }
    }

    Dictionary<Point, Portal> portalExits = [];
    foreach(var portal in portals.Values)
    {
      if (portal.InnerExit == Point.Zero && (portal.Label != "AA" && portal.Label != "ZZ"))
      {
        throw new ApplicationException();
      }
      if (portal.OuterExit == Point.Zero) throw new ApplicationException();
      portalExits[portal.OuterExit] = portal;
      if (portal.Label != "AA" && portal.Label != "ZZ")
      {
        portalExits[portal.InnerExit] = portal;
      }
    }

    return new(openGrid, portalExits);
  }
}
