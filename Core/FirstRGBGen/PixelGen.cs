//#define RUNTESTS


/*
 * General optimization notes:
 * - We try hard not to give any job for the GC. We allocate all objects at startup and only use structs later.
 * - To be very fast, we only use arrays as collections, and not even a foreach.
 * - Parallel processing is tricky. Smaller chunks mean more balance but more overhead. Our parallel operations
 *   are very small and we keep the queue balanced, so we aim for the biggest possible chunk size.
 */

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AllColors.FirstRGBGen;

public sealed class PixelGenOptions
{
        /// <summary>
    /// Prints the command line help.
    /// </summary>
    public static void PrintArgsHelp()
    {
        Console.WriteLine();
        Console.WriteLine("You have to run the program like this:");
        Console.WriteLine(
            "{0} [colors] [width] [height] [startx] [starty] [frames] [seed] [neighbors] [sorting] [algo]",
            AppDomain.CurrentDomain.FriendlyName);
        Console.WriteLine();
        Console.WriteLine("For a quick start, try this:");
        Console.WriteLine("{0} 64 512 512 256 256 5 0 11111111 rnd one",
            AppDomain.CurrentDomain.FriendlyName);
        Console.WriteLine();
        Console.WriteLine("Explanation:");
        Console.WriteLine("[colors]: number of colors per channel, at most 256, must be a power of 2");
        Console.WriteLine("[width], [height]: dimensions of the image, each must be a power of 2");
        Console.WriteLine("[startx], [starty]: coordinates of the first pixel");
        Console.WriteLine("[frames]: number of frames to generate, must be positive");
        Console.WriteLine("[seed]: random seed, try 0 for truly random");
        Console.WriteLine("[neighbors]: specify eight 1's or 0's to allow movements in these directions");
        Console.WriteLine("[sorting]: color ordering, can be 'rnd' or 'hue-N' (N=0..360)");
        Console.WriteLine("[algo]: can be 'one' or 'avg' or 'avgsq'");
        Console.WriteLine();
    }

