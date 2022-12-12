namespace AllColors.FirstRGBGen;

public sealed class WeightedPixelBias : PixelBias
{
    protected override int CompareImpl(Pixel left, Pixel right)
    {
        return left.Weight.CompareTo(right.Weight);
    }
}