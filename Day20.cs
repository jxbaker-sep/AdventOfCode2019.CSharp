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
    open.Enqueue(maze.PortalExits["AA"].Single());
    var goal = maze.PortalExits["ZZ"].Single();
    Dictionary<Point, long> closed = [];
    closed[maze.PortalExits["AA"].Single()] = 0;

    while (open.TryDequeue(out var current))
    {
      var n = closed[current];
      var neighbors = current.CardinalNeighbors().ToList();
      if (maze.PortalEntrances.TryGetValue(current, out var portal)) {
        neighbors.AddRange(maze.PortalExits[portal].Where(it => it != current));
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

  public record Maze(HashSet<Point> OpenGrid, Dictionary<Point, string> PortalEntrances,
    Dictionary<string, List<Point>> PortalExits);

  private static Maze Convert(string data) {
    var grid = data.Split("\n").ToList().Gridify();

    Dictionary<Point, string> portalEntrances = [];

    HashSet<Point> openGrid = [];
    foreach(var (point, value) in grid)
    {
      if (value == '.') openGrid.Add(point);
      if (char.IsLetter(value))
      {
        foreach(var (next, entrance) in new[]{ 
            (Vector.South, Vector.South * 2), 
            (Vector.South, Vector.North),
            (Vector.East, Vector.East * 2), 
            (Vector.East, Vector.West)}) {
            if (grid.TryGetValue(point + next, out var cn) && char.IsLetter(cn) &&
                grid.TryGetValue(point + entrance, out var cn2) && cn2 == '.')
            {
              var portal = $"{value}{cn}";
              portalEntrances[point + entrance] = portal;
            }
        }
      }
    }
    Dictionary<string, List<Point>> portalExits = portalEntrances
      .GroupBy(it => it.Value, it => it.Key)
      .ToDictionary(it => it.Key, it => it.ToList());

    return new(openGrid, portalEntrances, portalExits);
  }
}