    /// <summary>
    /// Parses the command line arguments.
    /// </summary>
    public static PixelGenOptions? ParseArgs(string[] args)
    {
        if (args.Length != 10)
        {
            Console.WriteLine("There must be exactly 10 arguments given!");
            return null;
        }

        /*// generate 2^0..2^24 for easier checking
        var twopows = new List<string>();
        var p = 1;
        for (var i = 0; i <= 24; i++)
        {
            twopows.Add(p.ToString());
            p *= 2;
        }*/

        // [colors]
        if (/*!twopows.Contains(args[0]) ||*/ !int.TryParse(args[0], out var numColors) || numColors > 256)
        {
            Console.WriteLine("[colors] is an invalid number");
            return null;
        }

        // [width] and [height]
        if (/*!twopows.Contains(args[1]) ||*/ !int.TryParse(args[1], out var width))
        {
            Console.WriteLine("[width] is an invalid number");
            return null;
        }

        if (/*!twopows.Contains(args[2]) ||*/ !int.TryParse(args[2], out var height))
        {
            Console.WriteLine("[height] is an invalid number");
            return null;
        }

        if ((long)width * height != numColors * numColors * numColors)
        {
            Console.WriteLine("[width]*[height] must be equal to [colors]*[colors]*[colors]");
            return null;
        }

        var imagePixels = new Pixel[width * height];

        // [startx], [starty]
        if (!int.TryParse(args[3], out var startX))
        {
            Console.WriteLine("[startx] is an invalid number");
            return null;
        }

        if (startX < 0 || startX >= width)
        {
            Console.WriteLine("[startx] is out of bounds");
            return null;
        }

        if (!int.TryParse(args[4], out var startY))
        {
            Console.WriteLine("[starty] is an invalid number");
            return null;
        }

        if (startY < 0 || startY >= height)
        {
            Console.WriteLine("[starty] is out of bounds");
            return null;
        }

        // [frames]
        if (!int.TryParse(args[5], out var numFrames))
        {
            Console.WriteLine("[frames] is an invalid number");
            return null;
        }

        if (numFrames < 1)
        {
            Console.WriteLine("[frames] must be positive");
            return null;
        }

        // [seed]
        int seed;
        if (!int.TryParse(args[6], out seed))
        {
            Console.WriteLine("[seed] is an invalid number");
            return null;
        }

        var shuffler = seed == 0 ? new Shuffler(null) : new Shuffler(seed);

        // [neighbors]
        if (!Regex.IsMatch(args[7], "^[01]{8}$"))
        {
            Console.WriteLine("[neighbors] is not given according to the rules");
            return null;
        }

        var nx = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        var ny = new[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        var neighX = Enumerable.Range(0, 8).Where(i => args[7][i] == '1').Select(i => nx[i]).ToArray();
        var neighY = Enumerable.Range(0, 8).Where(i => args[7][i] == '1').Select(i => ny[i]).ToArray();

        // [sorting]
        IComparer<ARGB> sorter;
        if (args[8] == "rnd")
            sorter = new RandomComparer(shuffler);
        else if (args[8].StartsWith("hue-"))
        {
            int hueshift;
            if (!int.TryParse(args[8].Substring(4), out hueshift) || hueshift < 0 || hueshift > 360)
            {
                Console.WriteLine("[sorting] has an invalid hue parameter");
                return null;
            }

            sorter = new HueComparer(hueshift);
        }
        else
        {
            Console.WriteLine("[sorting] is not one of the allowed values");
            return null;
        }

        // [algo]
        AlgorithmBase algorithm;
        PixelBias bias = new WeightedPixelBias();// RandomPixelBias(_shuffler);

        if (args[9] == "one")
            algorithm = new OneNeighborSqAlgorithm(imagePixels, (startY * width) + startX, bias);
        else if (args[9] == "avg")
            algorithm = new AverageNeighborAlgorithm(imagePixels, (startY * width) + startX);
        else if (args[9] == "avgsq")
            algorithm = new AverageNeighborSqAlgorithm(imagePixels, (startY * width) + startX);
        
        else
        {
            Console.WriteLine("[algo] is not one of the allowed values");
            return null;
        }

        Console.WriteLine("Command line arguments are accepted");
        return new PixelGenOptions
        {
            ColorSpace = new ColorSpace(numColors, width, height),
            Algorithm = algorithm,
            NeighX = neighX,
            NeighY = neighY,
            NumFrames = numFrames,
            Shuffler = shuffler,
            Sorter = sorter,
            ImagePixels = imagePixels,
        };
    }


    public ColorSpace ColorSpace { get; set; }

    /// <summary>
    /// Number of image frames to save.
    /// </summary>
    public int NumFrames { get; set; }

    /// <summary>
    /// Random generator, only used during pre-calculations in a deterministic way. The same seed always results in the same image.
    /// </summary>
    public Shuffler Shuffler { get; set; }

    /// <summary>
    /// Available neighbor X coordinate differences (-1,0,+1).
    /// </summary>
    public int[] NeighX { get; set; }

    /// <summary>
    /// Available neighbor Y coordinate differences (-1,0,+1).
    /// </summary>
    public int[] NeighY { get; set; }

    /// <summary>
    /// The chosen color sorting implementation.
    /// </summary>
    public IComparer<ARGB> Sorter { get; set; }

    /// <summary>
    /// The chosen algorithm implementation.
    /// </summary>
    public AlgorithmBase Algorithm { get; set; }

    public Pixel[] ImagePixels { get; init; }
}

public static class PixelGen
{
    


    /// <summary>
    /// Holds the big image.
    /// </summary>
    private static Pixel[] _imagePixels = null!;

    public static DirectBitmap? Run(PixelGenOptions options)
    {
        var width = options.ColorSpace.Width;
        var height = options.ColorSpace.Height;
        var colorDepth = options.ColorSpace.ColorDepth;
        var shuffler = options.Shuffler;

        var start = DateTime.Now;
        //var timer = Stopwatch.StartNew();

        //Console.WriteLine("----- Setup -----");

        // Create every color in our colorspace
        ARGB[] colors = ColorSpace.GetColors(colorDepth);

        //Console.WriteLine($"{timer.Elapsed:g}: Created {colors.Length} colors");

        // We need to shuffle it somehow
        if (options.Sorter is RandomComparer)
        {
            shuffler.Shuffle<ARGB>(colors);
        }
        else
        {
            Array.Sort<ARGB>(colors, options.Sorter);
        }
        
        //Console.WriteLine($"{timer.Elapsed:g}: Sorted colors");

        // create the pixels
        _imagePixels = options.ImagePixels;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            int weight = shuffler.Next();
            _imagePixels[(y * width) + x] = new Pixel(x, y, weight);
        }

#if RUNTESTS
        // Verify
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            var pixel = _imagePixels[(y * _width) + x];
            Debug.Assert(pixel.Pos.X == x);
            Debug.Assert(pixel.Pos.Y == y);
        }
#endif

        //Console.WriteLine($"{timer.Elapsed:g}: Setup {_imagePixels.Length} Pixels");

