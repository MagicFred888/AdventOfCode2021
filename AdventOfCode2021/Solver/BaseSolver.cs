using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventOfCode2021.Solver;

internal enum Part
{
    PartOne,
    PartTwo,
}

internal abstract partial class BaseSolver
{
    [GeneratedRegex(@"^Day(?<day>\d+)$")]
    private static partial Regex ExtractDayNumberRegex();

    [GeneratedRegex(@"^[a-zA-Z]*?(?<TestID>\d+)_")]
    private static partial Regex ExtractSampleIdRegex();

    private readonly int day;

    private sealed class DataSet
    {
        public required string TestFileName;
        public List<string> Data = [];
        public string[] RoundIdAnswers = new string[2];
    }

    // To be used by children class to solve challenges
    protected List<string> _puzzleInput = [];

    // For internal use only
    private readonly List<string> _challengeData = [];

    private readonly List<DataSet> _sampleDataSet = [];

    private readonly Stopwatch _stopwatch = new();

    private readonly string _splitChar = "_";

    protected BaseSolver()
    {
        // Base data folder
        this.day = int.Parse(ExtractDayNumberRegex().Match(this.GetType().Name).Groups["day"].Value);
        _challengeData.Clear();
        _sampleDataSet.Clear();
        string dataFolder = @$"..\..\..\Data\Day{day:00}\";

        // Load challenge data
        string challengeDataFilePath = $"{dataFolder}Challenge.txt";
        if (File.Exists(challengeDataFilePath)) _challengeData = [.. File.ReadAllLines(challengeDataFilePath)];

        // Load sample data
        string[] sampleDataFilePath = Directory.GetFiles(dataFolder, "Sample*.txt");
        foreach (string filePath in sampleDataFilePath)
        {
            // Skip if empty
            if (new FileInfo(filePath).Length == 0) continue;

            // Split file name
            string cleanFileName = Path.GetFileNameWithoutExtension(filePath);
            cleanFileName = cleanFileName[(cleanFileName.IndexOf(_splitChar) + 1)..];
            string[] answers = cleanFileName.Split('_');

            // Load, check and save sample
            if (answers.Length > 0)
            {
                DataSet newSet = new()
                {
                    TestFileName = Path.GetFileName(filePath),
                    Data = [.. File.ReadAllLines(filePath)]
                };
                if (answers[0] != "X") newSet.RoundIdAnswers[0] = answers[0];
                if (answers.Length >= 2 && answers[1] != "X") newSet.RoundIdAnswers[1] = answers[1];
                if (newSet.RoundIdAnswers[0] != null || newSet.RoundIdAnswers[1] != null) _sampleDataSet.Add(newSet);
            }
        }
    }

    public void PrintTitle()
    {
        // Print title
        Console.WriteLine("");
        string title = $"Day {day}: {PuzzleTitle}";
        Console.WriteLine(title);
        Console.WriteLine(new string('^', title.Length));
        Console.WriteLine("");
    }

    public bool HaveSampleData(Part firstRound)
    {
        return _sampleDataSet.Count > 0 && _sampleDataSet.Any(d => d.RoundIdAnswers[(int)firstRound] != null);
    }

    public bool SolveSample(Part roundId, out string[] resultString)
    {
        // Test sample
        bool allTestPassed = true;
        List<string> results = [];
        foreach (DataSet ds in _sampleDataSet)
        {
            // Check if must be tested
            if (ds.RoundIdAnswers[(int)roundId] == null) continue;

            // Do test
            _puzzleInput = ds.Data;
            _stopwatch.Restart();
            string answer = roundId == Part.PartOne ? GetSolution1(false) : GetSolution2(false);
            _stopwatch.Stop();
            string testId = ExtractSampleIdRegex().Match(Path.GetFileNameWithoutExtension(ds.TestFileName)).Groups["TestID"].Value;
            if (string.IsNullOrEmpty(answer))
            {
                results.Add(ColorString($"SAMPLE {testId} HAS BEEN SKIPPED", Color.BrightBlue));
            }
            else if (answer == ds.RoundIdAnswers[(int)roundId])
            {
                results.Add($"{ColorString($"SAMPLE {testId} PASSED:", Color.Green)} {ColorString(answer, Color.Yellow)} found in {GetProperUnitAndRounding(_stopwatch.Elapsed.TotalMilliseconds)}");
            }
            else
            {
                results.Add($"{ColorString($"SAMPLE {testId} FAILED:", Color.Red)} Found {ColorString(answer, Color.Yellow)} instead of {ColorString(ds.RoundIdAnswers[(int)roundId], Color.Green)} in {GetProperUnitAndRounding(_stopwatch.Elapsed.TotalMilliseconds)}");
                allTestPassed = false;
            }
        }

        // Give result
        if (results.Count == 0)
        {
            // No sample
            resultString = ["No sample data found !"];
        }
        else
        {
            // Give results
            resultString = [.. results];
        }

        // Done
        return allTestPassed;
    }

