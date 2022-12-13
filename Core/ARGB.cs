namespace AllColors;

[StructLayout(LayoutKind.Explicit, Size = 4)] // 4 bytes = sizeof(int)
[SkipLocalsInit]
public readonly struct ARGB : IEquatable<ARGB>,
    IEqualityOperators<ARGB, ARGB, bool>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ARGB(uint value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ARGB(Color color) => new(color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(ARGB argb) => argb.ToColor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ARGB left, ARGB right) => left.Value == right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ARGB left, ARGB right) => left.Value != right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MinMaxRgb(out int min, out int max, int red, int green, int blue)
    {
        if (red > green)
        {
            max = red;
            min = green;
        }
        else
        {
            max = green;
            min = red;
        }
        if (blue > max)
        {
            max = blue;
        }
        else if (blue < min)
        {
            min = blue;
        }
    }

    [FieldOffset(0)]
    internal readonly uint Value;

    [FieldOffset(0)]
    public readonly byte Blue;

    [FieldOffset(1)]
    public readonly byte Green;

    [FieldOffset(2)]
    public readonly byte Red;

    [FieldOffset(3)]
    public readonly byte Alpha;

    public ARGB(uint value)
    {
        Value = value;
    }

    public ARGB(byte alpha, byte red, byte green, byte blue)
    {
        Blue = blue;
        Green = green;
        Red = red;
        Alpha = alpha;
    }

    public ARGB(Color color)
    {
        Value = (uint)color.ToArgb();
    }

    public ARGB(int red, int green, int blue)
    {
        Blue = (byte)blue;
        Green = (byte)green;
        Red = (byte)red;
        Alpha = default;
    }

    public float GetBrightness()
    {
        MinMaxRgb(out int min, out int max, Red, Green, Blue);
        return (max + min) / (byte.MaxValue * 2.0f);
    }

    public float GetHue()
    {
        int r = Red;
        int g = Green;
        int b = Blue;

        if (r == g && g == b) return 0.0f;

        MinMaxRgb(out int min, out int max, r, g, b);

        float delta = max - min;
        float hue;

        if (r == max)
            hue = (g - b) / delta;
        else if (g == max)
            hue = (b - r) / delta + 2.0f;
        else
            hue = (r - g) / delta + 4.0f;

        hue *= 60.0f;
        if (hue < 0.0f)
            hue += 360.0f;

        return hue;
    }

    public float GetSaturation()
    {
        int r = Red;
        int g = Green;
        int b = Blue;

        if (r == g && g == b)
            return 0f;

        MinMaxRgb(out int min, out int max, r, g, b);

        int div = max + min;
        if (div > byte.MaxValue)
            div = (byte.MaxValue * 2) - max - min;

        return (max - min) / (float)div;
    }

    public Color ToColor()
    {
        return Color.FromArgb(Alpha, Red, Green, Blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Difference(ARGB color)
    {
        int rd = Red - color.Red;
        int gd = Green - color.Green;
        int bd = Blue - color.Blue;
        return (rd * rd) + (gd * gd) + (bd * bd);
    }

    public bool Equals(ARGB argb)
    {
        return Value == argb.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ARGB argb && argb.Value == Value;
    }

    public override int GetHashCode()
    {
        return (int)Value;
    }

    public override string ToString()
    {
        return $"({Alpha},{Red},{Green},{Blue})";
    }
}

public sealed class ARGBComparer : 
    IEqualityComparer<ARGB>,
    IEqualityComparer<ARGB[]>
{
    public static ARGBComparer Instance = new ARGBComparer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ARGB left, ARGB right)
    {
        return left.Value == right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlySpan<ARGB> left, ReadOnlySpan<ARGB> right)
    {
        return MemoryExtensions.SequenceEqual<ARGB>(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ARGB[]? left, ARGB[]? right)
    {
        return MemoryExtensions.SequenceEqual<ARGB>(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(ARGB color)
    {
        return color.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(ReadOnlySpan<ARGB> colors)
    {
        var hasher = new HashCode();
        hasher.AddBytes(MemoryMarshal.AsBytes<ARGB>(colors));
        return hasher.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(ARGB[]? colors)
    {
        if (colors is null) return 0;
        var hasher = new HashCode();
        hasher.AddBytes(MemoryMarshal.AsBytes<ARGB>(colors));
        return hasher.ToHashCode();
    }
}