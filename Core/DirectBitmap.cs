using System.Drawing;
using System.Drawing.Imaging;
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

public sealed class DirectBitmap : IDisposable
{
    private ARGB[] _argbAlloc;
    private GCHandle _allocHandle;
    private Bitmap _bitmap;

    public Bitmap Bitmap => _bitmap;
    
    public int Width { get; }
    public int Height { get; }

    public ref ARGB this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _argbAlloc[x + (y * Width)];
    }

    public DirectBitmap(int width, int height)
    {
        Width = width;
        Height = height;

        // Allocate enough space for this bitmap
        _argbAlloc = new ARGB[width * height];
        _allocHandle = GCHandle.Alloc(_argbAlloc, GCHandleType.Pinned);
        
        // Create the bitmap
        _bitmap = new Bitmap(width, height,
            width * 4,
            PixelFormat.Format32bppArgb,
            _allocHandle.AddrOfPinnedObject());
    }

    public void SetPixel(int x, int y, Color color)
    {
        int index = x + (y * Width);
        _argbAlloc[index] = color;
    }

    public Color GetPixel(int x, int y)
    {
        int index = x + (y * Width);
        return _argbAlloc[index];
    }

    public void Dispose()
    {
        _bitmap.Dispose();
        _allocHandle.Free();
    }
}