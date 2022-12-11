using System.Collections;
using AllColors.RGBGenerator;

using System.Diagnostics;
using System.Drawing;

namespace AllColors.Thrice;

public sealed class ImageOptions
{
    public static ImageOptions BestColors(int width, int height)
    {
        if (width is < 1 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width,
                $"Width must be at least 1 and no more than {ushort.MaxValue}");
        }

        if (height is < 1 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height,
                $"Height must be at least 1 and no more than {ushort.MaxValue}");
        }

        var area = width * height;

        // Do we have an exact fit?
        double cubeRoot = Math.Cbrt(area);
        if (double.IsInteger(cubeRoot))
        {
            // We have colorcount!
            return new ImageOptions((int)cubeRoot, width, height);
        }

        // We always want to use the most number of colors
        int colorCount = 256;
        // And we know, for that color count, how many pixels it will fill
        int pixelCount;

        // No fewer than 2 colors
        while (colorCount >= 2)
        {
            pixelCount = (colorCount * colorCount * colorCount);
            // If this is fewer pixels than us, we can scan it
            if (pixelCount <= area)
            {
                var factors = GetFactors(pixelCount);
                IEnumerable<Size> sizes;
                if (width <= height)
                {
                    sizes = factors.Where(t => t.Lesser <= width && t.Greater <= height)
                        .Select(factor => new Size(factor.Lesser, factor.Greater));
                }
                else
                {
                    sizes = factors.Where(t => t.Lesser <= height && t.Greater <= width)
                        .Select(factor => new Size(factor.Greater, factor.Lesser));
                }

                var options = sizes
                    .OrderByDescending(size => Math.Abs(size.Width - width) + Math.Abs(size.Height - height))
                    .ToList();

                if (options.Count > 0)
                {
                    if (options.Count > 1)
                        Debugger.Break();

                    return new ImageOptions(colorCount, options.First().Width, options.First().Height);
                }
            }
            colorCount--;
        }
        


        throw new NotImplementedException();
    }

    public static Size GetSize(int colorCount, Size bounds)
    {
        var area = colorCount * colorCount * colorCount;
        
        // Chunks of 16
        var widthChunks = Enumerable.Range(0, bounds.Width)
            .Reverse().Chunk(16);
        var heightChunks = Enumerable.Range(0, bounds.Height)
            .Reverse().Chunk(16);

        using var widthEnumerator = widthChunks.GetEnumerator();
        using var heightEnumerator = heightChunks.GetEnumerator();

        while (widthEnumerator.MoveNext() & heightEnumerator.MoveNext())
        {
            foreach (var y in heightEnumerator.Current)
            foreach (var x in widthEnumerator.Current)
            {
                var rectArea = y * x;
                if (rectArea == area)
                {
                    return new Size(x, y);
                }
            }
        }

        throw new InvalidOperationException();

    }


    public static (int Lesser, int Greater)[] GetFactors(int number)
    {
        // No more than sqrt
        int sqrt = (int)Math.Sqrt(number);

        // Create an array to store the factors
        Span<(int,int)> factors = stackalloc (int,int)[sqrt];
        int f = 0;

        // Loop through the numbers from 1 to the square root of the number
        // to find all the factors

        for (int i = 1; i <= sqrt; i++)
        {
            // If the number is evenly divisible by the current number,
            // add both the number and its corresponding factor to the array
            if (number % i == 0)
            {
                factors[f++] = (i, number/i);
            }
        }

        // Return the array of factors
        return factors[..f].ToArray();
    }

    public static ImageOptions BestRectangle(int colorCount)
    {
        if (colorCount is < 2 or > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(colorCount), colorCount,
                "There must be at least 2 and no more than 256 colors specified");
        }

        // Triple the colors is the number of colors we have to fill
        var pixels = colorCount * colorCount * colorCount;

        // Find the smallest power-of-2 square that can contain that many
        int sideLength;
        for (int shift = 0; shift <= 12; shift++)
        {
            sideLength = 1 << shift;
            var area = sideLength * sideLength;
            if (area >= pixels)
            {
                // Easy square?
                if (area == pixels)
                    return new ImageOptions(colorCount, sideLength, sideLength);

                // Widescreen?
                double dPixels = (double)pixels;

                double height = dPixels / sideLength;
                if (double.IsInteger(height))
                {
                    Debug.Assert(sideLength * (int)height == pixels);
                    return new ImageOptions(colorCount, sideLength, (int)height);
                }

                // We have to back down
                
                do
                {
                    // Lower
                    sideLength--;

                    // Recalculate
                    height = dPixels / sideLength;

                    // A match?
                    if (double.IsInteger(height))
                    {
                        Debug.Assert(sideLength * (int)height == pixels);
                        return new ImageOptions(colorCount, sideLength, (int)height);
                    }

                } while (sideLength > 1);
            }
        }

        throw new ApplicationException("This is not possible");
    }

    public int ColorCount { get; }
    public int Width { get; }
    public int Height { get; }

    public Coord MidPoint => new Coord((int)(Width / 2f), (int)(Height / 2f));

    private ImageOptions(int colorCount, int width, int height)
    {
        this.ColorCount = colorCount;
        this.Width = width;
        this.Height = height;

        Debug.Assert(ColorCount * ColorCount * ColorCount == Width * Height);
    }

    public override string ToString()
    {
        return $"{ColorCount} colors {Width}x{Height}";
    }
}