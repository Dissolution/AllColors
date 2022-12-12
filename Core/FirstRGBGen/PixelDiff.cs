namespace AllColors.FirstRGBGen;

/// <summary>
/// Represents a pixel with a color difference value. Used to sort and merge parallel run results.
/// </summary>
public readonly struct PixelDiff : IComparable<PixelDiff>
{
    public readonly Pixel? Pixel;
    public readonly int Value;

    public PixelDiff(Pixel? pixel, int value)
    {
        this.Pixel = pixel;
        this.Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Special handling here, we want <c>null</c> to be last, not first.
    /// </remarks>
    public int CompareTo(PixelDiff other)
    {
        if (other.Pixel is null)
        {
            if (Pixel is null) return 0;
            return -1;
        }
        if (Pixel is null) return 1;
        
        // compare the values
        var c = Value.CompareTo(other.Value);
        if (c == 0)
        {
            // Fallback to Pos compare
            c = Pixel.Pos.CompareTo(other.Pixel.Pos);
        }
        return c;
    }
}

