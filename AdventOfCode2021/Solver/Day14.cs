namespace AdventOfCode2021.Solver;

internal partial class Day14 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Extended Polymerization";

    private string polymerTemplate = string.Empty;
    private Dictionary<string, long> _pairCount = [];
    private readonly Dictionary<string, char> _insertionRules = [];

    public override string GetSolution1(bool isChallenge)
    {
        return ResultAfterPolymerizationCycles(10).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        return ResultAfterPolymerizationCycles(40).ToString();
    }

    private long ResultAfterPolymerizationCycles(int nbrOfCycleToPerform)
    {
        // Reset data
        ExtractData();

        // Perform cycles
        for (int cycle = 0; cycle < nbrOfCycleToPerform; cycle++)
        {
            Dictionary<string, long> newPairCount = [];
            foreach (KeyValuePair<string, long> pair in _pairCount)
            {
                if (_insertionRules.TryGetValue(pair.Key, out char value))
                {
                    string newPair1 = $"{pair.Key[0]}{value}";
                    string newPair2 = $"{value}{pair.Key[1]}";
                    newPairCount.TryAdd(newPair1, 0);
                    newPairCount[newPair1] += pair.Value;
                    newPairCount.TryAdd(newPair2, 0);
                    newPairCount[newPair2] += pair.Value;
                }
                else
                {
                    newPairCount.TryAdd(pair.Key, 0);
                    newPairCount[pair.Key] += pair.Value;
                }
            }
            _pairCount = newPairCount;
        }

        // Separate elements
        Dictionary<char, long> elementDictionary = [];
        foreach (KeyValuePair<string, long> pair in _pairCount)
        {
            elementDictionary.TryAdd(pair.Key[0], 0);
            elementDictionary[pair.Key[0]] += pair.Value;
            elementDictionary.TryAdd(pair.Key[1], 0);
            elementDictionary[pair.Key[1]] += pair.Value;
        }

        // Each element was counted twice (once as first element of a pair, once as second element of a pair), so we must divide by 2
        foreach (char key in elementDictionary.Keys)
        {
            elementDictionary[key] /= 2;
        }

        // Because first and last element at start where used only once, after division by 2 we get like 0.5 who is truncated at 0, we must add 1 to these 2 elements
        elementDictionary[polymerTemplate[0]]++;
        elementDictionary[polymerTemplate[^1]]++;

        // Get result
        return elementDictionary.Values.Max() - elementDictionary.Values.Min();
    }

    private void ExtractData()
    {
        // Reset data
        _pairCount.Clear();
        _insertionRules.Clear();

        // Extract polymer template
        polymerTemplate = _puzzleInput[0];
        for (int i = 0; i < polymerTemplate.Length - 1; i++)
        {
            _pairCount.TryAdd(polymerTemplate[i..(i + 2)], 0);
            _pairCount[polymerTemplate[i..(i + 2)]]++;
        }

        // Extract insertion rules
        for (int i = 2; i < _puzzleInput.Count; i++)
        {
            string[] parts = _puzzleInput[i].Split(" -> ");
            _insertionRules.Add(parts[0], parts[1][0]);
        }
    }
}