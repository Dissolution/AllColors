using System;
using System.Drawing;
using System.Net;

namespace AllColors.Thrice;


public sealed class ImageCell : IEquatable<ImageCell>
{
    public Coord Position { get; }

    public ImageCell[] Neighbors { get; internal set; }

    public bool IsEmpty => !Color.HasValue;

    public Color? Color { get; set; }

    public ImageCell(Coord position)
    {
        this.Position = position;
        this.Color = null;
        this.Neighbors = Array.Empty<ImageCell>();
    }

    public bool HasColor(out Color color)
    {
        if (Color.HasValue)
        {
            color = Color.Value;
            return true;
        }

        color = default;
        return false;
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
        var stringHandler = new DefaultInterpolatedStringHandler();
        stringHandler.AppendFormatted<Coord>(Position);
        stringHandler.AppendLiteral(": ");
        if (Color.HasValue)
        {
            var color = Color.Value;

            stringHandler.AppendFormatted<byte>(color.R);
            stringHandler.AppendLiteral(",");
            stringHandler.AppendFormatted<byte>(color.G);
            stringHandler.AppendLiteral(",");
            stringHandler.AppendFormatted<byte>(color.B);

            if (color.IsKnownColor)
            {
                stringHandler.AppendLiteral(" (");
                stringHandler.AppendFormatted<KnownColor>(color.ToKnownColor());
                stringHandler.AppendLiteral(")");
            }
        }
        else
        {
            stringHandler.AppendLiteral("Empty");
        }

        return stringHandler.ToStringAndClear();
    }
}

