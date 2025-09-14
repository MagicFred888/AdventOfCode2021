namespace AdventOfCode2021.Solver;

internal partial class Day08 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Seven Segment Search";

    private readonly List<(List<string> Signals, List<string> Outputs)> _entries = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return _entries.Sum(e => e.Outputs.Count(s => s.Length is 2 or 4 or 3 or 7)).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        long total = 0;
        foreach ((List<string> Signals, List<string> Outputs) in _entries)
        {
            Dictionary<string, int> segmentToDigit = ComputeSegmentToDigits(Signals);
            total += DecodeEntry(Outputs, segmentToDigit);
        }
        return total.ToString();
    }

    private static long DecodeEntry(List<string> outputs, Dictionary<string, int> segmentToDigit)
    {
        outputs = outputs.ConvertAll(s => String.Concat(s.OrderBy(c => c)));
        return long.Parse(outputs.Aggregate("", (current, s) => current + segmentToDigit[s].ToString()));
    }

    private static Dictionary<string, int> ComputeSegmentToDigits(List<string> signals)
    {
        Dictionary<string, int> segmentToDigit = [];

        // Find easy digits (only one possibility)
        segmentToDigit[signals.First(s => s.Length == 2)] = 1;
        segmentToDigit[signals.First(s => s.Length == 4)] = 4;
        segmentToDigit[signals.First(s => s.Length == 3)] = 7;
        segmentToDigit[signals.First(s => s.Length == 7)] = 8;

        // Find 9 (only 6 segments and contains all segments of 4)
        string four = segmentToDigit.First(kv => kv.Value == 4).Key;
        segmentToDigit[signals.First(s => s.Length == 6 && four.All(c => s.Contains(c)))] = 9;

        // Remove found digits from signals
        signals = [.. signals.Except(segmentToDigit.Keys)];

        // Find 0 (the only remaining 6 segments who also contain the 1)
        string one = segmentToDigit.First(kv => kv.Value == 1).Key;
        segmentToDigit[signals.First(s => s.Length == 6 && one.All(c => s.Contains(c)))] = 0;

        // Remove found digits from signals
        signals = [.. signals.Except(segmentToDigit.Keys)];

        // Find 6 (the only remaining 6 segments)
        segmentToDigit[signals.First(s => s.Length == 6)] = 6;

        // Remove found digits from signals
        signals = [.. signals.Except(segmentToDigit.Keys)];

        // Find 3 (the only remaining 5 segments who also contain the 1)
        segmentToDigit[signals.First(s => s.Length == 5 && one.All(c => s.Contains(c)))] = 3;

        // Remove found digits from signals
        signals = [.. signals.Except(segmentToDigit.Keys)];

        // Find 5 (the only remaining 5 segments who are all contained in the 6)
        string six = segmentToDigit.First(kv => kv.Value == 6).Key;
        segmentToDigit[signals.First(s => s.Length == 5 && s.All(c => six.Contains(c)))] = 5;

        // Remove found digits from signals
        signals = [.. signals.Except(segmentToDigit.Keys)];

        // The last remaining signal is the 2
        segmentToDigit[signals[0]] = 2;

        // Return normalize keys (sort characters ascending)
        return segmentToDigit.ToDictionary(kv => String.Concat(kv.Key.OrderBy(c => c)), kv => kv.Value);
    }

    private void ExtractData()
    {
        _entries.Clear();
        foreach (string line in _puzzleInput)
        {
            string[] parts = line.Split("|");
            List<string> signals = [.. parts[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)];
            List<string> outputs = [.. parts[1].Split(" ", StringSplitOptions.RemoveEmptyEntries)];
            _entries.Add((signals, outputs));
        }
    }
}