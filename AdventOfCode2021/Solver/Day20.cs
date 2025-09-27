namespace AdventOfCode2021.Solver;

internal partial class Day20 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Trench Map";

    private bool _infiniteState;
    private readonly HashSet<(int x, int y)> _image = [];
    private readonly List<bool> _enhancementAlgorithm = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        PerformEnhancementCycle(2);
        return _image.Count.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        PerformEnhancementCycle(50);
        return _image.Count.ToString();
    }

    private void PerformEnhancementCycle(int nbrOfCycle)
    {
        // Initialization
        _infiniteState = false;
        int minX = _image.Min(i => i.x);
        int maxX = _image.Max(i => i.x);
        int minY = _image.Min(i => i.y);
        int maxY = _image.Max(i => i.y);

        // Perform cycle
        for (int cycleNbr = 0; cycleNbr < nbrOfCycle; cycleNbr++)
        {
            HashSet<(int x, int y)> _newImage = [];
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    if (PixelIsOn(x, y, minX, maxX, minY, maxY))
                    {
                        _newImage.Add((x, y));
                    }
                }
            }

            // Fix bound
            minX--;
            maxX++;
            minY--;
            maxY++;

            // Change infinit color (this is the catch of this day challenge)
            _infiniteState = _enhancementAlgorithm[_infiniteState ? 0b111111111 : 0];

            // Replace image
            _image.Clear();
            _image.UnionWith(_newImage);
        }
    }

    private bool PixelIsOn(int x, int y, int minX, int maxX, int minY, int maxY)
    {
        int enhancementAlgorithmPosition = 0;
        for (int yPos = y - 1; yPos <= y + 1; yPos++)
        {
            for (int xPos = x - 1; xPos <= x + 1; xPos++)
            {
                enhancementAlgorithmPosition = enhancementAlgorithmPosition << 1;
                if (xPos < minX || xPos > maxX || yPos < minY || yPos > maxY)
                {
                    // Point is in infinit
                    enhancementAlgorithmPosition = enhancementAlgorithmPosition | (_infiniteState ? 1 : 0);
                }
                else
                {
                    // We check image
                    enhancementAlgorithmPosition = enhancementAlgorithmPosition | (_image.Contains((xPos, yPos)) ? 1 : 0);
                }
            }
        }
        return _enhancementAlgorithm[enhancementAlgorithmPosition];
    }

    private void ExtractData()
    {
        // Load enhancement algorithm
        _enhancementAlgorithm.Clear();
        foreach (char c in _puzzleInput[0])
        {
            _enhancementAlgorithm.Add(c == '#');
        }

        // Load image
        _image.Clear();
        for (int y = 1; y < _puzzleInput.Count; y++)
        {
            if (string.IsNullOrEmpty(_puzzleInput[y]))
            {
                continue;
            }
            for (int x = 0; x < _puzzleInput[y].Length; x++)
            {
                if (_puzzleInput[y][x] == '#')
                {
                    _image.Add((x, y));
                }
            }
        }
    }
}