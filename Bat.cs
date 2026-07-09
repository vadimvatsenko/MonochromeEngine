using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;
using Ray = MonochromeEngine.Engine.Ray;

namespace MonochromeEngine;

public class Bat: IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    private BoxCollider2D _collider;
    
    // Sprites
    private List<char[,]> _idleBase;
    private List<char[,]> _deathBase;
    private List<char[,]> _hitBase;
    private List<char[,]> _targetAnimation;

    // Movement
    private Vector2 _position = new Vector2(0, 5);
    private int _stepX = 2;
    private readonly float _speed = 1f;
    private double _multiplier = 0;
    private bool _isFacingRight = false;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;

    // Ray
    private Ray _downGroundRay;
    private Ray _upGroundRay;
    private Ray _rightGroundRay;
    private Ray _leftGroundRay;
    private bool _isGroundup;
    private bool _isGroundright;
    private bool _isGroundleft;
    private bool _isGrounddown;
    
    // animation settings
    private bool _loop = false;
    private bool _isFinished = false;
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.125;
    // 

    // jump logic
    private Vector2 _velocity = new Vector2(0, 0);
    private Vector2 _gravity = new Vector2(0, 100f);
    private bool _isGround = false;

    //
    private double _multy = 0;

    public Bat(MonoRenderer renderer, char[,] layer, SpritesLoaderSystem spritesLoaderSystem, Map map)
    {
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;

        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));

        _downGroundRay = new Ray(_position, Vector2.Down, _yOffset);
        _upGroundRay = new Ray(_position, Vector2.Up, 1);
        _rightGroundRay = new Ray(_position, Vector2.Right, _xOffset);
        _leftGroundRay = new Ray(_position, Vector2.Left, 0);
        
        DrawRay();

        if (_spritesLoaderSystem.Sprites.TryGetValue("Bat", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _hitBase = sprites.GetAnimationFrames("HitBase");
            _deathBase = sprites.GetAnimationFrames("DeadBase");
        }

        _targetAnimation = _idleBase;
    }

    public void DrawRay()
    {
        
    }

    public void Update(double deltatime)
    {
        UpdatePhysics(deltatime);
        CheckGroundAndWalls();
        Animation(deltatime);
        Move(deltatime);
    }
    
    private void Animation(double deltatime)
    {
        if (_targetAnimation == null || _targetAnimation.Count == 0) return;

        // Корректируем индекс, если он почему-то вылетел за границы (например, при баге в другом месте)
        if (_spriteIndex >= _targetAnimation.Count)
        {
            _spriteIndex = 0;
        }

        var currentFrame = _targetAnimation[_spriteIndex];
        int height = currentFrame.GetLength(0);
        int width = currentFrame.GetLength(1); // Это уже удвоенная ширина (например, 32)

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2) // Идем шагом по 2, так как пиксель "двойной"
            {
                // Вычисляем правильный индекс чтения по X в зависимости от Flip
                int readX = !_isFacingRight ? (width - 2 - x) : x;

                // Берем пару символов, составляющих один консольный пиксель
                char tileLeft = currentFrame[y, readX];
                char tileRight = currentFrame[y, readX + 1];

                // Отрисовка левой половинки пикселя
                if (tileLeft != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(_position.X + x), (int)(y + _position.Y), tileLeft);
                }

                // Отрисовка правой половинки пикселя
                if (tileRight != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(x + 1 + _position.X), (int)(y + _position.Y), tileRight);
                }
            }
        }

        _animTimer += deltatime;

        if (_animTimer >= FRAME_DURATION)
        {
            _spriteIndex = (_spriteIndex + 1) % _targetAnimation.Count;

            _animTimer = 0;
        }
    }

    private void Move(double deltatime)
    {
        // Накапливаем время движения
        _multiplier += deltatime * _speed;

        // Используем while на случай, если лаг был очень большим и нужно сделать больше 1 шага
        while (_multiplier >= FRAME_DURATION)
        {
            // 1. Сначала проверяем, не упремся ли мы в стену НА СЛЕДУЮЩЕМ шаге (Look-ahead)
            int nextPositionX = (int)(_position.X + _stepX);

            if (nextPositionX >= _map.Width - _xOffset || nextPositionX <= 0)
            {
                _stepX = -_stepX; // Разворачиваемся ДО того, как физически сдвинулись
            }

            // 2. Теперь безопасно обновляем направление спрайта
            _isFacingRight = _stepX < 0;

            // 3. Делаем фактический шаг
            _position.X += _stepX;

            // 4. Правильно уменьшаем таймер, не теряя доли секунд
            _multiplier -= FRAME_DURATION;
        }
    }

    private void HandleMovement(double deltatime)
    {
        if (_isGroundright && _velocity.X > 0) return;
        if (_isGroundleft && _velocity.X < 0) return;

        _position.X += _velocity.X;
        _collider.Position = _position;
    }

    private void CheckGroundAndWalls()
    {
        _upGroundRay.UpdatePosition(_position);
        _downGroundRay.UpdatePosition(_position);
        _rightGroundRay.UpdatePosition(_position);
        _leftGroundRay.UpdatePosition(_position);

        _isGroundup = _map.Blocks.Any(s => _upGroundRay.CheckDetection(s.Collider));
        _isGroundright = _map.Blocks.Any(s => _rightGroundRay.CheckDetection(s.Collider));
        _isGroundleft = _map.Blocks.Any(s => _leftGroundRay.CheckDetection(s.Collider));
        _isGrounddown = _map.Blocks.Any(s => _downGroundRay.CheckDetection(s.Collider));

        /*Console.WriteLine(
            $"isGroundup: {_isGroundup} isGroundright: {_isGroundright} isGroundleft: {_isGroundleft} isGrounddown: {_isGrounddown}");
        Console.WriteLine($"VelocityX: {_velocity.X} VelocityY: {_velocity.Y}");
        Console.WriteLine($"PositionX: {_position.X} PositionY: {_position.Y}");*/


        if (_isGrounddown)
        {
            _isGround = true;
            _velocity.Y = 0;
        }
        else
        {
            _isGround = false;
        }
    }
    
    private void UpdatePhysics(double deltatime)
    {
        _collider.Position = new Vector2(_position.X, _position.Y);
    }
    
}