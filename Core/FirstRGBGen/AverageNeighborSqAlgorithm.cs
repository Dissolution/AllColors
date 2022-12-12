/*using System.Collections.Concurrent;

namespace AllColors.FirstRGBGen;

/// <summary>
/// Almost like <see cref="AverageNeighborAlgorithm"/> (also lots of copypaste). The difference is that it uses a squared difference. This
/// leaves lots of gaps between differently colored branches, so it grows like a coral, which is very cool. The downside is that the queue
/// gets very big. It also has a blurred effect, but with some more rough edges.
/// </summary>
public class AverageNeighborSqAlgorithm : AlgorithmBase
{
    /// <summary>
    /// Let's say we have three neighbors: A, B, C. We want to add a fourth color, D. The squared difference for one channel is
    /// (D-A)^2+(D-B)^2+(D-C)^2. But for this, we need to read every neighbor. We need to make it quick by using precalculation. So we
    /// rearrange the formula like this: 3*D^2+((A^2+B^2+C^2)-2D(A+B+C)). Now we can precalculate the values which depend on the neighbors.
    /// This structure contains these values.
    /// </summary>
    private struct AvgInfo
    {
        // sum of neighbors
        public int R, G, B;

        // squared sum of neighbors
        public int RSq, GSq, BSq;

        // number of neighbors
        public int Num;
    }

    private PixelQueue<AvgInfo> _queue = new PixelQueue<AvgInfo>();

    public override PixelQueue Queue
    {
        get { return _queue; }
    }

    public AverageNeighborSqAlgorithm(Pixel[] imagePixels, int startIndex) : base(imagePixels, startIndex)
    {
    }

    protected override Pixel PlaceImpl(ARGB c)
    {
        // find the best pixel with parallel processing
        var q = _queue.Pixels;
        var best = Partitioner.Create(0, _queue.UsedUntil, Math.Max(256, _queue.UsedUntil / Program2.Threads))
            .AsParallel()
            .Min(range =>
            {
                var bestdiff = int.MaxValue;
                Pixel bestpixel = default;
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var qp = q[i];
                    if (qp.HasValue)
                    {
                        var avg = _queue.Data[qp.Value.QueueIndex];
                        var cr = (int)c.Red;
                        var cg = (int)c.Green;
                        var cb = (int)c.Blue;
                        var rd = cr * cr * avg.Num + (avg.RSq - 2 * cr * avg.R);
                        var gd = cg * cg * avg.Num + (avg.GSq - 2 * cg * avg.G);
                        var bd = cb * cb * avg.Num + (avg.BSq - 2 * cb * avg.B);
                        var diff = rd + gd + bd;
                        // we have to use the same comparison as PixelWithValue!
                        if (diff < bestdiff || (diff == bestdiff && qp.Value.Pos < bestpixel.Pos))
                        {
                            bestdiff = diff;
                            bestpixel = qp.Value;
                        }
                    }
                }

                return new PixelWithValue
                {
                    Pixel = bestpixel,
                    Value = bestdiff
                };
            }).Pixel!.Value;

        // found the pixel, return it
        _queue.Remove(best);
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
                int r = 0, g = 0, b = 0, rsq = 0, gsq = 0, bsq = 0, n = 0;
                for (var j = 0; j < np.Neighbors.Length; j++)
                {
                    var nnp = np.Neighbors[j];
                    if (!nnp.Empty)
                    {
                        var nr = (int)nnp.Color.Red;
                        var ng = (int)nnp.Color.Green;
                        var nb = (int)nnp.Color.Blue;
                        r += nr;
                        g += ng;
                        b += nb;
                        n++;
                        rsq += nr * nr;
                        gsq += ng * ng;
                        bsq += nb * nb;
                    }
                }

                var avg = new AvgInfo
                {
                    R = r,
                    G = g,
                    B = b,
                    RSq = rsq,
                    GSq = gsq,
                    BSq = bsq,
                    Num = n
                };
                if (np.QueueIndex == -1)
                    _queue.Add(np);
                _queue.Data[np.QueueIndex] = avg;
            }
        }
    }
}*/