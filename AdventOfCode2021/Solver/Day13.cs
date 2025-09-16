using AdventOfCode2021.Tools;

namespace AdventOfCode2021.Solver;

internal partial class Day13 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Transparent Origami";

    private QuickMatrix _sheet = new();
    private List<(char axis, int line)> _foldInstructions = new();

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        ExecuteFolds(1);
        return _sheet.Cells.Count(cell => cell.BoolVal).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        ExecuteFolds(int.MaxValue);
        _sheet.FlipHorizontal(); // Not in the puzzle but easier to read
        return string.Join("\r\n    ", _sheet.GetDebugPrintString(DataType.Bool));
    }

    private void ExecuteFolds(int nbrOfFoldsToDo)
    {
        for (int i = 0; i < Math.Min(nbrOfFoldsToDo, _foldInstructions.Count); i++)
        {
            (char axis, int line) = _foldInstructions[i];
            if (axis == 'y')
            {
                FoldUp(line);
            }
            else
            {
                FoldLeft(line);
            }
        }
    }

    private void FoldLeft(int line)
    {
        // Lazy way to implement all only one time ;-)
        _sheet.RotateCounterClockwise();
        FoldUp(line);
        _sheet.RotateClockwise();
    }

    private void FoldUp(int nbrLineUp)
    {
        QuickMatrix result = new();
        int nbrLineDown = _sheet.RowCount - nbrLineUp - 1;

        // Get a new matrix with top part
        result = _sheet.GetSubMatrix(new(0, 0), new(_sheet.ColCount - 1, nbrLineUp - 1));

        // Get bottom part and flip it
        QuickMatrix bottomSheet = _sheet.GetSubMatrix(new(0, nbrLineUp + 1), new(_sheet.ColCount - 1, _sheet.RowCount - 1));
        bottomSheet.FlipVertical();
        int verticalDiff = nbrLineUp - nbrLineDown;

        // Merge bottom with the top part
        bottomSheet.Cells.ForEach(c => result.Cell(c.Position.X, c.Position.Y + verticalDiff).BoolVal = c.BoolVal || result.Cell(c.Position.X, c.Position.Y + verticalDiff).BoolVal);

        // Update sheet
        _sheet = result;
    }

    private void ExtractData()
    {
        // Separate points and fold instructions
        int blankLineIndex = _puzzleInput.IndexOf(string.Empty);

        // Load points into a matrix
        List<CellInfo> points = _puzzleInput[..blankLineIndex].ConvertAll<CellInfo>(line =>
        {
            string[] parts = line.Split(',');
            return new(int.Parse(parts[0]), int.Parse(parts[1]), true);
        });
        _sheet = new(points);

        // Load fold instructions
        _foldInstructions = _puzzleInput[(blankLineIndex + 1)..].ConvertAll<(char axis, int line)>(line =>
        {
            string[] parts = line.Split(' ')[^1].Split('=');
            return (parts[0][0], int.Parse(parts[1]));
        });
    }
}