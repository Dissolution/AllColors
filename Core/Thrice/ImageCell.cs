namespace AllColors.Thrice;

public sealed class ImageCell : IChildItem,
    IEquatable<ImageCell>,
    IEqualityOperators<ImageCell, ImageCell, bool>
{
    public static bool operator ==(ImageCell? left, ImageCell? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Position == right.Position;
    }

    public static bool operator !=(ImageCell? left, ImageCell? right)
    {
        if (ReferenceEquals(left, right)) return false;
        if (left is null || right is null) return true;
        return left.Position != right.Position;
    }

    public Coord Position { get; }
    
    public int ListIndex { get; set; }

    public bool IsEmpty { get; private set; }

    public ARGB Color { get; private set; }

    public ImageCell[] Neighbors { get; internal set; }

    public ImageCell(Coord position)
    {
        this.Position = position;
        this.ListIndex = -1;
        this.IsEmpty = true;
        this.Color = default;
        this.Neighbors = Array.Empty<ImageCell>();
    }

    public void SetColor(ARGB color)
    {
        this.Color = color;
        this.IsEmpty = false;
    }

    public void ClearColor()
    {
        this.Color = default;
        this.IsEmpty = true;
    }
    
    public bool Equals(ImageCell? imageCell)
    {
        return imageCell is not null && imageCell.Position == this.Position;
    }

    public override bool Equals(object? obj)
    {
        return obj is ImageCell imageCell && imageCell.Position == this.Position;
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

