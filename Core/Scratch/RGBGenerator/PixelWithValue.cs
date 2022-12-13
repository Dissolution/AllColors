namespace AllColors.Scratch.RGBGenerator;

/// <summary>
/// Represents a pixel with a value. Used to sort and merge parallel run results.
/// </summary>
public struct PixelWithValue : IComparable<PixelWithValue>
{
    public Pixel? Pixel;
    public int Value;

    public int CompareTo(PixelWithValue that)
    {
        // a parallel run may have no result at all, in that case we prefer the one that has a value
        if (this.Pixel == null && that.Pixel == null)
            return 0;
        if (this.Pixel == null)
            return 1;
        if (that.Pixel == null)
            return -1;

        // compare the values, or use the weight if they're equal
        var c = this.Value.CompareTo(that.Value);
        if (c == 0)
            c = this.Pixel.Weight.CompareTo(that.Pixel.Weight);
        return c;
    }
}