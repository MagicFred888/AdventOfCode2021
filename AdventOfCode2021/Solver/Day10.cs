namespace AdventOfCode2021.Solver;

internal partial class Day10 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Syntax Scoring";

    public override string GetSolution1(bool isChallenge)
    {
        int score = 0;
        foreach (string line in _puzzleInput)
        {
            score += CheckLine(line, out _);
        }
        return score.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<long> scores = [];
        foreach (string line in _puzzleInput)
        {
            if (CheckLine(line, out string completionString) != -1 && !string.IsNullOrEmpty(completionString))
            {
                long score = 0;
                for (int i = 0; i < completionString.Length; i++)
                {
                    score *= 5;
                    score += 1 + ")]}>".IndexOf(completionString[i]);
                }
                scores.Add(score);
            }
        }
        scores.Sort();
        return scores[scores.Count / 2].ToString(); // too low 126445911
    }

    private static int CheckLine(string lineToCheck, out string completionString)
    {
        // Initialization
        completionString = string.Empty;
        Stack<char> expectedClosing = new();
        Dictionary<char, int> _points1 = new() { { ')', 3 }, { ']', 57 }, { '}', 1197 }, { '>', 25137 } };
        Dictionary<char, char> matchingChars = new() { { '(', ')' }, { '[', ']' }, { '{', '}' }, { '<', '>' } };

        // Check line
        foreach (char c in lineToCheck)
        {
            if (matchingChars.TryGetValue(c, out char value))
            {
                // Add expected closing char
                expectedClosing.Push(value);
                continue;
            }

            if (expectedClosing.Count == 0)
            {
                return 0; // Complete
            }

            if (c != expectedClosing.Pop())
            {
                // Corrupted line
                return _points1[c];
            }
        }

        // Create completion string
        while (expectedClosing.Count > 0)
        {
            completionString += expectedClosing.Pop();
        }
        return 0;
    }
}