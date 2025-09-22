namespace AdventOfCode2021.Solver;

internal partial class Day18 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Snailfish";

    public override string GetSolution1(bool isChallenge)
    {
        // Sum all snailfish numbers
        Snailfish result = new(_puzzleInput[0]);
        for (int i = 1; i < _puzzleInput.Count; i++)
        {
            Snailfish next = new(_puzzleInput[i]);
            result = new Snailfish(result, next);
        }
        return result.Magnitude.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        // Find the largest magnitude of any sum of two different snailfish numbers
        long largestMagnitude = long.MinValue;
        for (int i = 0; i < _puzzleInput.Count; i++)
        {
            for (int j = 0; j < _puzzleInput.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }
                long magnitude = new Snailfish(new Snailfish(_puzzleInput[i]), new Snailfish(_puzzleInput[j])).Magnitude;
                if (magnitude > largestMagnitude)
                {
                    largestMagnitude = magnitude;
                }
            }
        }
        return largestMagnitude.ToString();
    }

    private sealed class Snailfish
    {
        private Snailfish? parent;
        public object _leftItem;
        public object _rightItem;

        public long Magnitude =>
            3 * (_leftItem is int leftValue ? leftValue : ((Snailfish)_leftItem).Magnitude)
            + 2 * (_rightItem is int rightValue ? rightValue : ((Snailfish)_rightItem).Magnitude);

        public Snailfish(string snailfish, Snailfish? parent = null)
        {
            // Create from string
            (_leftItem, _rightItem) = SplitString(snailfish);
            this.parent = parent;
        }

        public Snailfish(Snailfish snailfish1, Snailfish snailfish2)
        {
            // Create from addding two existing snailfish pairs
            _leftItem = snailfish1;
            snailfish1.parent = this;
            _rightItem = snailfish2;
            snailfish2.parent = this;

            // Reduce the new pair
            Reduce();
        }

        private void Reduce()
        {
            // Search for exploding pairs and explode them as long as possible
            bool loopAgain;
            do
            {
                // Explode as many pairs as possible
                loopAgain = false;
                Snailfish? pairToExplode = GetPairToExplode(1);
                while (pairToExplode != null)
                {
                    ExplodePair(pairToExplode);
                    pairToExplode = GetPairToExplode(1);
                }

                // Try splitting pairs and if ok, back to exploding
                Snailfish? pairToSplit = GetPairToSplit();
                if (pairToSplit != null)
                {
                    SplitPair(pairToSplit);
                    loopAgain = true;
                }
            } while (loopAgain);
        }

        private static void SplitPair(Snailfish pairToSplit)
        {
            // Split the first regular number >= 10
            if (pairToSplit._leftItem is int leftValue && leftValue >= 10)
            {
                int leftSplit1 = leftValue / 2;
                int leftSplit2 = (leftValue + 1) / 2;
                pairToSplit._leftItem = new Snailfish($"[{leftSplit1},{leftSplit2}]", pairToSplit);
                return;
            }
            if (pairToSplit._rightItem is int rightValue && rightValue >= 10)
            {
                int rightSplit1 = rightValue / 2;
                int rightSplit2 = (rightValue + 1) / 2;
                pairToSplit._rightItem = new Snailfish($"[{rightSplit1},{rightSplit2}]", pairToSplit);
            }
        }

        private Snailfish? GetPairToSplit()
        {
            // Search in left items
            if (_leftItem is int leftPair && leftPair >= 10)
            {
                return this;
            }
            if (_leftItem is Snailfish leftPairObj)
            {
                Snailfish? result = leftPairObj.GetPairToSplit();
                if (result != null)
                {
                    return result;
                }
            }

            // Search in right items
            if (_rightItem is int rightPair && rightPair >= 10)
            {
                return this;
            }
            if (_rightItem is Snailfish rightPairObj)
            {
                Snailfish? result = rightPairObj.GetPairToSplit();
                if (result != null)
                {
                    return result;
                }
            }

            // Not found
            return null;
        }

        private static void ExplodePair(Snailfish pairToExplode)
        {
            // Sanity check
            ArgumentNullException.ThrowIfNull(pairToExplode.parent);

            // Add left value to the first regular number to the left of the exploding pair (if any)
            (Snailfish pair, bool isLeft)? leftPair = pairToExplode.GetItemWithCloserLeft();
            if (leftPair != null)
            {
                if (leftPair.Value.isLeft)
                {
                    leftPair.Value.pair._leftItem = (int)leftPair.Value.pair._leftItem + (int)pairToExplode._leftItem;
                }
                else
                {
                    leftPair.Value.pair._rightItem = (int)leftPair.Value.pair._rightItem + (int)pairToExplode._leftItem;
                }
            }

            // Add right value to the first regular number to the right of the exploding pair (if any)
            (Snailfish pair, bool isLeft)? rightPair = pairToExplode.GetItemWithCloserRight();
            if (rightPair != null)
            {
                if (rightPair.Value.isLeft)
                {
                    rightPair.Value.pair._leftItem = (int)rightPair.Value.pair._leftItem + (int)pairToExplode._rightItem;
                }
                else
                {
                    rightPair.Value.pair._rightItem = (int)rightPair.Value.pair._rightItem + (int)pairToExplode._rightItem;
                }
            }

            // Replace the exploding pair with 0 in its parent
            if (pairToExplode.parent._leftItem == pairToExplode)
            {
                pairToExplode.parent._leftItem = 0;
                return;
            }
            if (pairToExplode.parent._rightItem == pairToExplode)
            {
                pairToExplode.parent._rightItem = 0;
                return;
            }

            // Should never happen
            throw new InvalidOperationException("Exploding pair has no parent");
        }

        private (Snailfish pair, bool isLeft)? GetItemWithCloserLeft()
        {
            Snailfish current = this;
            Snailfish? parentOfCurrent = parent;

            // Moving up until we come from a left child
            while (parentOfCurrent != null && parentOfCurrent._leftItem == current)
            {
                current = parentOfCurrent;
                parentOfCurrent = parentOfCurrent.parent;
            }
            if (parentOfCurrent == null)
            {
                return null; // No left neighbor
            }

            // Moving back down
            object item = parentOfCurrent._leftItem;
            if (item is int)
            {
                return (parentOfCurrent, true);
            }
            Snailfish snailfishItem = (Snailfish)item;
            while (true)
            {
                if (snailfishItem._rightItem is int)
                {
                    return (snailfishItem, false);
                }
                snailfishItem = (Snailfish)snailfishItem._rightItem;
            }
        }

        private (Snailfish pair, bool isLeft)? GetItemWithCloserRight()
        {
            Snailfish current = this;
            Snailfish? parentOfCurrent = parent;

            // Moving up until we come from a right child
            while (parentOfCurrent != null && parentOfCurrent._rightItem == current)
            {
                current = parentOfCurrent;
                parentOfCurrent = parentOfCurrent.parent;
            }
            if (parentOfCurrent == null)
            {
                return null; // No right neighbor
            }

            // Moving back down
            object item = parentOfCurrent._rightItem;
            if (item is int)
            {
                return (parentOfCurrent, false);
            }
            Snailfish snailfishItem = (Snailfish)item;
            while (true)
            {
                if (snailfishItem._leftItem is int)
                {
                    return (snailfishItem, true);
                }
                snailfishItem = (Snailfish)snailfishItem._leftItem;
            }
        }

        public Snailfish? GetPairToExplode(int currentLevel)
        {
            // Found it?
            if (currentLevel > 4 && _leftItem is int && _rightItem is int)
            {
                return this;
            }

            // Search in left items
            if (_leftItem is Snailfish leftPair)
            {
                Snailfish? result = leftPair.GetPairToExplode(currentLevel + 1);
                if (result != null)
                {
                    return result;
                }
            }

            // Search in right items
            if (_rightItem is Snailfish rightPair)
            {
                Snailfish? result = rightPair.GetPairToExplode(currentLevel + 1);
                if (result != null)
                {
                    return result;
                }
            }

            // Not found
            return null;
        }

        private (object _leftItem, object _rightItem) SplitString(string snailfish)
        {
            // Split a snailfish string into its two items
            int nbrOpen = 0;
            for (int i = 1; i < snailfish.Length - 1; i++)
            {
                if (snailfish[i] == '[')
                {
                    nbrOpen++;
                }
                if (snailfish[i] == ']')
                {
                    nbrOpen--;
                }
                if (snailfish[i] == ',' && nbrOpen == 0)
                {
                    string left = snailfish[1..i];
                    string right = snailfish[(i + 1)..^1];
                    object leftItem = int.TryParse(left, out int leftValue) ? leftValue : new Snailfish(left, this);
                    object rightItem = int.TryParse(right, out int rightValue) ? rightValue : new Snailfish(right, this);
                    return (leftItem, rightItem);
                }
            }
            throw new ArgumentException("Invalid snailfish string");
        }

        public override string ToString()
        {
            // Convert to string (used for debugging)
            if (_leftItem == null || _rightItem == null)
            {
                return "Snailfish pair is not fully initialized";
            }
            string leftStr = _leftItem is int leftInt ? leftInt.ToString() : _leftItem?.ToString() ?? string.Empty;
            string rightStr = _rightItem is int rightInt ? rightInt.ToString() : _rightItem?.ToString() ?? string.Empty;
            return $"[{leftStr},{rightStr}]";
        }
    }
}