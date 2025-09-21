using AdventOfCode2021.Tools;
using System.Drawing;

namespace AdventOfCode2021.Solver;

internal partial class Day15 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Chiton";

    private QuickMatrix _chitonCavern = new();

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData(false);
        return GetSmallestPath(new(0, 0), new(_chitonCavern.ColCount - 1, _chitonCavern.RowCount - 1)).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData(true);
        return GetSmallestPath(new(0, 0), new(_chitonCavern.ColCount - 1, _chitonCavern.RowCount - 1)).ToString();
    }

    private long GetSmallestPath(Point start, Point end)
    {
        // Initialize position dictionary
        Dictionary<Point, long> bestDistances = [];
        _chitonCavern.Cells.ForEach(c => bestDistances[c.Position] = long.MaxValue);

        // Create priority queue based on score (small values to be evaluated first)
        PriorityQueue<Point, long> priorityQueue = new();
        bestDistances[start] = 0;
        priorityQueue.Enqueue(start, 0);

        // Check all positions
        while (priorityQueue.Count > 0)
        {
            // Get current position & distance
            Point currentPosition = priorityQueue.Dequeue();
            long currentDistance = bestDistances[currentPosition];

            // Arrived
            if (currentPosition == end)
            {
                return currentDistance;
            }

            // Scan around
            foreach (var neighbor in _chitonCavern.GetNeighbours(currentPosition, TouchingMode.HorizontalAndVertical))
            {
                long newDistance = currentDistance + neighbor.LongVal;
                if (newDistance < bestDistances[neighbor.Position])
                {
                    bestDistances[neighbor.Position] = newDistance;
                    priorityQueue.Enqueue(neighbor.Position, newDistance);
                }
            }
        }

        // Return value stored in end position (mean no result)
        return bestDistances[end];
    }

    private void ExtractData(bool multiplyFieldByFive)
    {
        // Base matrix
        _chitonCavern = new(_puzzleInput);
        _chitonCavern.Cells.ForEach(c => c.LongVal = long.Parse(c.StringVal));

        // End ?
        if (!multiplyFieldByFive)
        {
            return;
        }

        // Create big cave map
        QuickMatrix bigChitonCavern = new QuickMatrix(5 * _chitonCavern.ColCount, 5 * _chitonCavern.RowCount, 0);
        for (int matrixX = 0; matrixX < 5; matrixX++)
        {
            for (int matrixY = 0; matrixY < 5; matrixY++)
            {
                for (int x = 0; x < _chitonCavern.ColCount; x++)
                {
                    for (int y = 0; y < _chitonCavern.RowCount; y++)
                    {
                        Point bigCavernPosition = new(x + (matrixX * _chitonCavern.ColCount), y + (matrixY * _chitonCavern.RowCount));
                        long value = _chitonCavern.Cell(x, y).LongVal + (matrixX + matrixY);
                        if (value > 9)
                        {
                            value = (value % 10) + 1;
                        }
                        bigChitonCavern.Cell(bigCavernPosition).LongVal = value;
                    }
                }
            }
        }

        // replace cave
        _chitonCavern = bigChitonCavern;
    }
}