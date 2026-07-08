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

    private AdvanceInput _advanceInput;

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
    private Vector2 _position = new Vector2(32, 0);
    private int _stepX = 2;
    private readonly float _speed = 1f;
    private double _multiplier = 0;
    private bool _isFacingRight = true;
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
    
    public BaseGameObject(MonoRenderer renderer, Input input, AdvanceInput advanceInput, char[,] layer, SpritesLoaderSystem spritesLoaderSystem,
        Map map)
    {
        _advanceInput = advanceInput;
        _input = input;
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;

        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));

        _downGroundRay = new Ray(_position, Vector2.Down, _yOffset); // OK
        _upGroundRay = new Ray(_position, Vector2.Up, 1);
        _rightGroundRay = new Ray(new Vector2(_position.X + _xOffset / 2, _position.Y), Vector2.Right, _xOffset / 2); // OK
        _leftGroundRay = new Ray(_position, Vector2.Left, 0);

        if (_spritesLoaderSystem.Sprites.TryGetValue("Hero", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _moveBase = sprites.GetAnimationFrames("MoveBase");
            _bowBase = sprites.GetAnimationFrames("BowBase");
            _jumpBase = sprites.GetAnimationFrames("JumpBase");
            _fallBase = sprites.GetAnimationFrames("FallBase");
            _attackBase = sprites.GetAnimationFrames("AttackBase");
            _deathBase = sprites.GetAnimationFrames("DeadBase");
        }

        _targetAnimation = _moveBase;

        //_input.OnJump += Jump;
        //_input.OnRight += MoveRight;
        //_input.OnLeft += MoveLeft;
        _input.OnDown += MoveDown;
        _input.OnAttack += Attack;

        /*_advanceInput.OnRightDown += MoveRight;
        _advanceInput.OnRightHold += MoveRight;
        _advanceInput.OnLeftDown += MoveLeft;
        _advanceInput.OnLeftHold += MoveLeft;*/
    }
    
    

    public void Update(double deltatime)
    {
        //Move(deltatime);
        //StopHorizontal();
        
        
        UpdatePhysics(deltatime);
        
        HandleMovement(deltatime);
        CheckGroundAndWalls();
        Animation(deltatime);
        
       

        if (_velocity.Y > 0.5)
        {
            _targetAnimation = _fallBase;
        }
        else if (_velocity.Y < -0.5)
        {
            _targetAnimation = _jumpBase;
        }
        else if (_velocity.X < -0.5 || _velocity.X > 0.5)
        {
            _targetAnimation = _moveBase;
        }
        else if(_isGround)
        {
            _targetAnimation = _idleBase;
        }

    }

    private void Attack()
    {
        _targetAnimation = _deathBase;
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
        if (_isGroundright && _velocity.X > 0)
        {
            _targetAnimation = _idleBase;
            return;
        }

        if (_isGroundright && _velocity.X < 0)
        {
            _targetAnimation = _idleBase;
            return;
        }

        

        _position.X += _velocity.X;
        _collider.Position = _position;
    }

    private void CheckGroundAndWalls()
    {
        _upGroundRay.UpdatePosition(_position);
        _downGroundRay.UpdatePosition(_position);
        _rightGroundRay.UpdatePosition(new Vector2(_position.X + _xOffset / 2, _position.Y));
        _leftGroundRay.UpdatePosition(_position);

        _isGroundup = _map.Blocks.Any(s => _upGroundRay.CheckDetection(s.Collider));
        _isGroundright = _map.Blocks.Any(s => _rightGroundRay.CheckDetection(s.Collider));
        _isGroundleft = _map.Blocks.Any(s => _leftGroundRay.CheckDetection(s.Collider));
        _isGrounddown = _map.Blocks.Any(s => _downGroundRay.CheckDetection(s.Collider));
        
        Vector2 direction = _isFacingRight? Vector2.Right : Vector2.Left;
        int offset = _isFacingRight ? _xOffset : -_xOffset;
        _rightGroundRay.UpdateDirection(direction);
        
        _renderer.DrawLine(_layer, (int)_rightGroundRay.Position.X, 
            (int)_rightGroundRay.Position.Y, 
            (int)_rightGroundRay.Position.X + (offset / 2), 
            (int)_rightGroundRay.Position.Y, '▒');
        
        _renderer.DrawLine(_layer, (int)_downGroundRay.Position.X, 
            (int)_downGroundRay.Position.Y, 
            (int)_downGroundRay.Position.X, 
            (int)_downGroundRay.Position.Y + _yOffset, '▒');
        

        Console.WriteLine(
            $"isGroundup: {_isGroundup} isGroundright: {_isGroundright} isGroundleft: {_isGroundleft} isGrounddown: {_isGrounddown}");
        Console.WriteLine($"VelocityX: {_velocity.X} VelocityY: {_velocity.Y}");
        Console.WriteLine($"PositionX: {_position.X} PositionY: {_position.Y}");


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

    private void Jump()
    {
        if (_isGround && _advanceInput.IsPressedNow(_advanceInput.MaskJump))
        {
            _velocity = new Vector2(_velocity.X, -60);
            _isGround = false;
        }
    }

    private void UpdatePhysics(double deltatime)
    {
        if (_isGround) return;

        _position = _position + _velocity * deltatime;

        // Если _velocity.Y > 0, значит персонаж уже прошел пик прыжка и падает.
        // В этот момент мы умножаем гравитацию на 1.5 или 2, чтобы он падал быстрее!
        if (_velocity.Y > 0)
        {
            _velocity = _velocity + (_gravity * 1.8f) * deltatime;
        }
        else
        {
            _velocity = _velocity + (_gravity * 1.8f) * deltatime;
        }
    
        _collider.Position = new Vector2(_position.X, _position.Y);
    }




    private void MoveRight()
    {
        if (_advanceInput.IsHeld(_advanceInput.MaskRight))
        {
            _velocity = new Vector2(_speed, _velocity.Y);
            _isFacingRight = true;
        }
        
    }

    private void MoveLeft()
    {
        if (_advanceInput.IsHeld(_advanceInput.MaskLeft))
        {
            _velocity = new Vector2(-_speed, _velocity.Y);
            _isFacingRight = false;
        }
        
    }

    private void MoveDown()
    {
        _velocity = new Vector2(0, _velocity.Y);
    }

    private void StopHorizontal() => _velocity = new Vector2(0, _velocity.Y);

    public void Dispose()
    {
        //_input.OnJump -= Jump;
        //_input.OnRight -= MoveRight;
        //_input.OnLeft -= MoveLeft;
        _input.OnDown -= MoveDown;
        _input.OnAttack -= Attack;

        /*_advanceInput.OnRightDown -= MoveRight;
        _advanceInput.OnRightHold -= MoveRight;
        _advanceInput.OnLeftDown -= MoveLeft;
        _advanceInput.OnLeftHold -= MoveLeft;*/
    }
}

