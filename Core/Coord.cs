using System.Numerics;

namespace AllColors;

public readonly struct Coord : IEquatable<Coord>,
    IEqualityOperators<Coord, Coord, bool>
{
    public static bool operator ==(Coord left, Coord right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(Coord left, Coord right) => left.X != right.X || left.Y != right.Y;
    
    public readonly int X;
    public readonly int Y;
    
    public Coord(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public bool Equals(Coord coord)
    {
        return this.X == coord.X && this.Y == coord.Y;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Coord coord && this.X == coord.X && this.Y == coord.Y;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine<int, int>(X, Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}