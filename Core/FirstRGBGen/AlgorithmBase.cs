using System.Diagnostics;

namespace AllColors.FirstRGBGen;





/// <summary>
/// Base class of algorithms.
/// </summary>
public abstract class AlgorithmBase
{
    private readonly Pixel[] _imagePixels;
    private readonly int _startIndex;

    /// <summary>
    /// Every implementation has a pixel queue.
    /// </summary>
    public abstract PixelQueue Queue { get; }

    public AlgorithmBase(Pixel[] imagePixels, int startIndex)
    {
        _imagePixels = imagePixels;
        _startIndex = startIndex;
    }

    /// <summary>
    /// Places the given color on the image.
    /// </summary>
    public void Place(ARGB c)
    {
        // find the next coordinates
        Pixel p;
        if (Queue.Count == 0)
            p = _imagePixels[_startIndex];
        else
            p = PlaceImpl(c);

        // put the pixel where it belongs
        Debug.Assert(p.IsEmpty);
        p.IsEmpty = false;
        p.Color = c;

        // adjust the queue
        ChangeQueue(p);
    }

    /// <summary>
    /// Places the given color on the image. Can assume that the queue is not empty.
    /// </summary>
    protected abstract Pixel PlaceImpl(ARGB c);

    /// <summary>
    /// Adjusts the queue after placing the given pixel.
    /// </summary>
    protected abstract void ChangeQueue(Pixel p);
}