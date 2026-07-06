using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class BaseGameObject : IUpdatable, IDisposable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    private readonly Input _input;
    private BoxCollider2D _collider;

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
    private Vector2 _position = new Vector2(0, 0);
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
    private bool _isGround = false;
    
    public BaseGameObject(MonoRenderer renderer, Input input, char[,] layer, SpritesLoaderSystem spritesLoaderSystem, Map map)
    {
        _input = input;
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        
        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset + 1));

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
        _input.OnLeft += MoveLeft;
    }

    public void Update(double deltatime)
    {
        //Move(deltatime);
        //StopHorizontal();
        HandleMovement(deltatime);
        Animation(deltatime);
        UpdatePhysics(deltatime);
        
        if(_velocity.Y > 0.5)
        {
            _targetAnimation = _fallBase;
        }
        else if(_velocity.Y < -0.5)
        {
            _targetAnimation = _jumpBase;
        }
        else
        {
            _targetAnimation = _moveBase;
        }
        
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
        // --- 1. ДВИЖЕНИЕ И КОЛЛИЗИИ ПО ОСИ X ---
        _position.X += _velocity.X;
        _collider.Position = _position; // Обновляем коллайдер перед проверкой

        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                // Если врезались, двигаясь вправо -> выталкиваем влево к левой границе блока
                if (_velocity.X > 0) 
                    _position.X = block.Collider.Position.X - _collider.Size.X;
                // Если врезались, двигаясь влево -> выталкиваем вправо к правой границе блока
                else if (_velocity.X < 0) 
                    _position.X = block.Collider.Position.X + block.Collider.Size.X;

                _velocity.X = 0; // Останавливаем горизонтальное движение
                _collider.Position = _position; // Корректируем коллайдер
            }
        }

        // --- 2. ДВИЖЕНИЕ И КОЛЛИЗИИ ПО ОСИ Y ---
        _position.Y += _velocity.Y;
        _collider.Position = _position; // Обновляем коллайдер перед проверкой

        _isGround = false; // Сбрасываем флаг земли перед проверкой

        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                // Падаем вниз и ударяемся о верх блока (приземление на пол)
                if (_velocity.Y > 0)
                {
                    _position.Y = block.Collider.Position.Y - _collider.Size.Y;
                    _isGround = true;
                }
                // Летим вверх и ударяемся о низ блока (головой в потолок)
                else if (_velocity.Y < 0)
                {
                    _position.Y = block.Collider.Position.Y + block.Collider.Size.Y;
                }

                _velocity.Y = 0; // Останавливаем вертикальное движение
                _collider.Position = _position; // Корректируем коллайдер
            }
        }
    }

    private void Jump()
    {
        if (_isGround)
        {
            _velocity = new Vector2(_velocity.X, -3);
        }
    }
    
    private void UpdatePhysics(double deltatime)
    {
        Console.WriteLine($"{_isGround}");
        if (_isGround)
        {
            if (_velocity.Y > 0) 
            {
                _velocity.Y = 0; // На земле вертикальная скорость равна 0
            }
        }
        else
        {
            // 1. Сначала плавно увеличиваем скорость падения под действием гравитации
            // Умножаем на (float)deltatime, чтобы физика не зависела от FPS
            _velocity.Y += _gravity.Y * (float)deltatime;
        }
        
        Console.WriteLine($"Position: {_position.X}, {_position.Y} Velocity: {_velocity.X}, {_velocity.Y}");
        _collider.Position = new Vector2(_position.X, _position.Y);
    }

    private void MoveRight()
    {
        _velocity = new Vector2(_speed, _velocity.Y);
        _isFacingRight = true;
    }

    private void MoveLeft()
    {
        _velocity = new Vector2(-_speed, _velocity.Y);
        _isFacingRight = false;
    }
    private void StopHorizontal() => _velocity = new Vector2(0, _velocity.Y);

    public void Dispose()
    {
        _input.OnJump -= Jump;
        _input.OnRight -= MoveRight;
        _input.OnLeft -= MoveLeft;
    }
}

