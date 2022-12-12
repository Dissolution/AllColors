namespace AllColors.Thrice;

public struct ImagePixel : 
    IEquatable<ImagePixel>,
    IEqualityOperators<ImagePixel, ImagePixel, bool>
{
    public static bool operator ==(ImagePixel left, ImagePixel right)
    {
        return left.Position == right.Position;
    }

    public static bool operator !=(ImagePixel left, ImagePixel right)
    {
        return left.Position != right.Position;
    }

    public readonly Coord Position;

    public int QueueIndex;

    public bool IsEmpty;

    public ARGB Color;

    public ImagePixel[] Neighbors;

    public ImagePixel(int x, int y)
    {
        this.Position = new Coord(x, y);
        this.QueueIndex = -1;
        this.IsEmpty = true;
        this.Color = default;
        this.Neighbors = Array.Empty<ImagePixel>();
    }
    
    public bool Equals(ImagePixel imagePixel)
    {
        return imagePixel.Position == this.Position;
    }

    public override bool Equals(object? obj)
    {
        return obj is ImagePixel imagePixel && imagePixel.Position == this.Position;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public override string ToString()
    {
        if (IsEmpty)
            return $"{Position}: [EMPTY]";
        return $"{Position}: {Color}";
    }
}