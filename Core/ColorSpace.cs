using System.Diagnostics;

namespace AllColors;

public sealed class ColorSpace
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateWidth(int width, [CallerArgumentExpression(nameof(width))] string? widthName = "")
    {
        if (width is < 1 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                widthName, 
                width,
                $"{widthName} '{width}' must be at least 1 and no more than {ushort.MaxValue}");
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateHeight(int height, [CallerArgumentExpression(nameof(height))] string? heightName = "")
    {
        if (height is < 1 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                heightName, 
                height,
                $"{heightName} '{height}' must be at least 1 and no more than {ushort.MaxValue}");
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateColorDepth(int colorDepth, [CallerArgumentExpression(nameof(colorDepth))] string? colorDepthName = "")
    {
        if (colorDepth is < 2 or > 256)
        {
            throw new ArgumentOutOfRangeException(
                colorDepthName, 
                colorDepth,
                $"{colorDepthName} '{colorDepth}' must be at least 2 and no more than 256");
        }
    }

    public static ARGB[] AllColors()
    {
        const int count = 256 * 256 * 256;
        ARGB[] colors = new ARGB[count];
        int c = 0;

        for (var r = 0; r < 256; r++)
        for (var g = 0; g < 256; g++)
        for (var b = 0; b < 256; b++)
        {
            colors[c++] = new ARGB(0, (byte)r, (byte)g, (byte)b);
        }

        Debug.Assert(c == count);

        return colors;
    }

    public static ARGB[] GetColors(int colorDepth)
    {
        // Color Depth is the number of each different R, G, and B values we will use
        // Example:
        // 16 => 16*16*16 = 4096 = 64x64
        // 32 => 32*32*32 = ? = 256*128
        
        // Depth each of R, G, and B
        var count = colorDepth * colorDepth * colorDepth;

        ARGB[] colors = new ARGB[count];
        int c = 0;

        // We have a number of colors to generate, which gives us a divisor
        int divisor = colorDepth - 1;

        // For each channel
        for (var r = 0; r < colorDepth; r++)
        for (var g = 0; g < colorDepth; g++)
        for (var b = 0; b < colorDepth; b++)
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

    public static ColorSpace BestFit(int width, int height)
    {
        ValidateWidth(width);
        ValidateHeight(height);

        // This is the total area we want to fill
        var area = width * height;

        // Color Depth is the total number of the 256 values of R, G, and B to use
        // Thus, number of pixels = depth * depth * depth

        // Do we have an exact fit?
        double cubeRoot = Math.Cbrt(area);
        if (double.IsInteger(cubeRoot))
        {
            // We do!
            return new ColorSpace((int)cubeRoot, width, height);
        }

        // We always want to use the biggest depth we cant
        int colorDepth = 256;

        // And we know, for that color count, how many pixels it will fill
        int pixelCount;

        // No fewer than 2
        while (colorDepth >= 2)
        {
            pixelCount = colorDepth * colorDepth * colorDepth;
            // If this is fewer pixels than us, we can scan it
            if (pixelCount <= area)
            {
                // Look for the nicely divisible factors for this count
                var factors = GetFactors(pixelCount);

                IEnumerable<Size> sizes;
                // Adjust for Landscape vs Portrait
                if (width <= height)
                {
                    // Only factors that fit in our desired area
                    sizes = factors.Where(t => t.Lesser <= width && t.Greater <= height)
                        .Select(factor => new Size(factor.Lesser, factor.Greater));
                }
                else
                {
                    // Only factors that fit in our desired area
                    sizes = factors.Where(t => t.Lesser <= height && t.Greater <= width)
                        .Select(factor => new Size(factor.Greater, factor.Lesser));
                }

                // Order them from the closest match to the furthest
                var options = sizes
                    .OrderByDescending(size => Math.Abs(size.Width - width) + Math.Abs(size.Height - height))
                    .ToList();

                // We may not have found a match
                if (options.Count > 0)
                {
                    if (options.Count > 1)
                        Debugger.Break();

                    var match = options[0];
                    return new ColorSpace(colorDepth, match.Width, match.Height);
                }
            }

            // Try a lower depth
            colorDepth--;
        }

        throw new InvalidOperationException();
    }


    /// <summary>
    /// Gets all of the factors of the given <paramref name="number"/>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private static (int Lesser, int Greater)[] GetFactors(int number)
    {
        // No more than sqrt
        int sqrt = (int)Math.Sqrt(number);

        // Create an array to store the factors
        Span<(int, int)> factors = stackalloc (int, int)[sqrt];
        int f = 0;

        // Loop through the numbers from 1 to the square root of the number
        // to find all the factors

        for (int i = 1; i <= sqrt; i++)
        {
            // If the number is evenly divisible by the current number,
            // add both the number and its corresponding factor to the array
            if (number % i == 0)
            {
                factors[f++] = (i, number / i);
            }
        }

        // Return the array of factors
        return factors[..f].ToArray();
    }

    public static ColorSpace BestRectangle(int colorDepth)
    {
        ValidateColorDepth(colorDepth);

        // Triple the colors is the number of colors we have to fill
        var pixels = colorDepth * colorDepth * colorDepth;

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
                    return new ColorSpace(colorDepth, sideLength, sideLength);

                // Widescreen?
                double dPixels = pixels;

                double height = dPixels / sideLength;
                if (double.IsInteger(height))
                {
                    Debug.Assert(sideLength * (int)height == pixels);
                    return new ColorSpace(colorDepth, sideLength, (int)height);
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
                        return new ColorSpace(colorDepth, sideLength, (int)height);
                    }

                } while (sideLength > 1);
            }
        }

        throw new ApplicationException("This is not possible");
    }

    public int ColorDepth { get; }
    public int Width { get; }
    public int Height { get; }

    public Coord MidPoint => new Coord((int)(Width / 2f), (int)(Height / 2f));

    public ColorSpace(int colorCount, int width, int height)
    {
        ColorDepth = colorCount;
        Width = width;
        Height = height;

        Debug.Assert(ColorDepth * ColorDepth * ColorDepth == Width * Height);
    }

    public override string ToString()
    {
        return $"{ColorDepth} colors {Width}x{Height}";
    }
}