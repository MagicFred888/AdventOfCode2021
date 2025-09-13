namespace AdventOfCode2021.Tools;

public static class QuickPattern
{
    /// <summary>
    /// Represents information about a detected repeating pattern in a dataset.
    /// </summary>
    public class PatternInfo
    {
        /// <summary>
        /// Gets the starting index of the detected pattern in the data.
        /// </summary>
        public int StartIndex { get; init; }

        /// <summary>
        /// Gets the length of the detected pattern.
        /// </summary>
        public int PatternLength { get; init; }

        /// <summary>
        /// Gets the number of times the pattern repeats in the data.
        /// </summary>
        public int PatternRepeatCount { get; init; }

        /// <summary>
        /// Gets the increase per pattern, if the pattern elements are summable.
        /// </summary>
        public long? IncreasePerPattern { get; init; }
    }

    /// <summary>
    /// Searches for a repeating pattern in a list of items, starting from a specified offset.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="data">The list of items to analyze for patterns.</param>
    /// <param name="startOffset">The index in the list where the pattern search begins.</param>
    /// <param name="minimumPatternLength">The shortest length a pattern must have to be considered valid.</param>
    /// <param name="minimumRepetitions">The minimum number of times a pattern must repeat to be considered valid.</param>
    /// <returns>
    /// A <see cref="PatternInfo"/> object containing details about the detected pattern,
    /// or null if no valid pattern is found.
    /// </returns>
    public static PatternInfo? FindPattern<T>(List<T> data, int startOffset, int minimumPatternLength, int minimumRepetitions)
    {
        // Check if array is long enough to perform a search
        if (data.Count - startOffset < minimumPatternLength * minimumRepetitions)
        {
            return null;
        }

        // Consider amount of data to ignore
        int lengthOfDataToCheck = data.Count - startOffset;

        // Search
        int maxLength = lengthOfDataToCheck / minimumRepetitions;
        for (int length = maxLength; length >= minimumPatternLength; length--)
        {
            int patternStartingPosition = FindPatternStartPosition(data, startOffset, length, minimumRepetitions);
            if (patternStartingPosition >= 0)
            {
                int startIndex = patternStartingPosition;
                return new()
                {
                    StartIndex = startIndex,
                    PatternLength = length,
                    PatternRepeatCount = (lengthOfDataToCheck - patternStartingPosition) / length,
                    IncreasePerPattern = CalculateSumIfPossible(data.GetRange(startIndex, length)),
                };
            }
        }

        // Not found
        return null;
    }

    private static int FindPatternStartPosition<T>(List<T> dataPoint, int startOffset, int length, int minimumRepetitions)
    {
        int refStart = dataPoint.Count - length;
        int startIndex = dataPoint.Count - length;
        int nbrOfRepetition = 1;

        while (startIndex - length > startOffset)
        {
            int testedStartIndex = startIndex - length;
            for (int i = 0; i < length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(dataPoint[testedStartIndex + i], dataPoint[refStart + i]))
                {
                    return nbrOfRepetition >= minimumRepetitions ? startIndex : -1;
                }
            }
            nbrOfRepetition++;
            startIndex = testedStartIndex;
        }
        return nbrOfRepetition >= minimumRepetitions ? startIndex : -1;
    }

    private static long? CalculateSumIfPossible<T>(List<T> items)
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        // Try using dynamic if it's another numeric type
        try
        {
            dynamic sum = 0;
            foreach (var item in items)
            {
                sum += item;
            }
            return (long)sum;
        }
        catch
        {
            // Not summable
            return null;
        }
    }
}