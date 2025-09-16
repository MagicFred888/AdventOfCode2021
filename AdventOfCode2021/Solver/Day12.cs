namespace AdventOfCode2021.Solver;

internal partial class Day12 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Passage Pathing";

    private readonly Dictionary<string, List<string>> _allCavePaths = [];
    private readonly Dictionary<(string current, string visited, char twice), int> _memoizationData = new();

    public override string GetSolution1(bool isChallenge)
    {
        return CountPossiblePaths(false).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        return CountPossiblePaths(true).ToString();
    }

    private int CountPossiblePaths(bool allowOneSmallCaveTwice)
    {
        ExtractData();
        _memoizationData.Clear();
        return CountPossiblePathsDfs("start", ["start"], ' ', allowOneSmallCaveTwice);
    }

    private int CountPossiblePathsDfs(string currentCave, HashSet<string> visitedSmall, char visitedTwice, bool allowOneSmallCaveTwice)
    {
        // Check if at the end
        if (currentCave == "end")
        {
            return 1;
        }

        // Create memoization state key and check if already visited
        var stateKey = (currentCave, string.Join(",", visitedSmall.OrderBy(c => c)), visitedTwice);
        if (_memoizationData.TryGetValue(stateKey, out int cached))
        {
            return cached;
        }

        // Not visited so start at 0 from here
        int pathCount = 0;
        foreach (string nextCave in _allCavePaths[currentCave].Where(c => !c.Equals("start", StringComparison.InvariantCultureIgnoreCase)))
        {
            // Special case for small cave
            bool isSmall = char.IsLower(nextCave[0]);
            if (isSmall && visitedSmall.Contains(nextCave))
            {
                if (allowOneSmallCaveTwice && visitedTwice == ' ')
                {
                    // Only add if we are still allowed to visit a small cave twice
                    pathCount += CountPossiblePathsDfs(nextCave, visitedSmall, nextCave[0], allowOneSmallCaveTwice);
                }
                continue;
            }

            // Prepare new Hashset
            var newVisited = new HashSet<string>(visitedSmall);
            if (isSmall)
            {
                newVisited.Add(nextCave);
            }

            // Add value to pathcount
            pathCount += CountPossiblePathsDfs(nextCave, newVisited, visitedTwice, allowOneSmallCaveTwice);
        }

        // Save value we just scanned and return
        _memoizationData[stateKey] = pathCount;
        return pathCount;
    }

    private void ExtractData()
    {
        _allCavePaths.Clear();
        foreach (string line in _puzzleInput)
        {
            string[] parts = line.Split('-');
            _allCavePaths.TryAdd(parts[0], []);
            _allCavePaths[parts[0]].Add(parts[1]);
            _allCavePaths.TryAdd(parts[1], []);
            _allCavePaths[parts[1]].Add(parts[0]);
        }
    }
}