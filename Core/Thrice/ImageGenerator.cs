using System.Diagnostics;
using System.Drawing;

namespace AllColors.Thrice;

public class ImageGenerator
{
    private static ARGB[] CreateColors(ColorSpace options)
    {
        // ColorCount is the number of each different R, G, and B values we will use
        // Example:
        // 16 => 16*16*16 = 4096 = 64x64
        // 32 => 32*32*32 = ? = 256*128
        int colorCount = options.ColorDepth;

        // ColorCount each of R, G, and B
        var count = colorCount * colorCount * colorCount;

        ARGB[] colors = new ARGB[count];
        int c = 0;

        // We have a number of colors to generate, which gives us a divisor
        int divisor = colorCount - 1;

        // For each channel
        for (var r = 0; r < colorCount; r++)
        for (var g = 0; g < colorCount; g++)
        for (var b = 0; b < colorCount; b++)
        {
            var color = new ARGB(
                r * 255 / divisor,
                g * 255 / divisor,
                b * 255 / divisor);
            colors[c++] = color;
        }

        Debug.Assert(c == count);

        return colors;
    }

    // gets the difference between two colors
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Distance(ARGB left, ARGB right)
    {
        var r = left.Red - right.Red;
        var g = left.Green - right.Green;
        var b = left.Blue - right.Blue;
        return (r * r) + (g * g) + (b * b);
    }

    private static int CalculateFit(ImageCell imageCell, Color color)
    {
        int fit = int.MaxValue;
        var neighbors = imageCell.Neighbors;
        for (var i = neighbors.Length - 1; i >= 0; i--)
        {
            var neighbor = neighbors[i];
            if (!neighbor.IsEmpty)
            {
                var dist = Distance(color, neighbor.Color);
                if (dist < fit) fit = dist;
            }
        }
        return fit;
    }


    private readonly ImageGrid _imageGrid;
    private readonly ColorSpace _colorSpace;
    private readonly ARGB[] _colors;
    private readonly int _width;
    private readonly int _height;

    public ImageGenerator(ColorSpace options)
    {
        _width = options.Width;
        _height = options.Height;
        _imageGrid = new ImageGrid(_width, _height);
        _colorSpace = options;
        _colors = CreateColors(options);
    }

    public DirectBitmap Generate(int? seed = null)
    {
        var timer = Stopwatch.StartNew();

        // Get a shuffled set of colors
        var shuffler = new Shuffler(seed);
        var colors = shuffler.ShuffleCopy<ARGB>(_colors);
        var colorsCount = colors.Length;

        // Fresh grid
        _imageGrid.Clear();

        // Our Coordinates that are available to be filled
        var available = new HashSet<ImageCell>((_width + _height) * 2);

#if DEBUG

        // We need a double for percentage completion
        double dCount = (double)colorsCount;

        // We report every 1/10th of 1%
        var report = (int)Math.Floor(dCount * 0.001d);
        if (report < 512)
        {
            report = (int)Math.Floor(dCount * 0.01d);
        }

        int maxQueueCount = 0;
        //int consoleLineYPos = Console.CursorTop;
#endif

        // Loop through all colors
        for (var c = 0; c < colorsCount; c++)
        {
            Color color = colors[c];

#if DEBUG
            // Report every 256 colors
            if (c % report == 0)
            {
                double progress = c / dCount;
                int queueCount = available.Count;
                if (queueCount > maxQueueCount)
                    maxQueueCount = queueCount;
                //Console.CursorTop = consoleLineYPos;
                Debug.WriteLine($"{progress:P1} complete: Queue at {queueCount}");
            }
#endif

            ImageCell bestCell;

            // The very first pixel we place in the middle
            if (available.Count == 0)
            {
                var midPoint = _colorSpace.MidPoint;
                bestCell = _imageGrid[midPoint];
            }
            else
            {
                bestCell = available
                    .AsParallel()
                    .OrderBy(cell => CalculateFit(cell, color))
                    .ThenBy(cell => cell.Position)
                    .First();
            }

            Debug.Assert(bestCell.IsEmpty);

            // Set that cell's color
            bestCell.SetColor(color);

            // Remove that cell from available
            available.Remove(bestCell); // Okay if this is false

            // For all of that cell's neighbors, add them to available if they are empty
            var neighbors = bestCell.Neighbors;
            for (var i = neighbors.Length - 1; i >= 0; i--)
            {
                var neighbor = neighbors[i];
                if (neighbor.IsEmpty)
                {
                    available.Add(neighbor);
                }
            }
        }

        Debug.Assert(available.Count == 0);

        //Console.CursorTop = consoleLineYPos;
        Debug.WriteLine($"{1.0d:P1} complete: Queue at {0}");

        timer.Stop();
        Debug.WriteLine($"Completed in {timer.Elapsed:c}");

        // Build the image
        var img = new DirectBitmap(_width, _height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var cell = _imageGrid[x, y];
                Debug.Assert(!cell.IsEmpty);
                img.SetPixel(x, y, cell.Color);
            }
        }

        return img;
    }
}