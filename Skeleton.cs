using MonochromeEngine.Animation;
using MonochromeEngine.Collisions;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Skeleton : IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly BoxCollider2D _collider;
    private readonly BaseCollisionSystem _collisionSystem;
    private readonly Map _map;
    
    // Sprites
    private List<char[,]> _idleBase;
    private List<char[,]> _moveBase;
    private List<char[,]> _attackBase;
    private List<char[,]> _deadBase;
    private List<char[,]> _hitBase;

    private List<char[,]> _targetAnimation;

    // Movement
    private Vector2 _position = new Vector2(64, 80);
    private bool _isFacingRight = true;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;
    
    // animation settings
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.125;

    // Physics
    private Vector2 _velocity = new Vector2(30f, 0); // Начальная скорость вправо
    private Vector2 _gravity = new Vector2(0, 100f);
    
    public Skeleton(MonoRenderer renderer, char[,] layer, SpritesLoaderSystem spritesLoaderSystem,
        Map map)
    {
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;

        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));
        _map = map;
        _collisionSystem = new BaseCollisionSystem(map, _collider);

        if (_spritesLoaderSystem.Sprites.TryGetValue("Skeleton", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _moveBase = sprites.GetAnimationFrames("MoveBase");
            _attackBase = sprites.GetAnimationFrames("AttackBase");
            _hitBase =  sprites.GetAnimationFrames("HitBase");
            _deadBase = sprites.GetAnimationFrames("DeadBase");
        }

        _targetAnimation = _moveBase;
    }
    
    public void Update(double deltatime)
    {
        UpdatePhysics(deltatime);
        
        // Разрешаем столкновения через единую систему
        _collisionSystem.ResolveCollisions(ref _position, ref _velocity, new Vector2(_xOffset, _yOffset), deltatime);

        // --- ЛОГИКА ОБНАРУЖЕНИЯ ПРОПАСТЕЙ ---
        if (_collisionSystem.IsGrounded)
        {
            // Проверяем блок впереди-внизу
            double checkX = _position.X + (_isFacingRight ? _xOffset : 0);
            double checkY = _position.Y + _yOffset + 2; // Чуть ниже ног
            
            if (!_map.IsBlockAt((int)checkX, (int)checkY))
            {
                // Пропасть найдена! Разворачиваемся.
                _velocity.X = -_velocity.X;
                _isFacingRight = !_isFacingRight;
            }
        }
        // ------------------------------------

        // Логика разворота при ударе о стену
        if (Math.Abs(_velocity.X) < 0.1f) 
        {
            _velocity.X = _isFacingRight ? -30f : 30f;
            _isFacingRight = !_isFacingRight;
        }

        Animation(deltatime);
        SwitchAnimation();
    }

    private void SwitchAnimation()
    {
        if (Math.Abs(_velocity.X) > 0.5)
            _targetAnimation = _moveBase;
        else
            _targetAnimation = _idleBase;
    }
    
    private void Animation(double deltatime)
    {
        if (_targetAnimation == null || _targetAnimation.Count == 0) return;

        if (_spriteIndex >= _targetAnimation.Count) _spriteIndex = 0;

        var currentFrame = _targetAnimation[_spriteIndex];
        int height = currentFrame.GetLength(0);
        int width = currentFrame.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                int readX = !_isFacingRight ? (width - 2 - x) : x;

                char tileLeft = currentFrame[y, readX];
                char tileRight = currentFrame[y, readX + 1];

                if (tileLeft != ' ') _renderer.DrawChar(_layer, (int)(_position.X + x), (int)(y + _position.Y), tileLeft);
                if (tileRight != ' ') _renderer.DrawChar(_layer, (int)(x + 1 + _position.X), (int)(y + _position.Y), tileRight);
            }
        }

        _animTimer += deltatime;
        if (_animTimer >= FRAME_DURATION)
        {
            _spriteIndex = (_spriteIndex + 1) % _targetAnimation.Count;
            _animTimer = 0;
        }
    }

    private void UpdatePhysics(double deltatime)
    {
        _velocity = _velocity + (_gravity * 1.8f) * deltatime;
    }
}
