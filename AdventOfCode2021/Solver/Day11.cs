using AdventOfCode2021.Tools;

namespace AdventOfCode2021.Solver;

internal partial class Day11 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Dumbo Octopus";

    public override string GetSolution1(bool isChallenge)
    {
        return SimulateUntilNbrOfRoundOrAllFlashAtOnce(100).TotalNbrOfFlash.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        return SimulateUntilNbrOfRoundOrAllFlashAtOnce(int.MaxValue).FirstRoundAllFlashAtOnce.ToString();
    }

    private (int TotalNbrOfFlash, int FirstRoundAllFlashAtOnce) SimulateUntilNbrOfRoundOrAllFlashAtOnce(int nbrOfRounds)
    {
        // Get data
        QuickMatrix allOctopuses = new(_puzzleInput);
        allOctopuses.Cells.ForEach(cell => cell.LongVal = long.Parse(cell.StringVal));

        // Simulate nbr of steps (and stop if all flash at once)
        int totalFlashes = 0;
        for (int i = 0; i < nbrOfRounds; i++)
        {
            // Increase energy level of all octopuses
            allOctopuses.Cells.ForEach(cell => cell.LongVal++);

            // Flash
            int flashThisRound = 0;
            List<CellInfo> flashedThisStep = [.. allOctopuses.Cells.Where(cell => cell.LongVal > 9)];
            while (flashedThisStep.Count > 0)
            {
                flashThisRound += flashedThisStep.Count;
                flashedThisStep.ForEach(c => c.LongVal = 0);
                flashedThisStep.ForEach(c => allOctopuses.GetNeighbours(c.Position, TouchingMode.All).Where(c => !flashedThisStep.Contains(c) && c.LongVal != 0).ToList().ForEach(c => c.LongVal++));
                flashedThisStep = [.. allOctopuses.Cells.Where(cell => cell.LongVal > 9)];
            }

            // Add round flash to total
            totalFlashes += flashThisRound;

            // All flashing at once ?
            if (flashThisRound == allOctopuses.Cells.Count)
            {
                return (totalFlashes, i + 1);
            }
        }

        // Done
        return (totalFlashes, -1);
    }
}