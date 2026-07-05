namespace MonochromeEngine.Engine;

public struct Vector2
{
    public float X;
    public float Y;

    public static Vector2 Zero => new Vector2(0, 0);
    public static Vector2 Up => new Vector2(0, -1);
    public static Vector2 Down => new Vector2(0, 1);
    public static Vector2 Right => new Vector2(1, 0);
    public static Vector2 Left => new Vector2(-1, 0);
    public static Vector2 One => new Vector2(1, 1);
    public static Vector2 Tree => new Vector2(3, 3);
    
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 Clamp(Vector2 target, Vector2 min, Vector2 max)
    {
        float x = Math.Clamp(target.X, min.X, max.X);
        float y = Math.Clamp(target.Y, min.Y, max.Y);
        return new Vector2(x, y);
    }
       

    public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);
    public static Vector2 operator +(Vector2 left, int right) => new(left.X + right, left.Y + right);
    public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);
    public static bool operator ==(Vector2 left, Vector2 right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);
    public static Vector2 operator *(Vector2 left, int numb) => new Vector2(left.X * numb, left.Y * numb);
    public static Vector2 operator /(Vector2 left, int numb) => new Vector2(left.X / numb, left.Y / numb);
    public static Vector2 operator *(Vector2 left, double numb) => new Vector2(left.X * (float)numb, left.Y * (float)numb);
    public static bool operator <(Vector2 left, Vector2 right) => left.X < right.X && left.Y < right.Y;
    public static bool operator >(Vector2 left, Vector2 right) => left.X > right.X && left.Y > right.Y;
    public static bool operator <=(Vector2 left, Vector2 right) => left.X <= right.X && left.Y <= right.Y;
    public static bool operator >=(Vector2 left, Vector2 right) => left.X >= right.X && left.Y >= right.Y;
    
    
    public float Magnitude() => MathF.Sqrt(X * X + Y * Y); // наскільки довгий вектор. направлення
    
    public override bool Equals(object? obj)
    {
        if(obj is not Vector2 otherCell) return false;
        return X == otherCell.X && Y == otherCell.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}