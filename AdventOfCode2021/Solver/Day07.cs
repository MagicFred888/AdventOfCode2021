using AdventOfCode2021.Tools;

namespace AdventOfCode2021.Solver;

internal partial class Day07 : BaseSolver
{
    public override string PuzzleTitle { get; } = "The Treachery of Whales";

    public override string GetSolution1(bool isChallenge)
    {
        return FindMinFuelConsumption(false).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        return FindMinFuelConsumption(true).ToString();
    }

    private long FindMinFuelConsumption(bool useTriangularSerie)
    {
        // Initialization
        long minFuel = long.MaxValue;
        List<int> crabSubsPosition = QuickList.ListOfInt([.. _puzzleInput[0].Split(",")]);
        int min = crabSubsPosition.Min();
        int max = crabSubsPosition.Max();

        // Check each position
        for (int pos = min; pos <= max; pos++)
        {
            long fuel = crabSubsPosition.Sum(v => useTriangularSerie ? TriangularNumbers(Math.Abs(v - pos)) : Math.Abs(v - pos));
            if (fuel < minFuel)
            {
                minFuel = fuel;
            }
        }
        return minFuel;
    }

    private static long TriangularNumbers(int distance)
    {
        // https://en.wikipedia.org/wiki/Triangular_number
        return (long)distance * (distance + 1) / 2;
    }
}