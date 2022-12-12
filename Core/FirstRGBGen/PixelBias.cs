namespace AllColors.FirstRGBGen;

public abstract class PixelBias : IComparer<Pixel>
{
    protected abstract int CompareImpl(Pixel left, Pixel right);

    public int Compare(Pixel? left, Pixel? right)
    {
        // We want to sort nulls last!
        if (left is null) return right is null ? 0 : 1;
        if (right is null) return 1;
        return CompareImpl(left, right);
    }
}