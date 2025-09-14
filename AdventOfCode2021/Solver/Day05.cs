using AdventOfCode2021.Extensions;
using AdventOfCode2021.Tools;
using System.Drawing;

namespace AdventOfCode2021.Solver;

internal partial class Day05 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Hydrothermal Venture";

    private readonly List<(Point Start, Point End)> _allHydrothermalVents = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        QuickMatrix map = GetMapFromHydrothermalVents(_allHydrothermalVents.Where(v => v.End.X == v.Start.X || v.End.Y == v.Start.Y));
        return map.Cells.Count(c => c.LongVal >= 2).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        QuickMatrix map = GetMapFromHydrothermalVents(_allHydrothermalVents);
        return map.Cells.Count(c => c.LongVal >= 2).ToString();
    }

    private static QuickMatrix GetMapFromHydrothermalVents(IEnumerable<(Point Start, Point End)> allHydrothermalVents)
    {
        // Create MAtrix with proper size
        int maxX = allHydrothermalVents.Max(v => Math.Max(v.Start.X, v.End.X));
        int maxY = allHydrothermalVents.Max(v => Math.Max(v.Start.Y, v.End.Y));
        QuickMatrix map = new(maxX + 1, maxY + 1, 0);

        // Add all requested vents
        foreach ((Point start, Point end) in allHydrothermalVents)
        {
            Point dir = new(Math.Sign(end.X - start.X), Math.Sign(end.Y - start.Y));
            Point position = start.Clone().Subtract(dir); // To be at start in first loop...
            do
            {
                position = position.Add(dir);
                map.Cell(position).LongVal++;
            } while (position != end);
        }
        return map;
    }

    private void ExtractData()
    {
        _allHydrothermalVents.Clear();
        foreach (string line in _puzzleInput)
        {
            List<int> parts = line.Replace(" -> ", ",").Split(",").ToList().ConvertAll(int.Parse);
            _allHydrothermalVents.Add((new(parts[0], parts[1]), new(parts[2], parts[3])));
        }
    }
}