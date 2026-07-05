using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class BaseGameObject : IUpdatable, IDisposable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    private readonly Input _input;

    // Sprites
    private List<char[,]> _idleBase;
    private List<char[,]> _moveBase;
    private List<char[,]> _attackBase;
    private List<char[,]> _deathBase;
    private List<char[,]> _jumpBase;
    private List<char[,]> _fallBase;
    private List<char[,]> _hitBase;
    private List<char[,]> _bowBase;
    private List<char[,]> _targetAnimation;

    // Movement
    private Vector2 _position = new Vector2(0, 20);
    private int _stepX = 2;
    private readonly float _speed = 2f;
    private double _multiplier = 0;
    private bool _isFacingRight = true;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;
    

    // animation settings
    private bool _loop = false;
    private bool _isFinished = false;
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.125;
    // 
    
    // jump logic
    private Vector2 _velocity = new Vector2(0, 0);
    private Vector2 _gravity = new Vector2(0, 9.81f);
    
    public BaseGameObject(MonoRenderer renderer, Input input, char[,] layer, SpritesLoaderSystem spritesLoaderSystem, Map map)
    {
        _input = input;
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;

        if (_spritesLoaderSystem.Sprites.TryGetValue("Hero", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _moveBase = sprites.GetAnimationFrames("MoveBase");
            _bowBase = sprites.GetAnimationFrames("BowBase");
            _jumpBase = sprites.GetAnimationFrames("JumpBase");
            _fallBase = sprites.GetAnimationFrames("FallBase");
        }

        _targetAnimation = _moveBase;

        _input.OnJump += Jump;
        _input.OnRight += MoveRight;
        _input.OnRight += MoveLeft;
    }

    public void Update(double deltatime)
    {
        //StopHorizontal();
        //Move(deltatime);
        HandleMovement(deltatime);
        Animation(deltatime);
        UpdatePhysics(deltatime);
        
        if(_velocity.Y > 0)
        {
            _targetAnimation = _fallBase;
        }

        if (_velocity.Y < 0)
        {
            _targetAnimation = _jumpBase;
        }
    }

    private void Animation(double deltatime)
    {
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
                    _renderer.DrawChar(_layer, (int)(_position.X + x) , (int)(y + _position.Y), tileLeft);
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
        _position = _position + _velocity * deltatime;
    }

    public void Jump()
    {
        Console.WriteLine("Jumping");
        _velocity = new Vector2(_velocity.X, -20);
    }
    
    private void UpdatePhysics(double deltatime)
    {
        _position = _position + _velocity * deltatime;
        _velocity = _velocity + _gravity * deltatime;
        
        Console.WriteLine($"Position: {_position.X}, {_position.Y} Velocity: {_velocity.X}, {_velocity.Y}");
    }

    private void MoveRight()
    {
        _velocity = new Vector2(_speed, _velocity.Y);
        Console.WriteLine(_velocity.X);
    }

    private void MoveLeft()
    {
        _velocity = new Vector2(-_speed, _velocity.Y);
        Console.WriteLine("Left");
    }
    private void StopHorizontal() => _velocity = new Vector2(0, _velocity.Y);

    public void Dispose()
    {
        _input.OnJump -= Jump;
        _input.OnRight -= MoveRight;
        _input.OnRight -= MoveLeft;
    }
}

