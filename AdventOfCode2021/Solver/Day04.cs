using AdventOfCode2021.Tools;

namespace AdventOfCode2021.Solver;

internal partial class Day04 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Giant Squid";

    private List<string> _numbers = [];
    private readonly List<QuickMatrix> _boards = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        List<(int winningRound, long score)> gridQuality = _boards.ConvertAll(ScoreAtFirstBingo);
        int firstWinningRound = gridQuality.Min(q => q.winningRound);
        return gridQuality.Where(q => q.winningRound == firstWinningRound).Max(q => q.score).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        List<(int winningRound, long score)> gridQuality = _boards.ConvertAll(ScoreAtFirstBingo);
        int lastWinningRound = gridQuality.Max(q => q.winningRound);
        return gridQuality.Where(q => q.winningRound == lastWinningRound).Min(q => q.score).ToString(); // Should have only one but in case we return the smallest one
    }

    private (int winningRound, long score) ScoreAtFirstBingo(QuickMatrix board)
    {
        // Perform each round
        for (int round = 0; round < _numbers.Count; round++)
        {
            // Remove numbers if any
            string number = _numbers[round];
            board.Cells.Where(cell => cell.StringVal == number).ToList().ForEach(c => c.StringVal = "#");

            // Check for win
            if (board.Rows.Any(row => row.All(cell => cell.StringVal == "#")) ||
                board.Cols.Any(col => col.All(cell => cell.StringVal == "#")))
            {
                return (round + 1, board.Cells.Where(cell => cell.StringVal != "#").Sum(c => long.Parse(c.StringVal)) * long.Parse(number));
            }
        }

        // No win...
        return (0, 0);
    }

    private void ExtractData()
    {
        _numbers = [.. _puzzleInput[0].Split(',')];
        _boards.Clear();
        int startRow = -1;
        int nbrOfRows = -1;
        for (int i = 1; i < _puzzleInput.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(_puzzleInput[i]) || i == _puzzleInput.Count - 1)
            {
                if (startRow > 0)
                {
                    _boards.Add(new(_puzzleInput.GetRange(startRow, nbrOfRows), " ", true));
                }
                startRow = -1;
                continue;
            }
            if (startRow == -1)
            {
                nbrOfRows = _puzzleInput[i].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;
                startRow = i;
                continue;
            }
        }
    }
}