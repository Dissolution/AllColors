using System.Buffers;
using System.Diagnostics;

namespace AllColors.FirstRGBGen;

/// <summary>
/// Represents a pixel queue. It's a blend of <see cref="List{T}"/> and <see cref="Dictionary{Tk,Tv}"/> functionality. It allows very quick,
/// indexed traversal (it just exposes a simple array). It supports O(1) lookups (every pixel contains it's own index in this array). Adding
/// and removal are also O(1) because we don't usually reallocate the array.
/// </summary>
public sealed class PixelQueue
{
    private Pixel?[] _pixels;
    private int _endIndex;
    private int _count;

    public Pixel?[] AvailablePixels
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pixels;
    }

    public int AvailablePixelLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _endIndex;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public PixelQueue(int capacity = 4096)
    {
        _pixels = ArrayPool<Pixel?>.Shared.Rent(capacity);
        _endIndex = 0;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        var newArray = ArrayPool<Pixel?>.Shared.Rent(_pixels.Length * 2);
        int n = 0;
        Pixel?[] pixels = _pixels;
        for (var i = 0; i < _endIndex; i++)
        {
            Pixel? pixel = pixels[i];
            if (pixel is null) continue;

            pixel.QueueIndex = n;
            newArray[n++] = pixel;
        }

        ArrayPool<Pixel?>.Shared.Return(pixels);
        _pixels = newArray;
        _endIndex = n;
        Debug.Assert(_count == _endIndex);
    }

    public void Compact()
    {
        if (_count == _endIndex) return;

//#if RUNTESTS
        double waste = ((double)_endIndex / _count);
        if (waste < 1.05d)
            return;

        Debug.Write($"PixelQueue Compact: {_endIndex} => ");
//#endif
        Pixel?[] pixels = _pixels;

        int freeIndex = 0;

        for (var i = 0; i < _endIndex; i++)
        {
            Pixel? pixel = pixels[i];
            if (pixel is null) continue;

            if (pixel.QueueIndex != freeIndex)
            {
                pixel.QueueIndex = freeIndex;
                pixels[freeIndex] = pixel;
            }

            freeIndex++;
        }

#if RUNTESTS
        Debug.Assert(freeIndex == _count);
        Debug.WriteLine($"{freeIndex}");
#endif

        _endIndex = freeIndex;
    }

    public void BadCompact()
    {
        // we allow at most 5% to be wasted
        if ((double)_endIndex / _count < 1.05d)
            return;
        _endIndex = 0;
        for (var i = 0; _endIndex < _count; i++)
        {
            if (_pixels[i] != null)
            {
                Pixel pixel = _pixels[i]!;
                TryRemove(pixel);
                TryAdd(pixel);
            }
        }

        Debug.Assert(_endIndex >= _count);
    }

    public bool TryAdd(Pixel pixel)
    {
        if (pixel.QueueIndex != -1) return false;
        int i = _endIndex;
        if (i == _pixels.Length) Grow();
        pixel.QueueIndex = i;
        _pixels[i] = pixel;
        _endIndex = i + 1;
        _count++;
        return true;
    }

    public bool TryRemove(Pixel pixel)
    {
        if (pixel.QueueIndex == -1) return false;

        _pixels[pixel.QueueIndex] = null;
        pixel.QueueIndex = -1;
        _count--;
        return true;
    }
}