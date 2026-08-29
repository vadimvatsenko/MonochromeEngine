using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Engine;

public class CollisionSystem
{
    private readonly Map _map;
    private readonly Collider2D _collider;
    
    public bool IsGrounded { get; private set; }

    public CollisionSystem(Map map, Collider2D collider)
    {
        _map = map;
        _collider = collider;
    }
    
    // Единый метод для разрешения столкновений
    public void ResolveCollisions(ref Vector2 position, ref Vector2 velocity, Vector2 size, double deltatime)
    {
        IsGrounded = false;
        
        // 1. Движение по X
        position.X += velocity.X * deltatime;
        _collider.Position = position;
        
        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                if (velocity.X > 0) // Движение вправо
                    position.X = block.Collider.Position.X - size.X;
                else if (velocity.X < 0) // Движение влево
                    position.X = block.Collider.Position.X + block.Collider.Size.X;
                
                velocity.X = 0;
                _collider.Position = position; // Обновляем коллайдер
            }
        }

        // 2. Движение по Y
        position.Y += velocity.Y * deltatime;
        _collider.Position = position;
        
        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                if (velocity.Y > 0) // Движение вниз (падаем)
                {
                    position.Y = block.Collider.Position.Y - size.Y;
                    IsGrounded = true;
                }
                else if (velocity.Y < 0) // Движение вверх (прыгаем)
                {
                    position.Y = block.Collider.Position.Y + block.Collider.Size.Y;
                }
                
                velocity.Y = 0;
                _collider.Position = position; // Обновляем коллайдер
            }
        }
    }
}