    public bool HaveChallengeData()
    {
        return _challengeData.Count > 0;
    }

    public bool SolveChallenge(Part roundId, out string resultString)
    {
        // Test sample
        _puzzleInput = _challengeData;
        if (_challengeData.Count == 0)
        {
            resultString = ColorString($"NO CHALLENGE DATA FOUND ! Please make sure you save your puzzle input into Data\\Day{day:00}\\Challenge.txt !", Color.Red);
            return false;
        }
        _stopwatch.Restart();
        string answer = roundId == Part.PartOne ? GetSolution1(true) : GetSolution2(true);
        _stopwatch.Stop();
        if (string.IsNullOrEmpty(answer))
        {
            resultString = ColorString($"CHALLENGE {(roundId == Part.PartOne ? 1 : 2)} HAS BEEN SKIPPED !", Color.BrightBlue);
        }
        else
        {
            resultString = $"{ColorString(answer, Color.Yellow)} found in {GetProperUnitAndRounding(_stopwatch.Elapsed.TotalMilliseconds)}";
        }
        return true;
    }

    public string SolveFullDay(int longestPuzzleTitle)
    {
        // Title
        string title = (PuzzleTitle + new string(' ', longestPuzzleTitle))[..longestPuzzleTitle];
        int answerLength = long.MaxValue.ToString().Length + 2;

        // If no data we just say it
        if (!HaveChallengeData())
        {
            return $"{day:00}) {title}  :  Challenge data file is missing or empty !";
        }

        // Solve day 1 and 2
        _puzzleInput = _challengeData;

        string answer1 = new string(' ', answerLength) + GetSolution1(true);
        answer1 = answer1[^answerLength..];

        string answer2 = new string(' ', answerLength) + GetSolution2(true);
        answer2 = answer2[^answerLength..];

        // Done
        return $"{day:00}) {title}  :  Part 1 => {ColorString(answer1, Color.Green)}    Part 2 => {ColorString(answer2, Color.Green)}";
    }

    private static string GetProperUnitAndRounding(double totalMilliseconds)
    {
        // Change scale
        double duration = totalMilliseconds;
        string unit = "[ms]";
        if (totalMilliseconds < 1)
        {
            duration *= 1000;
            unit = "[μs]";
        }
        else if (totalMilliseconds >= 1000)
        {
            duration /= 1000;
            unit = "[s]";
        }

        // Choose rounding
        int nbrOfDecimals = 1;
        if (duration < 100 && duration >= 10)
        {
            nbrOfDecimals = 2;
        }
        else if (duration < 10)
        {
            nbrOfDecimals = 3;
        }

        // Done
        return ColorString($"{Math.Round(duration, nbrOfDecimals)} {unit}", Color.BrightCyan);
    }

    private enum Color
    {
        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Magenta = 35,
        Cyan = 36,
        White = 37,
        BrightBlack = 90,
        BrightRed = 91,
        BrightGreen = 92,
        BrightYellow = 93,
        BrightBlue = 94,
        BrightMagenta = 95,
        BrightCyan = 96,
        BrightWhite = 97
    }

    private static string ColorString(string text, Color color)
    {
        return $"\u001b[{color:d}m{text}\u001b[0m";
    }

    public abstract string PuzzleTitle { get; }

    public virtual bool DayCompleted => true;

    public abstract string GetSolution1(bool isChallenge);

    public abstract string GetSolution2(bool isChallenge);
}