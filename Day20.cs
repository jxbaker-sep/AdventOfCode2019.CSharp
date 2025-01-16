using AdventOfCode2019.CSharp.Utils;
using FluentAssertions;


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

  public record Portal(string Label, bool IsOuter, Point OuterExit, Point InnerExit);
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

              var portal = portals.TryGetValue(label, out var ptemp) ? ptemp : new Portal(label, isOuter, Point.Zero, Point.Zero);
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
