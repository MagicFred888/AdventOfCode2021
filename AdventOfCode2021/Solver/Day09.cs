using AdventOfCode2021.Tools;

namespace AdventOfCode2021.Solver;

internal partial class Day09 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Smoke Basin";

    private QuickMatrix map = new();

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();

        // Get low points
        IEnumerable<CellInfo> lowPoints = map.Cells.Where(c => map.GetNeighbours(c.Position, TouchingMode.HorizontalAndVertical).All(t => t.LongVal > c.LongVal));

        // Calculate risk level
        return lowPoints.Sum(c => c.LongVal + 1).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();

        // Get low points
        IEnumerable<CellInfo> lowPoints = map.Cells.Where(c => map.GetNeighbours(c.Position, TouchingMode.HorizontalAndVertical).All(t => t.LongVal > c.LongVal));

        // Flood fill from low points and log change between each fill
        List<long> bassinSize = [];
        foreach (CellInfo lowPoint in lowPoints)
        {
            bassinSize.Add(map.GetByFloodFill(lowPoint.Position, TouchingMode.HorizontalAndVertical, "9").Count);
        }

        // Get the 3 largest bassin and multiply their size
        bassinSize.Sort((a, b) => b.CompareTo(a));
        return bassinSize.GetRange(0, 3).Aggregate((long)1, (result, value) => result * value).ToString();
    }

    private void ExtractData()
    {
        map = new QuickMatrix(_puzzleInput);
        map.Cells.ForEach(c => c.LongVal = long.Parse(c.StringVal));
    }
}