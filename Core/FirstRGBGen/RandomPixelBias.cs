namespace AllColors.FirstRGBGen;

public sealed class RandomPixelBias : PixelBias
{
    private readonly Shuffler _shuffler;

    public RandomPixelBias(Shuffler shuffler)
    {
        _shuffler = shuffler;
    }

    protected override int CompareImpl(Pixel left, Pixel right)
    {
        return _shuffler.Compare();
    }
}