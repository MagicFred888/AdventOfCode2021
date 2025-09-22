using AdventOfCode2021.Extensions;
using System.Drawing;
using System.Text.RegularExpressions;

namespace AdventOfCode2021.Solver;

internal partial class Day17 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Trick Shot";

    private Rectangle _targetArea;

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return RunFullSimulation().highestY.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        return RunFullSimulation().maxNbrInTargetArea.ToString();
    }

    private (int maxNbrInTargetArea, int highestY) RunFullSimulation()
    {
        // Simulate all cases
        int maxNbrInTargetArea = 0;
        int globalHighestY = int.MinValue;
        for (int xVelocity = 1; xVelocity <= _targetArea.Right; xVelocity++)
        {
            for (int yVelocity = _targetArea.Bottom; yVelocity < 200; yVelocity++) // 200 is guessed
            {
                (bool passWithinTargetArea, int highestY) = SimulateTrajectory(xVelocity, yVelocity);
                if (passWithinTargetArea)
                {
                    maxNbrInTargetArea++;
                    if (highestY > globalHighestY)
                    {
                        globalHighestY = highestY;
                    }
                }
            }
        }
        return (maxNbrInTargetArea, globalHighestY);
    }

    private (bool passWithinTargetArea, int highestY) SimulateTrajectory(int xVelocity, int yVelocity)
    {
        int highestY = 0;
        Point currentPosition = new();
        Point currentVelocity = new(xVelocity, yVelocity);

        while (currentPosition.X <= _targetArea.Right && currentPosition.Y >= _targetArea.Bottom)
        {
            // Move
            currentPosition = currentPosition.Add(currentVelocity);

            // Update velocity
            currentVelocity = new(currentVelocity.X == 0 ? 0 : currentVelocity.X - Math.Sign(currentVelocity.X), currentVelocity.Y - 1);

            // Save max Y
            if (currentPosition.Y > highestY)
            {
                highestY = currentPosition.Y;
            }

            // In Area ?
            if (currentPosition.X >= _targetArea.Left && currentPosition.X <= _targetArea.Right
                && currentPosition.Y <= _targetArea.Top && currentPosition.Y >= _targetArea.Bottom)
            {
                return (true, highestY);
            }
        }

        // Not in target
        return (false, highestY);
    }

    private void ExtractData()
    {
        // Get rectange from input
        string data = KeepDigitSpaceAndDashRegex().Replace(_puzzleInput[0].Replace(".", " "), "");
        int[] values = [.. data.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse)];
        _targetArea = new(values[0], values[3], values[1] - values[0], values[2] - values[3]);
    }

    [GeneratedRegex(@"[^\d\s-]")]
    private static partial Regex KeepDigitSpaceAndDashRegex();
}