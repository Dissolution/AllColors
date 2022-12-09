using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace AllColors;

// represent a coordinate
public readonly struct XY : IEquatable<XY>,
                   IEqualityOperators<XY, XY, bool>
{
    public static bool operator ==(XY left, XY right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(XY left, XY right) => left.X != right.X || left.Y != right.Y;
    
    public readonly int X;
    public readonly int Y;
    
    public XY(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public bool Equals(XY xy)
    {
        return this.X == xy.X && this.Y == xy.Y;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is XY xy && this.X == xy.X && this.Y == xy.Y;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine<int, int>(X, Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}

public readonly struct ColorSpace
{
    public static readonly ColorSpace Sixteen = new ColorSpace(16, 64, 64);
    public static readonly ColorSpace TwentyFour = new ColorSpace(24, 128, 108);
    public static readonly ColorSpace ThirtyTwo = new ColorSpace(32, 256, 128);
    public static readonly ColorSpace SixtyFour = new ColorSpace(64, 512, 512);
        
    public readonly byte Count;
    public readonly ushort Width;
    public readonly ushort Height;

    public ushort StartX => (ushort)(Width / 2f);
    public ushort StartY => (ushort)(Height / 2f);

    private ColorSpace(byte count, ushort width, ushort height)
    {
        this.Count = count;
        this.Width = width;
        this.Height = height;
        
        Debug.Assert(((int)count * (int)count * (int)count) == ((int)height * (int)width));
    }
}

public sealed class StackExchangeUpdated
{
    // algorithm settings, feel free to mess with it
    private const bool AVERAGE = false;

    // gets the difference between two colors
    private static int ColorDiff(Color left, Color right)
    {
        var r = left.R - right.R;
        var g = left.G - right.G;
        var b = left.B - right.B;
        return (r * r) + (g * g) + (b * b);
    }

    
    private readonly ColorSpace _colorSpace;
    
    // gets the neighbors (3..8) of the given coordinate
    private XY[] GetNeighbors(XY xy, bool includeSelf = true)
    {
        Span<XY> neighbors = stackalloc XY[9];
        int n = 0;
        for (var yChange = -1; yChange <= 1; yChange++)
        {
            int newY = xy.Y + yChange;
            if (newY == -1 || newY == _colorSpace.Height) continue;
            for (var xChange = -1; xChange <= 1; xChange++)
            {
                int newX = xy.X + xChange;
                if (newX == -1 || newX == _colorSpace.Width) continue;
                if (!includeSelf && newX == xy.X && newY == xy.Y) continue;
                neighbors[n++] = new XY(newX, newY);
            }
        }
        return neighbors[..n].ToArray();
    }

    private static int Average(ReadOnlySpan<int> diffs)
    {
        long total = 0L;
        for (var i = diffs.Length - 1; i >= 0; i--)
        {
            total += diffs[i];
        }
        return (int)((double)total / (double)diffs.Length);
    }
    
    private static int Min(ReadOnlySpan<int> diffs)
    {
        int min = int.MaxValue;
        for (var i = diffs.Length - 1; i >= 0; i--)
        {
            if (diffs[i] < min)
                min = diffs[i];
        }
        return min;
    }

    public StackExchangeUpdated(ColorSpace colorSpace)
    {
        _colorSpace = colorSpace;
    }
    
    // calculates how well a color fits at the given coordinates
    private int CalcColorFit(Color[,] pixels, XY xy, Color color)
    {
        // get the diffs for each neighbor separately
        Span<int> diffs = stackalloc int[9];
        int d = 0;
        var xyNeighbors = GetNeighbors(xy);
        for (var i = 0; i < xyNeighbors.Length; i++)
        {
            XY neighborXY = xyNeighbors[i];
            var neighborColor = pixels[neighborXY.Y, neighborXY.X];
            if (!neighborColor.IsEmpty)
            {
                diffs[d++] = ColorDiff(neighborColor, color);
            }
        }

        // average or minimum selection
        if (AVERAGE)
        {
            return Average(diffs[..d]);
        }
        else
        {
            return Min(diffs[..d]);
        }
    }

    public void Produce(int? seed, string outputFilePath)
    {
        // create every color once and randomize the order
        Color[] colors = new Color[_colorSpace.Count * _colorSpace.Count * _colorSpace.Count];
        int c = 0;
        
        for (var r = 0; r < _colorSpace.Count; r++)
        for (var g = 0; g < _colorSpace.Count; g++)
        for (var b = 0; b < _colorSpace.Count; b++)
        {
            var color = Color.FromArgb(
                (r * 255) / (_colorSpace.Count - 1), 
                (g * 255) / (_colorSpace.Count - 1), 
                (b * 255) / (_colorSpace.Count - 1)
                );
            colors[c++] = color;
        }

        Random random;
        if (seed.HasValue)
        {
            random = new Random(seed.Value);
        }
        else
        {
            random = new Random();
        }
        // Shuffle the colors
        Shuffler.Shuffle<Color>(random, colors);

        // temporary place where we work (faster than all that many GetPixel calls)
        var pixels = new Color[_colorSpace.Height, _colorSpace.Width];
        //Debug.Assert(pixels.Length == colors.Length);

        // constantly changing list of available coordinates (empty pixels which have non-empty neighbors)
        var available = new HashSet<XY>(1000);

        // calculate the checkpoints in advance
        var checkpoints = Enumerable
            .Range(1, 10)
            .ToDictionary(i => ((i * colors.Length) / 10) - 1, static i => i - 1);

        // loop through all colors that we want to place
        for (var i = 0; i < colors.Length; i++)
        {
            Color color = colors[i];
            
            if (i % 256 == 0)
            {
                Console.WriteLine("{0:P}, queue size {1}", (double)i / (double)_colorSpace.Width / (double)_colorSpace.Height, available.Count);
            }

            XY bestXY;
            if (available.Count == 0)
            {
                // use the starting point
                bestXY = new XY(_colorSpace.StartX, _colorSpace.StartY);
            }
            else
            {
                // find the best place from the list of available coordinates
                // uses parallel processing, this is the most expensive step
                bestXY = available
                    .AsParallel()
                    .OrderBy(xy => CalcColorFit(pixels, xy, color))
                    .First();
            }

            // put the pixel where it belongs
            //Debug.Assert(pixels[bestXY.Y, bestXY.X].IsEmpty);
            pixels[bestXY.Y, bestXY.X] = colors[i];

            // adjust the available list
            available.Remove(bestXY);
            var bestXYNeighbors = GetNeighbors(bestXY, false);
            for (var b = 0; b < bestXYNeighbors.Length; b++)
            {
                XY nxy = bestXYNeighbors[b];
                if (pixels[nxy.Y, nxy.X].IsEmpty)
                    available.Add(nxy);
            }

            // save a checkpoint image?
            /*int chkidx;
            if (checkpoints.TryGetValue(i, out chkidx))
            {
                using (var img = new DirectBitmap(_colorSpace.Width, _colorSpace.Height))
                {
                    for (var y = 0; y < _colorSpace.Height; y++)
                    {
                        for (var x = 0; x < _colorSpace.Width; x++)
                        {
                            img.SetPixel(x, y, pixels[y, x]);
                        }
                    }
                    img.Bitmap.Save(Path.Combine(outputFilePath, $"Image_{chkidx}.bmp"), ImageFormat.Bmp);
                }
            }*/
        }

        //Debug.Assert(available.Count == 0);
        
        // Save final
        using (var img = new DirectBitmap(_colorSpace.Width, _colorSpace.Height))
        {
            for (var y = 0; y < _colorSpace.Height; y++)
            {
                for (var x = 0; x < _colorSpace.Width; x++)
                {
                    img.SetPixel(x, y, pixels[y, x]);
                }
            }
            img.Bitmap.Save(Path.Combine(outputFilePath, $"Image_Final.bmp"), ImageFormat.Bmp);
        }
    }
}