namespace AllColors.FirstRGBGen;

/// <summary>
/// Compares by hue first, then by brightness, and finally random.
/// </summary>
public sealed class HueComparer : IComparer<ARGB>
{
    private readonly int _shift;

    public HueComparer(int shift)
    {
        _shift = shift;
    }

    public int Compare(ARGB left, ARGB right)
    {
        var c = ((left.GetHue() + _shift) % 360).CompareTo((right.GetHue() + _shift) % 360);
        if (c == 0)
            c = left.GetBrightness().CompareTo(right.GetBrightness());
        //if (c == 0)
        //    c = _rndGen.Next(11) - 5;
        return c;
    }
}