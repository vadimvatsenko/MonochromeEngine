using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class BaseGameObject : IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    private readonly Input _input;
    private readonly BoxCollider2D _collider;
    
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
    private Vector2 _position = new Vector2(32, 100);
    private int _stepX = 2;
    private readonly float _speed = 6f;
    private double _multiplier = 0;
    private bool _isFacingRight = true;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;
    
    // Ray
    private Ray _downGroundRay;
    private Ray _upGroundRay;
    private Ray _rightOrLeftGroundRay;
    private bool _isGroundUp;
    private bool _isGroundRightOrLeftSide;
    private bool _isGroundDown;


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
    
    public BaseGameObject(MonoRenderer renderer, Input input, char[,] layer, SpritesLoaderSystem spritesLoaderSystem,
        Map map)
    {
        _input = input;
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;

        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));

        _downGroundRay = new Ray(
            _position + _xOffset / 2, 
            Vector2.Down, 
            _yOffset); // OK
        
        _upGroundRay = new Ray(_position, Vector2.Up, 1);
        _rightOrLeftGroundRay 
            = new Ray(
                new Vector2(_position.X + _xOffset / 2, _position.Y), 
                Vector2.Right, 
                _xOffset / 2); // OK

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
    }
    
    public void Update(double deltatime)
    {
        CheckInput(deltatime);
        
        UpdatePhysics(deltatime);
        UpdateCollider();
        HandleMovement(deltatime);
        CheckGroundAndWalls();
        Animation(deltatime);
        
        SwitchAnimation();
    }

    private void CheckInput(double deltatime)
    {
        if (_input.GetKey(ConsoleKey.D) || _input.GetKey(ConsoleKey.RightArrow))
            MoveRight();
        else if(_input.GetKey(ConsoleKey.A) || _input.GetKey(ConsoleKey.LeftArrow))
            MoveLeft();
        else
            StopHorizontal(deltatime);

        if (_input.GetKeyDown(ConsoleKey.Spacebar) && _isGroundDown)
            Jump();
        
    }

    private void SwitchAnimation()
    {
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
        else if(_isGroundDown)
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
        // если уперся в стену, не идём дальше
        if (_isGroundRightOrLeftSide && (_velocity.X >= 0 || _velocity.X <= 0))
        {
            _targetAnimation = _idleBase;
            return;
        }
        
        _position.X += _velocity.X;
        _collider.Position = _position;
    }

    private void CheckGroundAndWalls()
    {
        // Сначала синхронизируем колайдер с текущей позицией из физики
        UpdateCollider();

        // проверка через колайдер
        _isGroundRightOrLeftSide = false;

        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                double playerCenterX = _position.X + (_xOffset / 2f);
                double blockCenterX = block.Collider.Position.X + (block.Collider.Size.X / 2f);
                
                // Персонаж считается пересекающим стену, только если его ноги зашли в блок 
                // МЕНЬШЕ, чем на определенный порог (например, на 3-4 пикселя).
                // Если он провалился глубже — это уже приземление сверху, а не удар об стену!
                int skinWidthY = 4;
                bool isOverlappingVertically =
                    (_position.Y < block.Collider.Position.Y + block.Collider.Size.Y) &&
                    (_position.Y + _yOffset - skinWidthY > block.Collider.Position.Y);

                if (isOverlappingVertically)
                {
                    // Уперся в левую сторону стены (идешь вправо)
                    if (playerCenterX < blockCenterX && _velocity.X > 0)
                    {
                        _isGroundRightOrLeftSide = true;
                        _velocity.X = 0;
                        // Выталкиваем влево
                        _position.X = block.Collider.Position.X - _xOffset; 
                        UpdateCollider();
                        break;
                    }
                    // Уперся в правую сторону стены (идешь влево)
                    else if (playerCenterX > blockCenterX && _velocity.X < 0)
                    {
                        _isGroundRightOrLeftSide = true;
                        _velocity.X = 0;
                        // Выталкиваем вправо
                        _position.X = block.Collider.Position.X + block.Collider.Size.X; 
                        UpdateCollider();
                        break;
                    }
                }
            }
        }

        // проверка пола через луч
        _upGroundRay.UpdatePosition(_position);
        _downGroundRay.UpdatePosition(_position);
        _isGroundUp = _map.Blocks.Any(s => _upGroundRay.CheckDetection(s.Collider));

        var groundBlock = _map.Blocks.FirstOrDefault(s => _downGroundRay.CheckDetection(s.Collider));
        if (groundBlock != null)
        {
            _isGroundDown = true;
            _velocity.Y = 0;
            // Выталкиваем строго вверх
            _position.Y = groundBlock.Collider.Position.Y - _yOffset; 
        }
        else
        {
            _isGroundDown = false;
        }

        // Финальный снап колайдера под скорректированную позицию
        UpdateCollider();
    }

    private void DrawGizmos(int offset)
    {
        char isRightSymbol = _isGroundRightOrLeftSide ? '+' : '^';
        _renderer.DrawLine(_layer, (int)_rightOrLeftGroundRay.Position.X, 
            (int)_rightOrLeftGroundRay.Position.Y, 
            (int)_rightOrLeftGroundRay.Position.X + (offset / 2), 
            (int)_rightOrLeftGroundRay.Position.Y, isRightSymbol);

        char isGroundSymbol = _isGroundDown ? '+' : '-';
        _renderer.DrawLine(_layer, (int)_downGroundRay.Position.X + _xOffset / 2, 
            (int)_downGroundRay.Position.Y, 
            (int)_downGroundRay.Position.X + _xOffset / 2, 
            (int)_downGroundRay.Position.Y + _yOffset, isGroundSymbol);
    }
    

    private void Jump() => _velocity = new Vector2(_velocity.X, -80);
    
    private void UpdatePhysics(double deltatime)
    {
        
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
        
    }

    private void UpdateCollider()
    {
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
    
    private void StopHorizontal(double deltaTime) => _velocity = new Vector2(0, _velocity.Y);
    
}

