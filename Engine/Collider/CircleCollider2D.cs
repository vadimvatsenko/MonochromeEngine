namespace MonochromeEngine.Engine.Collider;

public class CircleCollider2D : Collider2D
{
    public float Radius { get; private set; }

    public CircleCollider2D(Vector2 position, float radius) : base(position)
    {
        Radius = radius;
    }

    public override bool IsColliding(Collider2D other)
    {
        if (other is CircleCollider2D circle)
        {
            float distance = (Position - circle.Position).Magnitude();
            return distance <= (Radius + circle.Radius);
        }

        if (other is BoxCollider2D box)
        {
            // Проверка пересечения круга с прямоугольником
            float closestX = Math.Clamp(Position.X, box.Position.X, box.Position.X + box.Size.X);
            float closestY = Math.Clamp(Position.Y, box.Position.Y, box.Position.Y + box.Size.Y);

            float distance = (new Vector2((int)closestX, (int)closestY) - Position).Magnitude();
            return distance <= Radius;
        }

        throw new NotSupportedException("Intersection with this collider type is not supported.");
    }
}