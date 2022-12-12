using System.Collections.Concurrent;
using System.Diagnostics;

namespace AllColors.FirstRGBGen;

/// <summary>
/// The queue contains filled pixels, which have at least one empty neighbor. In each step, we find the closest match to the new color in the
/// queue. Then we place the new color into a random empty neighbor. We use a squared difference metric.
/// </summary>
public class OneNeighborSqAlgorithm : AlgorithmBase
{
    private readonly PixelQueue _queue;
    private readonly PixelBias _bias;

    public override PixelQueue Queue
    {
        get { return _queue; }
    }

    public OneNeighborSqAlgorithm(Pixel[] imagePixels, int startIndex, PixelBias bias) 
        : base(imagePixels, startIndex)
    {
        _queue = new PixelQueue();
        _bias = bias;
    }

    private PixelDiff FindLowestDifference(ReadOnlySpan<Pixel?> pixels, ARGB color)
    {
        var minDiff = int.MaxValue;
        Pixel? bestPixel = null;
        for (var i = pixels.Length - 1; i >= 0; i--)
        {
            Pixel? pixel = pixels[i];
            if (pixel is not null)
            {
                var diff = color.Difference(pixel.Color);

                if (diff < minDiff || (diff == minDiff && _bias.Compare(pixel, bestPixel) < 0))
                {
                    minDiff = diff;
                    bestPixel = pixel;
                }
            }
        }

        return new PixelDiff(bestPixel, minDiff);
    }

    protected override Pixel PlaceImpl(ARGB color)
    {
        // What pixels do we have available
        Memory<Pixel?> available = new Memory<Pixel?>(
            array: _queue.AvailablePixels, 
            start: 0, 
            length: _queue.AvailablePixelLength);

        const int MaxSlice = 256;

        // Find the best fit
        Pixel bestPixel;

        if (available.Length <= MaxSlice)
        {
            var bestDiff = FindLowestDifference(available.Span, color);
            Debug.Assert(bestDiff.Pixel is not null);
            bestPixel = bestDiff.Pixel;
        }
        else
        {
            // find the best pixel with parallel processing
            var rangeSize = Math.Max(MaxSlice, available.Length / Environment.ProcessorCount);
            var bestDiff = Partitioner.Create(
                    fromInclusive: 0,
                    toExclusive: available.Length,
                    rangeSize: rangeSize)
                .AsParallel()
                .Min(rangeTuple =>
                {
                    Range range = new(start: rangeTuple.Item1, end: rangeTuple.Item2);
                    var slice = available.Span[range];
                    var bestDiff = FindLowestDifference(slice, color);
                    return bestDiff;
                });
            Debug.Assert(bestDiff.Pixel is not null);
            bestPixel = bestDiff.Pixel;
        }

        /*
        // Neighbors have already been randomized, so return the first that is empty
        var neighbors = bestPixel.Neighbors;
        for (var n = neighbors.Length - 1; n >= 0; n--)
        {
            var neighbor = neighbors[n];
            if (!neighbor.IsEmpty) continue;
            return neighbor;
        }
        */

        // select a deterministically random empty neighbor
        var neighbors = bestPixel.Neighbors;
        var neighborsLength = neighbors.Length;
        var shift = bestPixel.Weight % neighborsLength;
        for (var i = 0; i < neighborsLength; i++)
        {
            var neighbor = neighbors[(i + shift) % neighborsLength];
            if (neighbor.IsEmpty)
                return neighbor;
        }

        throw new Exception("NOT POSSIBLE");
    }

    protected override void ChangeQueue(Pixel pixel)
    {
        // Tracking a wavefront of available filled pixels!

        Pixel[] neighbors;

        // Check the Pixel's neighbors
        neighbors = pixel.Neighbors;
        for (var i = 0; i < neighbors.Length; i++)
        {
            var neighbor = neighbors[i];
            if (neighbor.IsEmpty)
            {
                // if pixel has an empty neighbor, it belongs in the queue
                if (pixel.QueueIndex == -1)
                {
                    _queue.TryAdd(pixel);
                }
            }
            else
            {
                // pixel has a filled neighbor,
                // and pixel was just filled,
                // so that neighbor may not belong to the queue anymore
                var ok = false;
                var nn = neighbor.Neighbors;
                for (var j = nn.Length - 1; j >= 0; j--)
                {
                    if (nn[j].IsEmpty)
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                {
                    _queue.TryRemove(neighbor);
                }
            }
        }
    }
}