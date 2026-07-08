using MonochromeEngine.Engine.Collider;

namespace MonochromeEngine.Engine;

public class Ray
{
    public Vector2 Position { get; private set; }
    public Vector2 Direction { get; private set; }
    private int _length;

    public Ray(Vector2 position, Vector2 direction, int length)
    {
        Position = position;
        Direction = direction;
        _length = length;
    }

    public bool CheckDetection(Collider2D other)
    {
        for (int i = 1; i <= _length; i++) // i идёт от 1, чтобы не проверять свою же позицию
        {
            Vector2 tempPos = Position + Direction * i; // Смещаемся в направлении игрока
            BoxCollider2D tempColl = new BoxCollider2D(tempPos, Vector2.One);

            if (tempColl.IsColliding(other)) return true;
        }

        return false;
    }
    
    public void UpdateDirection(Vector2 direction) => Direction = direction;
    
    public void UpdatePosition(Vector2 position) => Position = position;
    
}