        // pre-calculate the neighbors of every pixel
        var neighX = options.NeighX;
        var neighY = options.NeighY;
#if RUNTESTS
        Debug.Assert(_neighX.Length == _neighY.Length);
#endif
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            // This excludes self!
            Pixel[] neighbors = Enumerable.Range(0, neighX.Length)
                .Select(n =>
            {
                var y2 = y + neighY[n];
                if (y2 < 0 || y2 == height)
                    return null;
                var x2 = x + neighX[n];
                if (x2 < 0 || x2 == width)
                    return null;
                var pixel = _imagePixels[(y2 * width) + x2];
                Debug.Assert(pixel.Pos == new Coord(x2, y2));
                return pixel;
            }).Where(p => p != null).ToArray()!;
            // Store
            _imagePixels[(y * width) + x].Neighbors = neighbors; 
        }

#if RUNTESTS
        // Verify
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            var pixel = _imagePixels[((y * _width) + x)];
            var neighbors = pixel.Neighbors;
            foreach (var neighbor in neighbors)
            {
                var xChange = Math.Abs(neighbor.Pos.X - x);
                Debug.Assert(xChange <= 1);
                var yChange = Math.Abs(neighbor.Pos.Y - y);
                Debug.Assert(yChange <= 1);
            }
        }
#endif

        //Console.WriteLine($"{timer.Elapsed:g}: Calculated Pixel Neighbors");
        //int consoleY = Console.CursorTop;

#if RUNTESTS
        // calculate the saving checkpoints in advance
        var checkpoints = Enumerable.Range(1, _numFrames)
            .ToDictionary(i => (long)i * colors.Length / _numFrames - 1, i => i - 1);
        Thread pngThread = null;
#endif

        var algorithm = options.Algorithm;
        // loop through all colors that we want to place
        for (var i = 0; i < colors.Length; i++)
        {
            // give progress report to the impatient user
            if (i % 1024 == 0)
            {
                var queue = algorithm.Queue;
                queue.Compact();

                double percentComplete = (double)i / width / height;
                //Console.CursorTop = consoleY;
                //Console.WriteLine($"{timer.Elapsed:g}: {percentComplete:P1}  |  Queue: {algorithm.Queue.Count}")
                Console.WriteLine($"{percentComplete:P1}  |  Queue: {queue.Count}");
            }

            // run the algorithm
            algorithm.Place(colors[i]);

#if RUNTESTS
            // save a checkpoint if needed
            int checkIndex;
            if (checkpoints.TryGetValue(i, out checkIndex))
            {
                // create the image
                var img = new Bitmap(_width, _height, PixelFormat.Format24bppRgb);
                var idata = img.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);
                var ibytes = new byte[idata.Stride * idata.Height];
                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var c = _imagePixels[y * _width + x].Color;
                        ibytes[y * idata.Stride + x * 3 + 2] = c.Red;
                        ibytes[y * idata.Stride + x * 3 + 1] = c.Green;
                        ibytes[y * idata.Stride + x * 3 + 0] = c.Blue;
                    }
                }

                Marshal.Copy(ibytes, 0, idata.Scan0, ibytes.Length);
                img.UnlockBits(idata);

                // png compression uses only one processor, so push it into the background, limiting to one thread at a time is more than enough
                if (pngThread != null)
                    pngThread.Join();
                pngThread = new Thread(new ThreadStart(delegate
                {
                    img.Save($"result{checkIndex:D5}.png", ImageFormat.Png);
                    img.Dispose();
                }));
                pngThread.Start();
            }
#endif
        }

        Debug.Assert(algorithm.Queue.Count == 0);

#if RUNTESTS
        // wait for the final image
        pngThread.Join();

        // check the number of colors to be sure
        var img2 = (Bitmap)Image.FromFile($"result{_numFrames - 1:D5}.png");
        var ch = new HashSet<ARGB>();
        for (var y = 0; y < img2.Height; y++)
        for (var x = 0; x < img2.Width; x++)
        {
            var pix = img2.GetPixel(x, y);
            if (!ch.Add(new ARGB
                (
                    red: pix.R,
                    green: pix.G,
                    blue: pix.B
                )))
            {
                Console.WriteLine("Color {0}/{1}/{2} is added more than once!!!!!!", pix.R, pix.G, pix.B);
            }
        }

        img2.Dispose();
#endif

        // we're done!
        //Console.WriteLine($"{timer.Elapsed:g}: Finished!");
        Console.WriteLine($"Finished in {DateTime.Now - start:g}");

        DirectBitmap image = new DirectBitmap(width, height);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            image.SetPixel(x,y, _imagePixels[(y*width)+x].Color);
        }

        return image;
    }

}