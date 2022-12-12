namespace AllColors.FirstRGBGen;

/// <summary>
/// Totally random (but detereministic) color sorting.
/// </summary>
public sealed class RandomComparer : IComparer<ARGB>
{
    private readonly Shuffler _shuffler;

    public RandomComparer(Shuffler shuffler)
    {
        _shuffler = shuffler;
    }

    public int Compare(ARGB left, ARGB right)
    {
        return _shuffler.Compare();
    }
}