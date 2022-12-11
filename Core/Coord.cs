namespace AllColors;

[SkipLocalsInit]
[StructLayout(LayoutKind.Explicit, Size = 8)]
public readonly struct Coord : 
    IEquatable<Coord>,
    IEqualityOperators<Coord, Coord, bool>,
    IComparable<Coord>,
    IComparisonOperators<Coord, Coord, bool>,
    ISpanFormattable,
    IFormattable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Coord left, Coord right) => left._raw == right._raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Coord left, Coord right) => left._raw != right._raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Coord first, Coord second) => first.CompareTo(second) < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Coord first, Coord second) => first.CompareTo(second) <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Coord first, Coord second) => first.CompareTo(second) > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Coord first, Coord second) => first.CompareTo(second) >= 0;

    [FieldOffset(0)] 
    private readonly ulong _raw;

    [FieldOffset(0)]
    public readonly int X;

    [FieldOffset(4)]
    public readonly int Y;

    internal int Distance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => X + Y;
    }

    public Coord(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = this.X;
        y = this.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Coord coord)
    {
        int c = this.X.CompareTo(coord.X);
        if (c != 0) return c;
        c = this.Y.CompareTo(coord.Y);
        if (c != 0) return c;
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Coord coord) => coord._raw == _raw;

    public override bool Equals(object? obj) => obj is Coord coord && coord._raw == _raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine<int, int>(X, Y);

    public bool TryFormat(Span<char> destination, out int charsWritten, 
        ReadOnlySpan<char> format = default, 
        IFormatProvider? provider = default)
    {
        charsWritten = 0; // fast return
        int destLen = destination.Length;
        if (destLen < 5) return false;
        Span<char> buffer = new char[destLen];
        int b = 0;
        int wrote;
        buffer[b++] = '(';
        if (!X.TryFormat(buffer[b..], out wrote, format, provider))
            return false;
        b += wrote;
        if (b >= destLen - 3) return false;
        buffer[b++] = ',';
        if (!Y.TryFormat(buffer[b..], out wrote, format, provider))
            return false;
        b += wrote;
        if (b >= destLen) return false;
        buffer[b++] = ')';
        buffer[..b].CopyTo(destination);
        charsWritten = b;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider = default)
    {
        var stringHandler = new DefaultInterpolatedStringHandler(3, 2, formatProvider);
        stringHandler.AppendFormatted('(');
        stringHandler.AppendFormatted<int>(X, format);
        stringHandler.AppendFormatted(',');
        stringHandler.AppendFormatted<int>(Y, format);
        stringHandler.AppendFormatted(')');
        return stringHandler.ToString();
    }
    
    public override string ToString() => $"({X},{Y})";
}