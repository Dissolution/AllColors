namespace AllColors.FirstRGBGen;

/// <summary>
/// Represents a pixel in the big image.
/// </summary>
public sealed class Pixel : IEquatable<Pixel>
{
    public Coord Pos { get; }

    public int Weight { get; }

    /// <summary>
    /// Is this pixel empty? If so, it has no color.
    /// </summary>
    public bool IsEmpty { get; set; }

    /// <summary>
    /// Color of this pixel.
    /// </summary>
    public ARGB Color { get; set; }

    /// <summary>
    /// Index of this pixel in the queue (<see cref="PixelQueue"/>), or -1 if it's not queued.
    /// </summary>
    public int QueueIndex { get; set; }

    /// <summary>
    /// Pre-calculated array of neighbor pixels.
    /// </summary>
    public Pixel[] Neighbors { get; internal set; }

    public Pixel(int x, int y, int weight)
    {
        this.Pos = new(x, y);
        this.Weight = weight;
        this.IsEmpty = true;
        this.Color = default;
        this.QueueIndex = -1;
        this.Neighbors = Array.Empty<Pixel>();
    }

    public bool Equals(Pixel? pixel)
    {
        return pixel is not null && pixel.Pos == this.Pos;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pixel pixel && pixel.Pos == this.Pos;
    }

    public override int GetHashCode()
    {
        return Weight;
    }

    public override string ToString()
    {
        if (IsEmpty)
            return $"{Pos}: [EMPTY]";
        return $"{Pos}: {Color}";
    }
}