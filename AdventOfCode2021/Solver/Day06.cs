namespace AdventOfCode2021.Solver;

internal partial class Day06 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Lanternfish";

    private List<long> _lanternfishByAge = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        SimulateFishGrowth(80);
        return _lanternfishByAge.Sum().ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        SimulateFishGrowth(256);
        return _lanternfishByAge.Sum().ToString();
    }

    private void SimulateFishGrowth(int numberOfDays)
    {
        for (int day = 0; day < numberOfDays; day++)
        {
            _lanternfishByAge = [
                _lanternfishByAge[1],
                _lanternfishByAge[2],
                _lanternfishByAge[3],
                _lanternfishByAge[4],
                _lanternfishByAge[5],
                _lanternfishByAge[6],
                _lanternfishByAge[7] + _lanternfishByAge[0],
                _lanternfishByAge[8],
                _lanternfishByAge[0]
                ];
        }
    }

    private void ExtractData()
    {
        _lanternfishByAge.Clear();
        List<int> fishScan = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        for (int age = 0; age <= 8; age++)
        {
            _lanternfishByAge.Add(fishScan.Count(i => i == age));
        }
    }
}