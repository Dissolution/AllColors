using CommunityToolkit.HighPerformance;

namespace AllColors.Thrice;

public readonly struct ImagePixels
{
    private readonly ImagePixel[] _pixels;
    private readonly int _length;
    private readonly int _width;
    private readonly int _height;

    public Span<ImagePixel> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Span<ImagePixel>(_pixels);
    }

    public Span2D<ImagePixel> Span2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Span2D<ImagePixel>(_pixels, _height, _width);
    }
    
    public ref ImagePixel this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _pixels[ToIndex(x,y)];
    }

    public ref ImagePixel this[Coord pos]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _pixels[ToIndex(pos.X, pos.Y)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToIndex(int x, int y) => (y * _width) + x;

    public ImagePixels(int width, int height)
    {
        _width = width;
        _height = height;
        _length = width * height;
        _pixels = new ImagePixel[_length];
        for (var i = 0; i < _length; i++)
        {
            _pixels[i] = new ImagePixel(i % _width, i / _width);
        }
        var span2D = this.Span2D;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            span2D[y, x].Neighbors = GetNeighbors(x, y);
        }
    }

    internal ImagePixel[] GetNeighbors(int x, int y)
    {
        var neighbors = new List<ImagePixel>(8);
        
        // Vertical, top to bottom
        for (var yOffset = -1; yOffset <= 1; yOffset++)
        {
            int newY = y + yOffset;

            // We might not have this neighbor
            if (newY < 0 || newY >= _height) continue;

            // Horizontal, left to right
            for (var xOffset = -1; xOffset <= 1; xOffset++)
            {
                int newX = x + xOffset;

                // We might not have this neighbor
                if (newX < 0 || newX >= _width) continue;

                // Self?
                if (yOffset == 0 && xOffset == 0) continue;

                neighbors.Add(_pixels[ToIndex(newX, newY)]);
            }
        }
        return neighbors.ToArray();
    }

    public void Clear()
    {
        ref ImagePixel pixel = ref Unsafe.NullRef<ImagePixel>();
        for (var i = 0; i < _length; i++)
        {
            pixel = ref _pixels[i];
            pixel.IsEmpty = true;
            pixel.Color = default;
        }
    }
}