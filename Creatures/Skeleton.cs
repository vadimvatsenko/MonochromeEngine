using MonochromeEngine.Engine;
using MonochromeEngine.Engine.BaseObjects;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Creatures;

public class Skeleton : GameObject, IUpdatable, IRenderable
{
    // Attack
    public BoxCollider2D CombatCollider { get; private set; }
    private Vector2 _attackOffset = new Vector2(20, 0); // Смещение зоны атаки
    private bool _isAttacking = false;
    private double _attackCooldownTimer = 0;
    private double _attackAnimTimer = 0;

    // Physics
    private Vector2 _velocity = new Vector2(30f, 0); // Начальная скорость вправо
    private Vector2 _gravity = new Vector2(0, 100f);


    public Skeleton(string name,
        Vector2 position,
        char[,] layer,
        Map map,
        MonoRenderer renderer,
        SpritesLoaderSystem spritesLoaderSystem) : base(name, position, layer, map, renderer, spritesLoaderSystem)
    {
        Animator.TargetAnimation = Animator.MoveBase;
        CombatCollider = new BoxCollider2D(Position, new Vector2(20, 16));
    }

    public void Update(double deltatime)
    {
        // Таймеры
        if (_attackCooldownTimer > 0) _attackCooldownTimer -= deltatime;
        if (_attackAnimTimer > 0) _attackAnimTimer -= deltatime;

        _isAttacking = _attackAnimTimer > 0;

        if (_isAttacking)
        {
            _velocity.X = 0;
            // Все равно применяем гравитацию, чтобы скелет не зависал в воздухе при атаке
            UpdatePhysics(deltatime);
            CollisionSystem.ResolveCollisions(ref Position, ref _velocity, new Vector2(XOffset, YOffset), deltatime);
        }
        else
        {
            UpdatePhysics(deltatime);
            // Разрешаем столкновения через единую систему
            CollisionSystem.ResolveCollisions(ref Position, ref _velocity, new Vector2(XOffset, YOffset), deltatime);

            // --- ЛОГИКА ОБНАРУЖЕНИЯ ПРОПАСТЕЙ ---
            if (CollisionSystem.IsGrounded)
            {
                // Проверяем блок впереди-внизу
                double checkX = Position.X + (IsFacingRight ? XOffset : 0);
                double checkY = Position.Y + YOffset + 2; // Чуть ниже ног

                if (!Map.IsBlockAt((int)checkX, (int)checkY))
                {
                    // Пропасть найдена! Разворачиваемся.
                    _velocity.X = -_velocity.X;
                    IsFacingRight = !IsFacingRight;
                }
            }
            // ------------------------------------

            // Логика разворота при ударе о стену
            if (Math.Abs(_velocity.X) < 0.1f)
            {
                _velocity.X = IsFacingRight ? -30f : 30f;
                IsFacingRight = !IsFacingRight;
            }
        }

        // Обновляем позицию боевого коллайдера
        CombatCollider.Position =
            new Vector2(Position.X + (IsFacingRight ? _attackOffset.X : -_attackOffset.X), Position.Y);

        // Проверка атаки
        if (Map.Hero != null && _attackCooldownTimer <= 0 && CombatCollider.IsColliding(Map.Hero.Collider))
        {
            Map.Hero.TakeDamage(10f); // Наносим урон
            _attackCooldownTimer = 1.5; // Перезарядка атаки
            _attackAnimTimer = 0.5; // Длительность анимации
            _velocity.X = 0;

        }

        Animator.UpdateAnimation(deltatime);
        SwitchAnimation();
    }

    public void Draw() => Animator.Draw(Position, IsFacingRight);

    private void SwitchAnimation()
    {
        if (_attackAnimTimer > 0)
            Animator.TargetAnimation = Animator.AttackBase;
        else if (Math.Abs(_velocity.X) > 0.5)
            Animator.TargetAnimation = Animator.MoveBase;
        else if (_velocity.X == 0)
            Animator.TargetAnimation = Animator.IdleBase;
        else if (_isAttacking)
        {
            Animator.TargetAnimation = Animator.AttackBase;
        }
    }

    private void UpdatePhysics(double deltatime)
    {
        _velocity = _velocity + (_gravity * 1.8f) * deltatime;
    }
}
