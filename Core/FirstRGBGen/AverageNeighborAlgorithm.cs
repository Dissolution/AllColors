/*using System.Collections.Concurrent;

namespace AllColors.FirstRGBGen;

/// <summary>
/// The queue contains empty pixels which have at least one filled neighbor. For every pixel in the queue, we calculate the average color of
/// its neighbors. In each step, we find the average that matches the new color the most. Uses a linear difference metric. It gives a blurred
/// effect.
/// </summary>
public class AverageNeighborAlgorithm : AlgorithmBase
{
    private readonly PixelData<ARGB> _pixelData = new();

    public override PixelQueue Queue
    {
        get { return _pixelData; }
    }

    public AverageNeighborAlgorithm(Pixel[] imagePixels, int startIndex) : base(imagePixels, startIndex)
    {
    }

    protected override Pixel PlaceImpl(ARGB c)
    {
        // find the best pixel with parallel processing
        var q = _pixelData.Pixels;
        var best = Partitioner.Create(0, _pixelData.UsedUntil, Math.Max(256, _pixelData.UsedUntil / Program2.Threads)).AsParallel()
            .Min(range =>
            {
                var bestdiff = int.MaxValue;
                Pixel bestpixel = null;
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var qp = q[i];
                    if (qp != null)
                    {
                        var avg = _pixelData.Data[qp.QueueIndex];
                        var rd = (int)avg.Red - c.Red;
                        var gd = (int)avg.Green - c.Green;
                        var bd = (int)avg.Blue - c.Blue;
                        var diff = rd * rd + gd * gd + bd * bd;
                        // we have to use the same comparison as PixelWithValue!
                        if (diff < bestdiff || (diff == bestdiff && qp.Pos < bestpixel.Pos))
                        {
                            bestdiff = diff;
                            bestpixel = qp;
                        }
                    }
                }

                return new PixelWithValue
                {
                    Pixel = bestpixel,
                    Value = bestdiff
                };
            }).Pixel;

        // found the pixel, return it
        _pixelData.Remove(best);
        return best;
    }

    protected override void ChangeQueue(Pixel p)
    {
        // recalculate the neighbors
        for (var i = 0; i < p.Neighbors.Length; i++)
        {
            var np = p.Neighbors[i];
            if (np.Empty)
            {
                int r = 0, g = 0, b = 0, n = 0;
                for (var j = 0; j < np.Neighbors.Length; j++)
                {
                    var nnp = np.Neighbors[j];
                    if (!nnp.Empty)
                    {
                        r += nnp.Color.Red;
                        g += nnp.Color.Green;
                        b += nnp.Color.Blue;
                        n++;
                    }
                }

                var avg = new ARGB
                (
                    red: (r / n),
                    green: (g / n),
                    blue: (b / n)
                );
                if (np.QueueIndex == -1)
                    _pixelData.Add(np);
                _pixelData.Data[np.QueueIndex] = avg;
            }
        }
    }
}*/