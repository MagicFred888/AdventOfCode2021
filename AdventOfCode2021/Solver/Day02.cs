using AdventOfCode2021.Extensions;
using System.Drawing;

namespace AdventOfCode2021.Solver;

internal partial class Day02 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Dive!";

    private enum Direction
    {
        Forward,
        Down,
        Up
    }

    private readonly List<(Direction, int)> _allMoves = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        Point finalPosition = new();
        foreach ((Direction direction, int value) in _allMoves)
        {
            finalPosition = direction switch
            {
                Direction.Forward => finalPosition.Add(new(value, 0)),
                Direction.Down => finalPosition.Add(new(0, value)),
                Direction.Up => finalPosition.Add(new(0, -value)),
                _ => throw new InvalidOperationException($"Unknown direction '{direction}'")
            };
        }
        return (finalPosition.X * finalPosition.Y).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        Point finalPosition = new();
        int aim = 0;
        foreach ((Direction direction, int value) in _allMoves)
        {
            if (direction == Direction.Forward)
            {
                finalPosition = finalPosition.Add(value, value * aim);
            }
            else
            {
                aim += direction == Direction.Down ? value : -value;
            }
        }
        return (finalPosition.X * finalPosition.Y).ToString();
    }

    private void ExtractData()
    {
        _allMoves.Clear();
        foreach (string line in _puzzleInput)
        {
            string[] parts = line.ToLower().Split(' ');
            if (parts.Length == 2 && int.TryParse(parts[1], out int value))
            {
                (Direction, int) move = parts[0] switch
                {
                    "forward" => (Direction.Forward, value),
                    "down" => (Direction.Down, value),
                    "up" => (Direction.Up, value),
                    _ => throw new InvalidOperationException($"Unknown direction '{parts[0]}'")
                };
                _allMoves.Add(move);
            }
        }
    }
}