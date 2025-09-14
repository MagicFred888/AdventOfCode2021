namespace AdventOfCode2021.Solver;

internal partial class Day01 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Sonar Sweep";

    private List<int> data = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();

        int nbrDecrease = 0;
        for (int i = 1; i < data.Count; i++)
        {
            nbrDecrease += data[i] > data[i - 1] ? 1 : 0;
        }

        return nbrDecrease.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();

        int nbrDecrease = 0;
        for (int i = 3; i < data.Count; i++)
        {
            nbrDecrease += data[i] > data[i - 3] ? 1 : 0; // No need to add numbers who are same in both side, so we just check first of previous and current
        }

        return nbrDecrease.ToString();
    }

    private void ExtractData()
    {
        data = _puzzleInput.ConvertAll(int.Parse);
    }
}