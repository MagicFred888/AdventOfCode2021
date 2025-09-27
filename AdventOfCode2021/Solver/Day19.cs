using System.Collections.Immutable;

namespace AdventOfCode2021.Solver;

internal partial class Day19 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Beacon Scanner";

    private readonly List<Space3D> _all3dSpace = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        Space3D matchedSpace = PerformMatch();

        // Return number of beacons
        return matchedSpace.BeaconCount.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        Space3D matchedSpace = PerformMatch();

        // Get scanners
        List<Point3D> allScanners = matchedSpace.GetAllScanners();

        // Calculate max distance
        int maxDistance = 0;
        for (int i = 0; i < allScanners.Count - 1; i++)
        {
            for (int j = i + 1; j < allScanners.Count; j++)
            {
                maxDistance = Math.Max(maxDistance, allScanners[i].ManhattanDistance(allScanners[j]));
            }
        }

        // Return max distance
        return maxDistance.ToString();
    }

    private Space3D PerformMatch()
    {
        // We placed matched item in a new list
        List<Space3D> matchedSpace = [_all3dSpace[0]];
        _all3dSpace.RemoveAt(0);
        while (_all3dSpace.Count > 0)
        {
            for (int i = 0; i < matchedSpace.Count; i++)
            {
                Space3D refSpace = matchedSpace[i];
                for (int j = _all3dSpace.Count - 1; j >= 0; j--) // Iterate in reverse because we will remove some items
                {
                    if (MatchFound(refSpace, _all3dSpace[j]))
                    {
                        matchedSpace.Add(_all3dSpace[j]);
                        _all3dSpace.RemoveAt(j);
                    }
                }
            }
        }

        // Now merge all points into first one (who is 0)
        Space3D result = matchedSpace[0];
        matchedSpace.RemoveAt(0);
        foreach (Space3D space3D in matchedSpace)
        {
            result.AddSpace(space3D);
        }

        // Done
        return result;
    }

    private static bool MatchFound(Space3D refSpace, Space3D otherSpace)
    {
        // Use a hash set for O(1) membership checks
        HashSet<Point3D> refSet = [.. refSpace.GetAllBeacons()];

        // Prepare data for parallel search
        bool matched = false;
        object lockObj = new();
        var orientations = Enumerable.Range(0, 24);

        // Result
        Point3D? bestTranslation = null;
        int bestOrientationId = -1;

        // Search
        Parallel.ForEach(orientations, (orientationId, state) =>
        {
            // Exit if already found in a parallel search
            if (matched)
            {
                return;
            }

            List<Point3D> orientedOther = otherSpace.GetAllBeacons(orientationId);
            foreach (var refBeacon in refSet)
            {
                foreach (var otherBeacon in orientedOther)
                {
                    // Translation vector
                    Point3D translation = refBeacon - otherBeacon;

                    // Count matches with early exit
                    int matchCount = 0;
                    foreach (var ob in orientedOther) // Don't use LINQ for early exit
                    {
                        if (refSet.Contains(ob + translation))
                        {
                            matchCount++;
                            if (matchCount >= 12)
                            {
                                lock (lockObj)
                                {
                                    if (!matched)
                                    {
                                        matched = true;
                                        bestTranslation = translation;
                                        bestOrientationId = orientationId;
                                    }
                                }
                                state.Stop();
                                return;
                            }
                        }
                    }
                }
            }
        });

        // Return transformed space if we have a result
        if (matched && bestTranslation != null)
        {
            otherSpace.Transform(bestOrientationId, bestTranslation.Value);
            return true;
        }

        // Not found
        return false;
    }

    private void ExtractData()
    {
        _all3dSpace.Clear();
        Space3D current3dSpace = new(-1);
        for (int i = 0; i < _puzzleInput.Count; i++)
        {
            // Read line and skip if empty
            string line = _puzzleInput[i];
            if (line.Length == 0)
            {
                continue;
            }

            // Save scanner
            if (line.StartsWith("---"))
            {
                // Save previous scanner if any
                if (current3dSpace.BeaconCount != 0)
                {
                    _all3dSpace.Add(current3dSpace);
                }

                // New scanner
                int scannerId = int.Parse(line.Split(' ')[2]);
                current3dSpace = new Space3D(scannerId);
                continue;
            }

            // Read beacon and add to current scanner
            string[] parts = line.Split(',');
            current3dSpace.AddBeacon(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        // Save on-going scanner if any
        if (current3dSpace.BeaconCount != 0)
        {
            _all3dSpace.Add(current3dSpace);
        }
    }

    private sealed class Space3D
    {
        private static readonly ImmutableList<(int c1, int c2, int c3, int s1, int s2, int s3)> _allOrientations =
        [
            (0, 1, 2,  1,  1,  1),
            (0, 2, 1,  1, -1,  1),
            (0, 1, 2,  1, -1, -1),
            (0, 2, 1,  1,  1, -1),

            (0, 1, 2, -1,  1, -1),
            (0, 2, 1, -1,  1,  1),
            (0, 1, 2, -1, -1,  1),
            (0, 2, 1, -1, -1, -1),

            (1, 0, 2,  1,  1, -1),
            (1, 2, 0,  1,  1,  1),
            (1, 0, 2, -1,  1,  1),
            (1, 2, 0, -1,  1, -1),

            (1, 0, 2,  1, -1,  1),
            (1, 2, 0,  1, -1, -1),
            (1, 0, 2, -1, -1, -1),
            (1, 2, 0, -1, -1,  1),

            (2, 0, 1,  1,  1,  1),
            (2, 1, 0,  1, -1,  1),
            (2, 0, 1, -1,  1, -1),
            (2, 1, 0, -1, -1, -1),

            (2, 0, 1,  1, -1, -1),
            (2, 1, 0,  1,  1, -1),
            (2, 0, 1, -1, -1,  1),
            (2, 1, 0, -1,  1,  1),
        ];

        private readonly List<Point3D> _allBeacons = [];
        private readonly Dictionary<int, Point3D> _allScanners = [];
        private readonly List<Point3D>[] _beaconsByOrientation = new List<Point3D>[24];

        public Space3D(int id)
        {
            _allScanners.Add(id, new Point3D(0, 0, 0));
        }

        public void AddBeacon(int x, int y, int z)
        {
            _allBeacons.Add(new Point3D(x, y, z));
        }

        public void AddSpace(Space3D space3D)
        {
            // Add scanners
            foreach (var s in space3D._allScanners.Where(s => !_allScanners.ContainsKey(s.Key)))
            {
                _allScanners.Add(s.Key, s.Value);
            }

            // Add beacons (dedup)
            foreach (var b in space3D._allBeacons.Where(b => !_allBeacons.Contains(b)))
            {
                _allBeacons.Add(b);
            }
        }

        public void Transform(int orientationId, Point3D translation)
        {
            _allBeacons.Clear();
            _allBeacons.AddRange(GetAllBeacons(orientationId).Select(b => b + translation));

            foreach (var id in _allScanners.Keys.ToList())
            {
                _allScanners[id] += translation;
            }

            // Reset cache
            Array.Clear(_beaconsByOrientation, 0, _beaconsByOrientation.Length);
        }

        public List<Point3D> GetAllBeacons() => [.. _allBeacons];

        public List<Point3D> GetAllBeacons(int orientationId)
        {
            if (_beaconsByOrientation[orientationId] != null)
            {
                return _beaconsByOrientation[orientationId];
            }

            var (c1, c2, c3, s1, s2, s3) = _allOrientations[orientationId];
            var oriented = _allBeacons
                .Select(b => new Point3D(
                    GetCoord(b, c1) * s1,
                    GetCoord(b, c2) * s2,
                    GetCoord(b, c3) * s3))
                .ToList();

            _beaconsByOrientation[orientationId] = oriented;
            return oriented;
        }

        private static int GetCoord(Point3D p, int index) =>
            index switch
            {
                0 => p.X,
                1 => p.Y,
                2 => p.Z,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };

        public List<Point3D> GetAllScanners() => [.. _allScanners.Values];

        public int BeaconCount => _allBeacons.Count;
    }

    internal readonly record struct Point3D(int X, int Y, int Z)
    {
        public static Point3D operator +(Point3D a, Point3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public int ManhattanDistance(Point3D other) =>
            Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
}