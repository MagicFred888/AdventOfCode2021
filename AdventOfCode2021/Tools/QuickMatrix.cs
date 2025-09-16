using AdventOfCode2021.Extensions;
using System.Diagnostics;
using System.Drawing;

namespace AdventOfCode2021.Tools;

public enum TouchingMode
{
    Horizontal,
    Vertical,
    HorizontalAndVertical,
    Diagonal,
    All
}

public enum DataType
{
    String,
    Bool,
    Long,
}

public class QuickMatrix
{
    private bool[,] _touchingSearch;

    public static readonly IReadOnlyDictionary<TouchingMode, List<Point>> Directions = new Dictionary<TouchingMode, List<Point>>()
    {
        { TouchingMode.Horizontal, new() { new Point(-1, 0), new Point(1, 0) } },
        { TouchingMode.Vertical, new() { new Point(0, -1), new Point(0, 1) } },
        { TouchingMode.HorizontalAndVertical, new() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) } },
        { TouchingMode.Diagonal, new() { new Point(-1, -1), new Point(1, 1), new Point(-1, 1), new Point(1, -1) } },
        { TouchingMode.All, new() { new Point(-1, -1), new Point(0, -1), new Point(1, -1), new Point(-1, 0), new Point(1, 0), new Point(-1, 1), new Point(0, 1), new Point(1, 1) } }
    };

    private CellInfo[,] _allCellInfo;

    public int ColCount { get; private set; }

    public int RowCount { get; private set; }

    public List<List<CellInfo>> Rows { get; private set; } = [];

    public List<List<CellInfo>> Cols { get; private set; } = [];

    public List<CellInfo> Cells { get; private set; } = [];

    public QuickMatrix()
    {
        _allCellInfo = new CellInfo[0, 0];
        _touchingSearch = new bool[0, 0];
        ColCount = 0;
        RowCount = 0;
    }

    public QuickMatrix(int col, int row, string defaultValue = "")
    {
        ColCount = col;
        RowCount = row;
        _allCellInfo = new CellInfo[col, row];
        _touchingSearch = new bool[col, row];
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                _allCellInfo[x, y] = new CellInfo(x, y, defaultValue);
            }
        }

        // Compute other properties
        ComputeOtherProperties();
    }

    public QuickMatrix(int col, int row, long defaultValue)
    {
        ColCount = col;
        RowCount = row;
        _allCellInfo = new CellInfo[col, row];
        _touchingSearch = new bool[col, row];
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                _allCellInfo[x, y] = new CellInfo(x, y, defaultValue);
            }
        }

        // Compute other properties
        ComputeOtherProperties();
    }

    public QuickMatrix(List<string> rawData, string trueValue)
    {
        // Extract data
        ColCount = rawData[0].Length;
        RowCount = rawData.Count;
        _allCellInfo = new CellInfo[ColCount, RowCount];
        _touchingSearch = new bool[ColCount, RowCount];
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                _allCellInfo[x, y] = new CellInfo(x, y, rawData[y].Substring(x, 1) == trueValue);
            }
        }

        // Compute other properties
        ComputeOtherProperties();
    }

    public QuickMatrix(int col, int row, List<Point> filledCells, string filledCellsValue, string emptyCellsValue = "")
    {
        ColCount = col;
        RowCount = row;
        _allCellInfo = new CellInfo[col, row];
        _touchingSearch = new bool[col, row];
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                _allCellInfo[x, y] = new CellInfo(x, y, emptyCellsValue);
            }
        }
        foreach (Point p in filledCells)
        {
            _allCellInfo[p.X, p.Y].StringVal = filledCellsValue;
        }

        // Compute other properties
        ComputeOtherProperties();
    }

    public QuickMatrix(List<string> rawData, string separator = "", bool removeEmpty = false)
    {
        // Extract data
        _allCellInfo = new CellInfo[0, 0];
        for (int y = 0; y < rawData.Count; y++)
        {
            if (y == 0)
            {
                ColCount = separator == string.Empty ? rawData.Max(v => v.Length) : rawData.Max(v => v.Split(separator, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None).Length);
                RowCount = rawData.Count;
                _allCellInfo = new CellInfo[ColCount, RowCount];
            }
            string line = rawData[y];
            if (string.IsNullOrEmpty(separator))
            {
                // Each char in a new box
                for (int x = 0; x < line.Length; x++)
                {
                    _allCellInfo[x, y] = new(x, y, line[x].ToString());
                }
            }
            else
            {
                // Each item in a cell
                string[] values = line.Split(separator, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
                for (int x = 0; x < values.Length; x++)
                {
                    _allCellInfo[x, y] = new(x, y, values[x]);
                }
            }
        }
        _touchingSearch = new bool[ColCount, RowCount];

        // Compute other properties
        ComputeOtherProperties();
    }

    public QuickMatrix(List<CellInfo> data)
    {
        ColCount = data.Max(c => c.Position.X) + 1;
        RowCount = data.Max(c => c.Position.Y) + 1;
        _allCellInfo = new CellInfo[ColCount, RowCount];
        _touchingSearch = new bool[ColCount, RowCount];
        for (int x = 0; x < ColCount; x++)
        {
            for (int y = 0; y < RowCount; y++)
            {
                _allCellInfo[x, y] = new CellInfo(x, y, "");
            }
        }
        data.ForEach(c => _allCellInfo[c.Position.X, c.Position.Y] = c);
        ComputeOtherProperties();
    }

    public QuickMatrix Clone()
    {
        // Make a deep copy of everything
        QuickMatrix clone = new(ColCount, RowCount);
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                clone._allCellInfo[x, y] = _allCellInfo[x, y].Clone();
            }
        }
        clone.ComputeOtherProperties();
        return clone;
    }

    public void SetAllCells(object value)
    {
        foreach (CellInfo cell in Cells)
        {
            cell.ObjectVal = value;
        }
    }

    public void SetAllCells(long value)
    {
        foreach (CellInfo cell in Cells)
        {
            cell.LongVal = value;
        }
    }

    public void SetAllCells(string value)
    {
        foreach (CellInfo cell in Cells)
        {
            cell.StringVal = value;
        }
    }

    public void SetAllCells(bool value)
    {
        foreach (CellInfo cell in Cells)
        {
            cell.BoolVal = value;
        }
    }

    public void SetCells(QuickMatrix refMatrix, Point startPosition)
    {
        // Copy value from subPattern into this matrix
        refMatrix.Cells.ForEach(c => Cell(c.Position.Add(startPosition)).Set(c));
    }

    public void RotateCounterClockwise()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[height, width];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[y, width - 1 - x] = new(y, width - 1 - x, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;
        ColCount = height;
        RowCount = width;

        // Compute other properties
        ComputeOtherProperties();
    }

    public void RotateClockwise()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[height, width];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[height - 1 - y, x] = new(height - 1 - y, x, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;
        ColCount = height;
        RowCount = width;

        // Compute other properties
        ComputeOtherProperties();
    }

    public void Rotate180Degree()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[width - 1 - x, height - 1 - y] = new(width - 1 - x, height - 1 - y, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;

        // Compute other properties
        ComputeOtherProperties();
    }

    public void FlipHorizontal()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[width - 1 - x, y] = new(width - 1 - x, y, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;

        // Compute other properties
        ComputeOtherProperties();
    }

    public void FlipVertical()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[x, height - 1 - y] = new(x, height - 1 - y, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;

        // Compute other properties
        ComputeOtherProperties();
    }

    public void Transpose()
    {
        int width = _allCellInfo.GetLength(0);
        int height = _allCellInfo.GetLength(1);
        CellInfo[,] rotated = new CellInfo[height, width];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[y, x] = new(y, x, _allCellInfo[x, y]);
            }
        }
        _allCellInfo = rotated;
        ColCount = height;
        RowCount = width;

        // Compute other properties
        ComputeOtherProperties();
    }

    public CellInfo Cell(Point p) => Cell(p.X, p.Y);

    public CellInfo Cell(int x, int y)
    {
        if (x < 0 || x >= ColCount || y < 0 || y >= RowCount)
        {
            return new CellInfo(int.MinValue, int.MinValue, "");
        }
        return _allCellInfo[x, y];
    }

    public string CellStr(Point p, string valueIfNull = "") => CellStr(p.X, p.Y, valueIfNull);

    public string CellStr(int x, int y, string valueIfNull = "")
    {
        if (x < 0 || x >= ColCount || y < 0 || y >= RowCount)
        {
            return valueIfNull;
        }
        return _allCellInfo[x, y].StringVal;
    }

    private void ComputeOtherProperties()
    {
        // Create rows
        Rows = [];
        for (int y = 0; y < RowCount; y++)
        {
            List<CellInfo> row = [];
            for (int x = 0; x < ColCount; x++)
            {
                row.Add(_allCellInfo[x, y]);
            }
            Rows.Add(row);
        }

        // Create columns
        Cols = [];
        for (int x = 0; x < ColCount; x++)
        {
            List<CellInfo> col = [];
            for (int y = 0; y < RowCount; y++)
            {
                col.Add(_allCellInfo[x, y]);
            }
            Cols.Add(col);
        }

        // Create cells
        Cells = [];
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ColCount; x++)
            {
                Cells.Add(_allCellInfo[x, y]);
            }
        }
    }

    public void DebugPrint()
    {
        DebugPrint(DataType.String);
    }

    public void DebugPrint(DataType dataType)
    {
        Debug.WriteLine(string.Join("\r\n", GetDebugPrintString(dataType)));
    }

    public List<string> GetDebugPrintString(DataType dataType)
    {
        List<string> result = [];
        foreach (List<CellInfo> row in Rows)
        {
            string rowText = dataType switch
            {
                DataType.String => row.Aggregate("", (acc, cell) => acc + cell.StringVal),
                DataType.Bool => row.Aggregate("", (acc, cell) => acc + (cell.BoolVal ? "#" : " ")),
                DataType.Long => row.Aggregate("", (acc, cell) => acc + cell.LongVal.ToString()),
                _ => throw new NotImplementedException(),
            };
            result.Add(rowText);
        }
        return result;
    }

    public List<CellInfo> GetTouchingCellsWithValue(Point position, TouchingMode touchingMode)
    {
        _touchingSearch = new bool[ColCount, RowCount];
        return SearchTouchingCellsWithValue(position, touchingMode);
    }

    private List<CellInfo> SearchTouchingCellsWithValue(Point position, TouchingMode touchingMode)
    {
        // Already visited
        if (_touchingSearch[position.X, position.Y])
        {
            return [];
        }
        _touchingSearch[position.X, position.Y] = true;
        List<CellInfo> result = [Cell(position)];
        string targetValue = result[0].StringVal;
        foreach (Point move in Directions[touchingMode])
        {
            Point nextPosition = position.Add(move);
            if (Cell(nextPosition).IsValid && Cell(nextPosition).StringVal == targetValue)
            {
                result.AddRange(SearchTouchingCellsWithValue(nextPosition, touchingMode));
            }
        }
        return result;
    }

    public List<CellInfo> CellsAtManhattanDistance(Point refPosition, int manhattanDistance)
    {
        // Get all cells at manhattan distance
        return Cells.FindAll(c => Math.Abs(c.Position.X - refPosition.X) + Math.Abs(c.Position.Y - refPosition.Y) == manhattanDistance);
    }

    public List<CellInfo> GetCellsInRange(Point startPos, Point endPos)
    {
        List<CellInfo> result = [];
        for (int x = startPos.X; x <= endPos.X; x++)
        {
            result.AddRange(Cols[x].GetRange(startPos.Y, endPos.Y - startPos.Y + 1));
        }
        return result;
    }

    public QuickMatrix GetSubMatrix(Point startPos, Point endPos)
    {
        List<CellInfo> allCells = [];
        foreach (CellInfo cell in GetCellsInRange(startPos, endPos))
        {
            Point targetPos = cell.Position.Subtract(startPos);
            allCells.Add(new CellInfo(targetPos, cell));
        }
        return new(allCells);
    }

    public void SetSubMatrix(Point point, QuickMatrix newData)
    {
        foreach (CellInfo cell in newData.Cells)
        {
            Point targetPos = point.Add(cell.Position);
            _allCellInfo[targetPos.X, targetPos.Y] = new CellInfo(targetPos, cell);
        }
        ComputeOtherProperties();
    }

    public List<CellInfo> GetNeighbours(CellInfo cell, TouchingMode touchingMode = TouchingMode.All)
    {
        // Get all neighbours
        List<CellInfo> result = [];
        foreach (Point move in Directions[touchingMode])
        {
            Point nextPosition = cell.Position.Add(move);
            if (Cell(nextPosition).IsValid)
            {
                result.Add(Cell(nextPosition));
            }
        }
        return result;
    }

    public List<CellInfo> GetNeighbours(Point position, TouchingMode touchingMode = TouchingMode.All)
    {
        // Get all neighbours
        List<CellInfo> result = [];
        foreach (Point move in Directions[touchingMode])
        {
            Point nextPosition = position.Add(move);
            if (Cell(nextPosition).IsValid)
            {
                result.Add(Cell(nextPosition));
            }
        }
        return result;
    }

    public List<CellInfo> GetByFloodFill(Point position, TouchingMode touchingMode, string wall)
    {
        // floodfill using BFS like pile
        HashSet<CellInfo> added = [];
        Queue<Point> positionToReview = new();
        positionToReview.Enqueue(position);
        while (positionToReview.Count > 0)
        {
            Point currentPosition = positionToReview.Dequeue();
            if (Cell(currentPosition).IsValid && Cell(currentPosition).StringVal != wall && !added.Contains(Cell(currentPosition)))
            {
                added.Add(Cell(currentPosition));
                foreach (CellInfo neighbor in GetNeighbours(currentPosition, touchingMode))
                {
                    if (neighbor.StringVal != wall && !added.Contains(neighbor))
                    {
                        positionToReview.Enqueue(neighbor.Position);
                    }
                }
            }
        }
        return [.. added];
    }

    public override string ToString()
    {
        return string.Join("\\", Rows.Select(r => string.Join("", r.Select(c => c.StringVal))));
    }
}