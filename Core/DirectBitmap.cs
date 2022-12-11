using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AllColors;

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