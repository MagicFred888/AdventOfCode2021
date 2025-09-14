using System.Text;

namespace AdventOfCode2021.Solver;

internal partial class Day03 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Binary Diagnostic";

    public override string GetSolution1(bool isChallenge)
    {
        StringBuilder gammaRate = new();
        StringBuilder epsilonRate = new();
        for (int i = 0; i < _puzzleInput[0].Length; i++)
        {
            int nbrOfOnes = _puzzleInput.Count(x => x[i] == '1');
            int nbrOfZeros = _puzzleInput.Count - nbrOfOnes;
            gammaRate.Append(nbrOfOnes > nbrOfZeros ? '1' : '0');
            epsilonRate.Append(nbrOfOnes > nbrOfZeros ? '0' : '1');
        }
        return (Convert.ToInt32(gammaRate.ToString(), 2) * Convert.ToInt32(epsilonRate.ToString(), 2)).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<string> oxygenGeneratorNumbers = [.. _puzzleInput];
        List<string> co2ScrubberNumbers = [.. _puzzleInput];
        for (int i = 0; i < _puzzleInput[0].Length; i++)
        {
            if (oxygenGeneratorNumbers.Count > 1)
            {
                int nbrOfOnes = oxygenGeneratorNumbers.Count(x => x[i] == '1');
                int nbrOfZeros = oxygenGeneratorNumbers.Count - nbrOfOnes;
                oxygenGeneratorNumbers = [.. oxygenGeneratorNumbers.Where(n => (n[i] == '1' && nbrOfOnes >= nbrOfZeros) || (n[i] == '0' && nbrOfZeros > nbrOfOnes))];
            }
            if (co2ScrubberNumbers.Count > 1)
            {
                int nbrOfOnes = co2ScrubberNumbers.Count(x => x[i] == '1');
                int nbrOfZeros = co2ScrubberNumbers.Count - nbrOfOnes;
                co2ScrubberNumbers = [.. co2ScrubberNumbers.Where(n => (n[i] == '0' && nbrOfZeros <= nbrOfOnes) || (n[i] == '1' && nbrOfZeros > nbrOfOnes))];
            }
        }
        return (Convert.ToInt32(oxygenGeneratorNumbers[0], 2) * Convert.ToInt32(co2ScrubberNumbers[0], 2)).ToString();
    }
}