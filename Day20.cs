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
  [InlineData("Day20.Sample.3", 396)]
  [InlineData("Day20", 7056)] 
  public void Part2(string path, long expected)
  {
    var maze = Convert(AoCLoader.LoadFile(path));
    List<Edge> downEdges = [];
    List<Edge> upEdges = [];
    foreach(var label in maze.PortalExits.Values.Distinct().Select(it => it.Label))
    {
      downEdges.AddRange(CreateSimpleNodesPairs(label, maze, true));
      upEdges.AddRange(CreateSimpleNodesPairs(label, maze, false));
    }
    var downMap = downEdges.GroupToDictionary(it => it.First, it => it);
    var upMap = upEdges.GroupToDictionary(it => it.First, it => it);
    var minup = downEdges.Concat(upEdges).Where(edge => edge.GoingUp).Min(edge => edge.Steps);

    Dictionary<(string, int, bool), long> seen = [];
    seen[("AA", 0, true)] = 0;
    PriorityQueue<(string Exit, int Depth, bool GoingDown, List<(string Exit, int Depth, bool GoingDown, long Steps, long)> Path)> open = new(it => seen[(it.Exit, it.Depth, it.GoingDown)] + it.Depth * minup);
    open.Enqueue(("AA", 0, true, []));
    while (open.TryDequeue(out var current))
    {
      var n = seen[(current.Exit, current.Depth, current.GoingDown)];
      if (current.Exit == "ZZ" && current.Depth == 0)
      {
        // foreach(var item in current.Path)Console.WriteLine(item);
        n.Should().Be(expected);
        return;
      }
      var neighbors = (current.GoingDown ? downMap : upMap)[current.Exit];
      foreach(var next in neighbors)
      {
        var nextSteps = n + next.Steps + 1; // 1 to go through the portal
        var nextDepth = current.Depth + (next.GoingDown ? 1 : -1);
        if (next.GoingUp && current.Depth == 0)
        {
          if (next.Second != "ZZ") continue;
          nextSteps -= 1; // -1 because we don't have to go through the portal
          if (seen.TryGetValue(("ZZ", 0, true), out var zz) && zz <= nextSteps) continue;
          seen[("ZZ", 0, true)] = nextSteps;
          open.Enqueue(("ZZ", 0, true, current.Path));
          continue;
        }
        if ((next.Second == "AA" || next.Second == "ZZ") && current.Depth > 0) continue;
        if (seen.TryGetValue((next.Second, nextDepth, next.GoingDown), out var already) && already <= nextSteps) continue;
        seen[(next.Second, nextDepth, next.GoingDown)] = nextSteps;
        open.Enqueue((next.Second, nextDepth, next.GoingDown, current.Path.Append((next.Second, nextDepth, next.GoingDown, nextSteps, next.Steps)).ToList()));
      }
    }
    throw new ApplicationException();
  }

  public record Edge(string First, string Second, bool GoingDown, long Steps)
  {
    public bool GoingUp => !GoingDown;
  }

  public static IEnumerable<Edge> CreateSimpleNodesPairs(string label, Maze maze, bool goingDown)
  {
    var start = maze.PortalExits.Values.Distinct().Single(portal => portal.Label == label).OuterExit;
    if (!goingDown) {
      start = maze.PortalExits.Values.Distinct().Single(portal => portal.Label == label).InnerExit;
      if (start == Point.Zero) yield break;
    }
    Queue<Point> open = [];
    open.Enqueue(start);
    Dictionary<Point, long> closed = [];
    closed[start] = 0;

    while (open.TryDequeue(out var current))
    {
      var n = closed[current];
      var neighbors = current.CardinalNeighbors().ToList();

      if (maze.PortalExits.TryGetValue(current, out var portal))
      {
        var isOuter = portal.OuterExit == current;
        if (portal.Label == label && ( (goingDown && isOuter) || (!goingDown && !isOuter) ))
        {
          // do nothing
        }
        else
        {
          yield return new Edge(label, portal.Label, !isOuter, n);
        }
      }

      foreach(var next in neighbors)
      {
        if (closed.ContainsKey(next)) continue;
        if (maze.OpenGrid.Contains(next))
        {
          closed[next] = n + 1;
          open.Enqueue(next);
          continue;
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
