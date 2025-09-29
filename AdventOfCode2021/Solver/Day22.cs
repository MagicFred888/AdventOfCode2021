using System.Text.RegularExpressions;

namespace AdventOfCode2021.Solver;

internal partial class Day22 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Reactor Reboot";

    private readonly List<RebootInstructionBox> _allRebootInstructionBoxes = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return ExecuteAllRebootInstructionAndCountOnBox(true).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        return ExecuteAllRebootInstructionAndCountOnBox(false).ToString();
    }

    private long ExecuteAllRebootInstructionAndCountOnBox(bool clipBox)
    {
        InfiniteSpace result = new();
        foreach (RebootInstructionBox rebootInstructionBox in _allRebootInstructionBoxes)
        {
            result.AddInstruction(rebootInstructionBox, clipBox);
        }
        return result.NbrOfUnit;
    }

    private void ExtractData()
    {
        _allRebootInstructionBoxes.Clear();
        foreach (string line in _puzzleInput)
        {
            bool state = line.Split(' ')[0] == "on";

            // Using regex we keep only digit and -, all others are replaced by "space"
            string sanitized = Regex.Replace(line, @"[^\d\-]", " ");
            int[] data = [.. sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse)];
            _allRebootInstructionBoxes.Add(new(state, data[0], data[1], data[2], data[3], data[4], data[5]));
        }
    }

    private sealed class InfiniteSpace()
    {
        private List<RebootInstructionBox> _rebootInstructions = [];

        public long NbrOfUnit => _rebootInstructions.Sum(r => r.NbrOfUnit);

        public void AddInstruction(RebootInstructionBox newBox, bool clipBox)
        {
            if (clipBox)
            {
                // Ignore box outside -50..50
                if (newBox.MaxCoordinate[0] < -50 || newBox.MinCoordinate[0] > 50 ||
                    newBox.MaxCoordinate[1] < -50 || newBox.MinCoordinate[1] > 50 ||
                    newBox.MaxCoordinate[2] < -50 || newBox.MinCoordinate[2] > 50)
                {
                    return;
                }

                // We clip box to -50..50
                newBox = new(
                    newBox.State,
                    Math.Max(-50, newBox.MinCoordinate[0]),
                    Math.Min(50, newBox.MaxCoordinate[0]),
                    Math.Max(-50, newBox.MinCoordinate[1]),
                    Math.Min(50, newBox.MaxCoordinate[1]),
                    Math.Max(-50, newBox.MinCoordinate[2]),
                    Math.Min(50, newBox.MaxCoordinate[2]));
            }

            // First box is simply added
            if (_rebootInstructions.Count == 0)
            {
                _rebootInstructions.Add(newBox);
                return;
            }

            // We split existing boxes by min and max of new box
            for (int axisId = 0; axisId < 3; axisId++)
            {
                List<RebootInstructionBox> newRebootInstructions = [];
                foreach (RebootInstructionBox existingBox in _rebootInstructions)
                {
                    newRebootInstructions.AddRange(existingBox.SplitBox(newBox, axisId));
                }
                _rebootInstructions = newRebootInstructions;
            }

            // We remove all boxes fully inside new box (Like making a hole)
            _ = _rebootInstructions.RemoveAll(r => r.IsFullyInside(newBox));

            // Add new big box if ON
            if (newBox.State)
            {
                _rebootInstructions.Add(newBox);
            }

            // Merge
            _rebootInstructions = Merge(_rebootInstructions);
        }

        private static List<RebootInstructionBox> Merge(List<RebootInstructionBox> rebootInstructions)
        {
            // Note:
            // Only ON boxes are kept in rebootInstructions, so all boxes have State = true
            // We have no overlap, just boxes touching each other!
            bool loopAgain;
            List<RebootInstructionBox> mergedBoxes = [.. rebootInstructions];
            do
            {
                loopAgain = false;
                for (int pos = 0; pos < mergedBoxes.Count; pos++)
                {
                    RebootInstructionBox box = mergedBoxes[pos];
                    RebootInstructionBox? canBeMerged = mergedBoxes.FirstOrDefault(b => !box.Equals(b) && CanBeMerged(box, b));
                    if (canBeMerged != null)
                    {
                        // We can merge the two boxes
                        mergedBoxes.Add(new(
                            true,
                            Math.Min(box.MinCoordinate[0], canBeMerged.MinCoordinate[0]),
                            Math.Max(box.MaxCoordinate[0], canBeMerged.MaxCoordinate[0]),
                            Math.Min(box.MinCoordinate[1], canBeMerged.MinCoordinate[1]),
                            Math.Max(box.MaxCoordinate[1], canBeMerged.MaxCoordinate[1]),
                            Math.Min(box.MinCoordinate[2], canBeMerged.MinCoordinate[2]),
                            Math.Max(box.MaxCoordinate[2], canBeMerged.MaxCoordinate[2])));
                        mergedBoxes.Remove(box);
                        mergedBoxes.Remove(canBeMerged);
                        loopAgain = true;
                    }
                }
            } while (loopAgain);
            return mergedBoxes;
        }

        private static bool CanBeMerged(RebootInstructionBox box1, RebootInstructionBox box2)
        {
            // Check if boxes can be merged along X-axis
            if (box1.MinCoordinate[1] == box2.MinCoordinate[1] &&
                box1.MaxCoordinate[1] == box2.MaxCoordinate[1] &&
                box1.MinCoordinate[2] == box2.MinCoordinate[2] &&
                box1.MaxCoordinate[2] == box2.MaxCoordinate[2] &&
                (box1.MaxCoordinate[0] + 1 == box2.MinCoordinate[0] || box2.MaxCoordinate[0] + 1 == box1.MinCoordinate[0]))
            {
                return true;
            }

            // Check if boxes can be merged along Y-axis
            if (box1.MinCoordinate[0] == box2.MinCoordinate[0] &&
                box1.MaxCoordinate[0] == box2.MaxCoordinate[0] &&
                box1.MinCoordinate[2] == box2.MinCoordinate[2] &&
                box1.MaxCoordinate[2] == box2.MaxCoordinate[2] &&
                (box1.MaxCoordinate[1] + 1 == box2.MinCoordinate[1] || box2.MaxCoordinate[1] + 1 == box1.MinCoordinate[1]))
            {
                return true;
            }

            // Check if boxes can be merged along Z-axis
            if (box1.MinCoordinate[0] == box2.MinCoordinate[0] &&
                box1.MaxCoordinate[0] == box2.MaxCoordinate[0] &&
                box1.MinCoordinate[1] == box2.MinCoordinate[1] &&
                box1.MaxCoordinate[1] == box2.MaxCoordinate[1] &&
                (box1.MaxCoordinate[2] + 1 == box2.MinCoordinate[2] || box2.MaxCoordinate[2] + 1 == box1.MinCoordinate[2]))
            {
                return true;
            }

            return false;
        }
    }

    private sealed class RebootInstructionBox(bool state, int xMin, int xMax, int yMin, int yMax, int zMin, int zMax)
    {
        public bool State { get; set; } = state;
        public int[] MinCoordinate { get; init; } = [xMin, yMin, zMin];
        public int[] MaxCoordinate { get; init; } = [xMax, yMax, zMax];

        public long NbrOfUnit
        {
            get
            {
                long sizeX = MaxCoordinate[0] - MinCoordinate[0] + 1;
                long sizeY = MaxCoordinate[1] - MinCoordinate[1] + 1;
                long sizeZ = MaxCoordinate[2] - MinCoordinate[2] + 1;
                return sizeX * sizeY * sizeZ;
            }
        }

        public IEnumerable<RebootInstructionBox> SplitBox(RebootInstructionBox cuttingBox, int axisId)
        {
            // Get info
            int minCutPos = cuttingBox.MinCoordinate[axisId];
            int maxCutPos = cuttingBox.MaxCoordinate[axisId];
            int thisMin = MinCoordinate[axisId];
            int thisMax = MaxCoordinate[axisId];

            // Skip splitting if the cut does not overlap the box
            if (maxCutPos < thisMin || minCutPos > thisMax || !this.IsOverlaping(cuttingBox))
            {
                yield return this;
                yield break;
            }

            // Split at minCut
            if (minCutPos > thisMin && minCutPos <= thisMax)
            {
                yield return new(State,
                    MinCoordinate[0],
                    axisId == 0 ? minCutPos - 1 : MaxCoordinate[0],
                    MinCoordinate[1],
                    axisId == 1 ? minCutPos - 1 : MaxCoordinate[1],
                    MinCoordinate[2],
                    axisId == 2 ? minCutPos - 1 : MaxCoordinate[2]);
                thisMin = minCutPos;
            }

            // Split at maxCut
            if (maxCutPos >= thisMin && maxCutPos < thisMax)
            {
                yield return new(State,
                    axisId == 0 ? maxCutPos + 1 : MinCoordinate[0],
                    MaxCoordinate[0],
                    axisId == 1 ? maxCutPos + 1 : MinCoordinate[1],
                    MaxCoordinate[1],
                    axisId == 2 ? maxCutPos + 1 : MinCoordinate[2],
                    MaxCoordinate[2]);
                thisMax = maxCutPos;
            }

            // Return the remaining box
            yield return new RebootInstructionBox(
                State,
                axisId == 0 ? thisMin : MinCoordinate[0],
                axisId == 0 ? thisMax : MaxCoordinate[0],
                axisId == 1 ? thisMin : MinCoordinate[1],
                axisId == 1 ? thisMax : MaxCoordinate[1],
                axisId == 2 ? thisMin : MinCoordinate[2],
                axisId == 2 ? thisMax : MaxCoordinate[2]);
        }

        private bool IsOverlaping(RebootInstructionBox cuttingBox)
        {
            return !(MaxCoordinate[0] < cuttingBox.MinCoordinate[0] || MinCoordinate[0] > cuttingBox.MaxCoordinate[0]
                  || MaxCoordinate[1] < cuttingBox.MinCoordinate[1] || MinCoordinate[1] > cuttingBox.MaxCoordinate[1]
                  || MaxCoordinate[2] < cuttingBox.MinCoordinate[2] || MinCoordinate[2] > cuttingBox.MaxCoordinate[2]);
        }

        public bool Equals(RebootInstructionBox? other)
        {
            if (other is null) return false;
            return MinCoordinate[0] == other.MinCoordinate[0] && MaxCoordinate[0] == other.MaxCoordinate[0]
                && MinCoordinate[1] == other.MinCoordinate[1] && MaxCoordinate[1] == other.MaxCoordinate[1]
                && MinCoordinate[2] == other.MinCoordinate[2] && MaxCoordinate[2] == other.MaxCoordinate[2];
        }

        public override bool Equals(object? obj) => Equals(obj as RebootInstructionBox);

        public override int GetHashCode()
        {
            HashCode hc = new();
            hc.Add(MinCoordinate[0]);
            hc.Add(MinCoordinate[1]);
            hc.Add(MinCoordinate[2]);
            hc.Add(MaxCoordinate[0]);
            hc.Add(MaxCoordinate[1]);
            hc.Add(MaxCoordinate[2]);
            return hc.ToHashCode();
        }

        public override string ToString()
        {
            return $"x={MinCoordinate[0]}..{MaxCoordinate[0]},y={MinCoordinate[1]}..{MaxCoordinate[1]},z={MinCoordinate[2]}..{MaxCoordinate[2]}";
        }

        public bool IsFullyInside(RebootInstructionBox containingBox)
        {
            return MinCoordinate[0] >= containingBox.MinCoordinate[0] && MaxCoordinate[0] <= containingBox.MaxCoordinate[0]
                && MinCoordinate[1] >= containingBox.MinCoordinate[1] && MaxCoordinate[1] <= containingBox.MaxCoordinate[1]
                && MinCoordinate[2] >= containingBox.MinCoordinate[2] && MaxCoordinate[2] <= containingBox.MaxCoordinate[2];
        }
    }
}