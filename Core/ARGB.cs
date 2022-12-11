using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    public static ARGB[] AllRGBs { get; }

    static ARGB()
    {
        const int count = 256 * 256 * 256;
        ARGB[] colors = new ARGB[count];
        int c = 0;

        for (var r = 0; r < 256; r++)
        for (var g = 0; g < 256; g++)
        for (var b = 0; b < 256; b++)
        {
            colors[c++] = new ARGB(0, (byte)r, (byte)g, (byte)b);
        }

        Debug.Assert(c == count);

        AllRGBs = colors;
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

    public Color ToColor()
    {
        return Color.FromArgb(Alpha, Red, Green, Blue);
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
        hasher.AddBytes(MemoryMarshal.Cast<ARGB, byte>(colors));
        return hasher.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(ARGB[]? colors)
    {
        if (colors is null) return 0;
        var hasher = new HashCode();
        hasher.AddBytes(MemoryMarshal.Cast<ARGB, byte>(colors));
        return hasher.ToHashCode();
    }
}