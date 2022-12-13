namespace AllColors.Scratch.ThriceGen;

/// <summary>
/// Represents a pixel in the big image.
/// </summary>
public class Pixel
{
    /// <summary>
    /// Is this pixel empty? If so, it has no color.
    /// </summary>
    public bool Empty;

    //public uint QueueScore;

    public bool inQueue;

    public byte nonEmptyNeigh = 0;
    public int block;
    //public int indexInBlock;


    public ARGB avg;
    /// <summary>
    /// Color of this pixel.
    /// </summary>
    public ARGB Color;

    /// <summary>
    /// Index of this pixel in the queue (<see cref="PixelList"/>), or -1 if it's not queued.
    /// </summary>
    public int QueueIndex {
        get { return 0; }
        set {}
    }

    /// <summary>
    /// Precalculated array of neighbor pixels.
    /// </summary>
    public Pixel[] Neighbors;

    /// <summary>
    /// A unique weight of thix pixel. Used in comparisons when calculated values are equal. Needs to be randomly distributed, otherwise the
    /// picture cannot grow equally in every direction.
    /// </summary>
    public int Weight;

#if DEBUG
    // we don't need to know these, just for debugging
    public int X;
    public int Y;
    public override string ToString()
    {
        return X + ";" + Y;
    }
#endif
}