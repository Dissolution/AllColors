using System.Diagnostics;

namespace AllColors;

public readonly struct ColorSpace
{
    public static readonly ColorSpace Sixteen = new ColorSpace(16, 64, 64);
    public static readonly ColorSpace TwentyFour = new ColorSpace(24, 128, 108);
    public static readonly ColorSpace ThirtyTwo = new ColorSpace(32, 256, 128);
    public static readonly ColorSpace SixtyFour = new ColorSpace(64, 512, 512);
        
    public readonly byte Count;
    public readonly ushort Width;
    public readonly ushort Height;

    public ushort StartX => (ushort)(Width / 2f);
    public ushort StartY => (ushort)(Height / 2f);

    private ColorSpace(byte count, ushort width, ushort height)
    {
        this.Count = count;
        this.Width = width;
        this.Height = height;
        
        Debug.Assert(((int)count * (int)count * (int)count) == ((int)height * (int)width));
    }
}