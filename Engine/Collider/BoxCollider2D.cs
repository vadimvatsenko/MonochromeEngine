namespace MonochromeEngine.Engine.Collider;

public class BoxCollider2D : Collider2D
{
    public Vector2 Size { get; private set; }
    
    public BoxCollider2D(Vector2 position, Vector2 size) : base(position)
    {
        Size = size;
    }

    public override bool IsColliding(Collider2D other)
    {
        if (other is BoxCollider2D box)
        {
            float leftA = Position.X;
            float rightA = Position.X + Size.X;
            float topA = Position.Y;
            float bottomA = Position.Y + Size.Y;

            float leftB = box.Position.X;
            float rightB = box.Position.X + box.Size.X;
            float topB = box.Position.Y;
            float bottomB = box.Position.Y + box.Size.Y;

            // Проверяем, не находятся ли коллайдеры вне друг друга
            bool noCollision = rightA <= leftB || // A полностью слева от B
                               leftA >= rightB || // A полностью справа от B
                               bottomA <= topB || // A полностью выше B
                               topA >= bottomB; // A полностью ниже B

            return !noCollision;
        }

        throw new NotSupportedException("Intersection with this collider type is not supported.");
    }
}