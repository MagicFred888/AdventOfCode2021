namespace AdventOfCode2021.Solver;

internal partial class Day21 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Dirac Dice";

    public readonly List<int> _playersPositions = [];
    public readonly List<int> _diracDicePossibleOutcome = [];
    public readonly Dictionary<(int p1Pos, int p1Score, int p2Pos, int p2Score, int turn), (long p1Win, long p2Win)> _meoMemoization = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();

        // Simulate game until we have a winner
        int looserId = -1;
        int nbrOfRoll = 0;
        int[] playerScore = [.. Enumerable.Repeat(0, _playersPositions.Count)];
        int diceNextValue = 1;
        do
        {
            for (int playerId = 0; playerId < _playersPositions.Count; playerId++)
            {
                (int move, diceNextValue) = Get100FacesDiceSum(diceNextValue, 3);
                nbrOfRoll += 3;
                _playersPositions[playerId] = (_playersPositions[playerId] + move) % 10;
                playerScore[playerId] += _playersPositions[playerId] + 1;
                if (playerScore[playerId] >= 1000)
                {
                    looserId = 1 - playerId;
                    break;
                }
            }
        } while (looserId < 0);

        // Return looser infos
        return (nbrOfRoll * playerScore[looserId]).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();

        // Simulate all possible games
        long[] winPerPlayer = [.. Enumerable.Repeat(0, _playersPositions.Count)];
        (winPerPlayer[0], winPerPlayer[1]) = PerformSimulation((_playersPositions[0], 0, _playersPositions[1], 0, 0));

        // Give max number of win for best player
        return winPerPlayer.Max().ToString();
    }

    private (long p1Win, long p2Win) PerformSimulation((int p1Pos, int p1Score, int p2Pos, int p2Score, int turn) initalState)
    {
        // Check if already computed
        if (_meoMemoization.TryGetValue(initalState, out (long p1Win, long p2Win) value))
        {
            return value;
        }

        // Check if game over
        if (initalState.p1Score >= 21 || initalState.p2Score >= 21)
        {
            return initalState.p1Score >= 21 ? (1, 0) : (0, 1);
        }

        // Count win for each player from here
        long p1TotalWin = 0;
        long p2TotalWin = 0;
        foreach (int diceOutcome in _diracDicePossibleOutcome)
        {
            // Create new state
            int p1NewPos = initalState.p1Pos;
            int p1NewScore = initalState.p1Score;
            int p2NewPos = initalState.p2Pos;
            int p2NewScore = initalState.p2Score;
            if (initalState.turn % 2 == 0)
            {
                // P1 play
                p1NewPos = (p1NewPos + diceOutcome) % 10;
                p1NewScore += p1NewPos + 1;
            }
            else
            {
                p2NewPos = (p2NewPos + diceOutcome) % 10;
                p2NewScore += p2NewPos + 1;
            }

            // Simulate and save
            (long p1Win, long p2Win) result = PerformSimulation((p1NewPos, p1NewScore, p2NewPos, p2NewScore, initalState.turn + 1));
            p1TotalWin += result.p1Win;
            p2TotalWin += result.p2Win;
        }

        // Save et return
        _meoMemoization.Add(initalState, (p1TotalWin, p2TotalWin));
        return (p1TotalWin, p2TotalWin);
    }

    private static (int move, int diceNextValue) Get100FacesDiceSum(int diceNextValue, int nbrOfRoll)
    {
        // Perform 3 rolls
        int move = 0;
        for (int roll = 0; roll < nbrOfRoll; roll++)
        {
            move += diceNextValue;
            diceNextValue++;
            if (diceNextValue > 100)
            {
                diceNextValue = 1;
            }
        }

        // Give back vallue and dice new position
        return (move, diceNextValue);
    }

    private void ExtractData()
    {
        // Get position
        _playersPositions.Clear();
        _playersPositions.AddRange(_puzzleInput.ConvertAll(s => int.Parse(s.Split(':', StringSplitOptions.TrimEntries)[1]) - 1));

        // Get possible dirac dice outcome
        _diracDicePossibleOutcome.Clear();
        for (int roll1 = 1; roll1 <= 3; roll1++)
        {
            for (int roll2 = 1; roll2 <= 3; roll2++)
            {
                for (int roll3 = 1; roll3 <= 3; roll3++)
                {
                    _diracDicePossibleOutcome.Add(roll1 + roll2 + roll3);
                }
            }
        }
    }
}