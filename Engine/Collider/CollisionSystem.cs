using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.Collider;

public class CollisionSystem
{
    // На сколько строк (пикселей) можно подняться без прыжка: стык склона с ровным блоком,
    // скруглённые края тайлов. На столько же объект "прилипает" к земле при спуске
    private const double MaxStep = 4;

    private readonly Map _map;
    private readonly Collider2D _collider;

    // Стоял ли объект на склоне в прошлом кадре
    private bool _onSlope;

    public bool IsGrounded { get; private set; }

    public CollisionSystem(Map map, Collider2D collider)
    {
        _map = map;
        _collider = collider;
    }

    // Единый метод для разрешения столкновений
    public void ResolveCollisions(ref Vector2 position, ref Vector2 velocity, Vector2 size, double deltatime)
    {
        bool wasGrounded = IsGrounded;
        bool wasOnSlope = _onSlope;
        IsGrounded = false;
        _onSlope = false;

        // 1. Движение по X
        position.X += velocity.X * deltatime;
        _collider.Position = position;

        // На склоне передний край объекта выше его ног на полширины — это не стена
        double stepAllowed = MaxStep + (wasOnSlope ? size.X / 2 / LevelLoader.CharsPerPixelX : 0);

        foreach (var block in _map.Blocks)
        {
            if (!_collider.IsColliding(block.Collider)) continue;

            // Склон или невысокая ступенька — не стена: на неё поднимет проход по Y
            double bottom = position.Y + size.Y;
            if (bottom - SurfaceY(block, position.X + size.X / 2) <= stepAllowed) continue;

            if (velocity.X > 0) // Движение вправо
                position.X = block.Collider.Position.X - size.X;
            else if (velocity.X < 0) // Движение влево
                position.X = block.Collider.Position.X + block.Collider.Size.X;

            velocity.X = 0;
            _collider.Position = position; // Обновляем коллайдер
        }

        // 2. Движение по Y
        position.Y += velocity.Y * deltatime;
        _collider.Position = position;

        // Опорная точка — середина низа объекта: на склоне он стоит на линии под ней
        double footX = position.X + size.X / 2;
        double fallDepth = Math.Max(velocity.Y * deltatime, 0) + MaxStep;

        // Под серединой склон — держит только он. Иначе на спуске объект "висит" на краю
        // соседнего ровного блока, пока полностью с него не сойдёт
        bool slopeUnderFoot = HasSlopeUnder(footX, position.Y + size.Y);

        foreach (var block in _map.Blocks)
        {
            if (!_collider.IsColliding(block.Collider)) continue;
            if (slopeUnderFoot && block.SlopeHeights == null && !IsOver(block, footX)) continue;

            double bottom = position.Y + size.Y;
            double blockTop = block.Collider.Position.Y;
            double blockBottom = blockTop + block.Collider.Size.Y;

            if (block.SlopeHeights != null)
            {
                // Склон держит, только если под ним середина объекта и объект не летит вверх
                if (velocity.Y < 0 || !IsOver(block, footX)) continue;
                double surface = SurfaceY(block, footX);
                if (bottom < surface) continue; // ещё выше линии склона

                position.Y = surface - size.Y;
                velocity.Y = 0;
                IsGrounded = true;
                _onSlope = true;
            }
            else if (velocity.Y >= 0)
            {
                // Глубокое перекрытие сбоку (передняя часть на склоне заходит в соседний блок) —
                // не повод подбрасывать наверх. Если же середина объекта над блоком — он застрял
                // внутри (например, появился в блоке) и его, как и раньше, выталкивает наверх
                if (bottom - blockTop > fallDepth && !IsOver(block, footX)) continue;

                position.Y = blockTop - size.Y;
                velocity.Y = 0;
                IsGrounded = true;
            }
            else
            {
                // Удар головой — только если верх объекта зашёл в блок снизу
                if (position.Y <= blockTop || blockBottom - position.Y > -velocity.Y * deltatime + MaxStep) continue;

                position.Y = blockBottom;
                velocity.Y = 0;
            }

            _collider.Position = position; // Обновляем коллайдер
        }

        // 3. Шёл по земле и не прыгал — держим у земли при спуске по склону или на ступеньку вниз,
        // иначе на спуске объект каждый кадр чуть подлетает
        if (!IsGrounded && wasGrounded && velocity.Y >= 0)
        {
            StickToGround(ref position, ref velocity, size);
        }
    }

    private void StickToGround(ref Vector2 position, ref Vector2 velocity, Vector2 size)
    {
        double footX = position.X + size.X / 2;
        double bottom = position.Y + size.Y;
        GroundBlock? ground = null;
        double groundY = double.MaxValue;

        foreach (var block in _map.Blocks)
        {
            if (!IsOver(block, footX)) continue;
            double surface = SurfaceY(block, footX);
            if (surface >= bottom && surface - bottom <= MaxStep && surface < groundY)
            {
                ground = block;
                groundY = surface;
            }
        }

        if (ground == null) return;

        position.Y = groundY - size.Y;
        velocity.Y = 0;
        IsGrounded = true;
        _onSlope = ground.SlopeHeights != null;
        _collider.Position = position;
    }

    // Есть ли склон под точкой x рядом с низом объекта (на уровне ног или на шаг ниже)
    private bool HasSlopeUnder(double x, double bottom)
    {
        foreach (var block in _map.Blocks)
        {
            if (block.SlopeHeights == null || !IsOver(block, x)) continue;
            double top = block.Collider.Position.Y;
            if (top <= bottom + MaxStep && top + block.Collider.Size.Y >= bottom - MaxStep) return true;
        }
        return false;
    }

    private static bool IsOver(GroundBlock block, double x) =>
        x >= block.Collider.Position.X && x < block.Collider.Position.X + block.Collider.Size.X;

    // Где поверхность блока под точкой x: у обычного блока — его верх, у склона — по линии склона
    private static double SurfaceY(GroundBlock block, double x)
    {
        double top = block.Collider.Position.Y;
        if (block.SlopeHeights == null) return top;

        int column = (int)((x - block.Collider.Position.X) / LevelLoader.CharsPerPixelX);
        column = Math.Clamp(column, 0, block.SlopeHeights.Length - 1);
        return top + block.SlopeHeights[column];
    }

    public bool CheckCombatIntersection(Collider2D attackerCollider, Collider2D targetCollider)
    {
        return attackerCollider.IsColliding(targetCollider);
    }
}
