using System.Collections.Concurrent;

namespace AllColors.FirstRGBGen;

/// <summary>
/// The queue contains empty pixels which have at least one filled neighbor. For every pixel in the queue, we calculate the average color of
/// its neighbors. In each step, we find the average that matches the new color the most. Uses a linear difference metric. It gives a blurred
/// effect.
/// </summary>
public class AverageNeighborAlgorithm : AlgorithmBase
{
    private readonly PixelQueue<ARGB> _pixelQueue = new();

    public override PixelQueue Queue
    {
        get { return _pixelQueue; }
    }

    public AverageNeighborAlgorithm(Pixel[] imagePixels, int startIndex) : base(imagePixels, startIndex)
    {
    }

    protected override Pixel PlaceImpl(ARGB c)
    {
        // find the best pixel with parallel processing
        var q = _pixelQueue.Pixels;
        var best = Partitioner.Create(0, _pixelQueue.EndLength, Math.Max(256, _pixelQueue.EndLength / Environment.ProcessorCount))
            .AsParallel()
            .Min(range =>
            {
                var bestdiff = int.MaxValue;
                Pixel bestpixel = null;
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var qp = q[i];
                    if (qp != null)
                    {
                        var avg = _pixelQueue.Data[qp.QueueIndex];
                        var rd = (int)avg.Red - c.Red;
                        var gd = (int)avg.Green - c.Green;
                        var bd = (int)avg.Blue - c.Blue;
                        var diff = rd * rd + gd * gd + bd * bd;
                        // we have to use the same comparison as PixelWithValue!
                        if (diff < bestdiff || (diff == bestdiff && qp.Weight < bestpixel.Weight))
                        {
                            bestdiff = diff;
                            bestpixel = qp;
                        }
                    }
                }

                return new PixelDiff(bestpixel, bestdiff);
            }).Pixel!;

        // found the pixel, return it
        _pixelQueue.TryRemove(best);
        return best;
    }

    protected override void ChangeQueue(Pixel p)
    {
        // recalculate the neighbors
        for (var i = 0; i < p.Neighbors.Length; i++)
        {
            var np = p.Neighbors[i];
            if (np.IsEmpty)
            {
                int r = 0, g = 0, b = 0, n = 0;
                for (var j = 0; j < np.Neighbors.Length; j++)
                {
                    var nnp = np.Neighbors[j];
                    if (!nnp.IsEmpty)
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
                    _pixelQueue.TryAdd(np);
                _pixelQueue.Data[np.QueueIndex] = avg;
            }
        }
    }
}