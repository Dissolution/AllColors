namespace AllColors.Thrice;

/// <summary>
/// Represents a pixel with a color difference value. Used to sort and merge parallel run results.
/// </summary>
public readonly struct CellDiff : IComparable<CellDiff>
{
    public readonly ImageCell? Cell;
    public readonly int Value;

    public CellDiff(ImageCell? cell, int value)
    {
        this.Cell = cell;
        this.Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Special handling here, we want <c>null</c> to be last, not first.
    /// </remarks>
    public int CompareTo(CellDiff other)
    {
        if (other.Cell is null)
        {
            if (Cell is null) return 0;
            return -1;
        }
        if (Cell is null) return 1;
        
        // compare the values
        return Value.CompareTo(other.Value);
    }
}

