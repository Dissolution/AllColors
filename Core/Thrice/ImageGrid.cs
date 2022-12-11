using System.Drawing;

namespace AllColors.Thrice;

public sealed class ImageGrid
{
    private readonly ImageCell[,] _imageCells;
    private readonly int _width;
    private readonly int _height;

    public ImageCell this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _imageCells[x, y];
        }
    }

    public ImageCell this[Coord pos]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _imageCells[pos.X, pos.Y];
        }
    }

    public ImageGrid(int width, int height)
    {
        _width = width;
        _height = height;
        _imageCells = new ImageCell[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            _imageCells[x, y] = new ImageCell(new(x, y));
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            _imageCells[x, y].Neighbors = GetNeighbors(x, y);
        }
    }

    internal ImageCell[] GetNeighbors(int x, int y)
    {
        var neighbors = new List<ImageCell>(8);
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

                neighbors.Add(_imageCells[newX, newY]);
            }
        }
        return neighbors.ToArray();
    }

    public void Clear()
    {
        var cells = _imageCells;
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            cells[x, y].Color = null;
        }
    }
}