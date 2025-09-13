using AdventOfCode2021.Extensions;
using System.Drawing;

namespace AdventOfCode2021.Tools;

public sealed class QuickHexGrid
{
    // Very basic functions, will be improved if other problems need more complex Hex-Grid function.
    // We use special “double-height” and “double-width” layout coordinate system for hexagons
    // https://www.redblobgames.com/grids/hexagons/#coordinates

    public enum HexGridType
    {
        DoubleHeight,
        DoubleWidth,
    }

    private readonly Dictionary<string, Point> _hexMoveDic = [];

    public Dictionary<Point, bool> HexTiles { get; private set; } = [];

    public QuickHexGrid() : this(HexGridType.DoubleHeight, 0)
    {
    }

    public QuickHexGrid(HexGridType gridType, int maxDistanceFromCenter)
    {
        if (gridType == HexGridType.DoubleHeight)
        {
            _hexMoveDic = new()
                {
                    {"n", new(0, 2)},
                    {"ne", new(1, 1)},
                    {"se", new(1, -1)},
                    {"s", new(0, -2)},
                    {"sw", new(-1, -1)},
                    {"nw", new(-1, 1)},
                };
        }
        else
        {
            _hexMoveDic = new()
                { {"e", new(2, 0)},
                  {"se", new(1, -1)},
                  {"sw", new(-1, -1)},
                  {"w", new(-2, 0)},
                  {"nw", new(-1, 1)},
                  {"ne", new(1, 1)},
                };
        }

        // Create tiles
        for (int x = -maxDistanceFromCenter; x <= maxDistanceFromCenter; x++)
        {
            for (int y = -maxDistanceFromCenter; y <= maxDistanceFromCenter; y++)
            {
                Point position = new(x, y);
                HexTiles[position] = false;
            }
        }
    }

    public void SetAllTiles(Dictionary<Point, bool> newState)
    {
        HexTiles = new(newState);
    }

    public Point GetTileCoordinate(Point startTile, string pathWithoutSeparators)
    {
        List<string> moves = GetMoves(pathWithoutSeparators);
        Point result = startTile;
        foreach (string move in moves)
        {
            result = result.Add(_hexMoveDic[move]);
        }
        return result;
    }

    private List<string> GetMoves(string path)
    {
        List<string> moves = [];
        while (path.Length > 0)
        {
            KeyValuePair<string, Point> match = _hexMoveDic.FirstOrDefault(kvp => path.StartsWith(kvp.Key));
            if (!match.Equals(default(KeyValuePair<string, Point>)))
            {
                path = path.Remove(0, match.Key.Length);
                moves.Add(match.Key);
            }
        }
        return moves;
    }

    public List<bool> GetNeighbours(Point position)
    {
        return _hexMoveDic.Aggregate(new List<bool>(), (acc, kvp) =>
        {
            Point neighbour = position.Add(kvp.Value);
            acc.Add(HexTiles.ContainsKey(neighbour) && HexTiles[neighbour]);
            return acc;
        });
    }

    public static int ComputeDistance(Point p1, Point p2)
    {
        int x = Math.Abs(p2.X - p1.X);
        int y = Math.Abs(p2.Y - p1.Y);
        return Math.Min(x, y) + ((Math.Max(x, y) - Math.Min(x, y)) / 2);
    }
}