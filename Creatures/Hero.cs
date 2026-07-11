using System.Diagnostics;
using MonochromeEngine.Animation;
using MonochromeEngine.Collisions;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Creatures;

public class Hero: IUpdatable
{
    private readonly Input _input;
    private readonly BoxCollider2D _collider;
    private readonly BaseAnimator _animator;
    private readonly BaseCollisionSystem _collisionSystem;
    // Movement
    private Vector2 _position = new Vector2(32, 100);
    private int _stepX = 2;
    private readonly float _speed = 6f;
    private double _multiplier = 0;
    private bool _isFacingRight = true;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;
    // jump logic
    private Vector2 _velocity = new Vector2(0, 0);
    private Vector2 _gravity = new Vector2(0, 100f);
    // CoyotyJump
    private double _coyoteJumpWindow = 0.5f;
    private double _coyoteJumpActivated = -1;
    private double _timeAccumulation = 0;
    // Buffer Jump
    private float _bufferJumpWindow = 0.25f; // Окно буфера (сколько секунд допустимо)
    private float _bufferJumpActivated = -1;
    private bool _isAirborne = false;
    
    
    public Hero(MonoRenderer renderer, Input input, char[,] layer, SpritesLoaderSystem spritesLoaderSystem,
        Map map)
    {
        _input = input;
        
        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));
        _animator = new BaseAnimator(spritesLoaderSystem, renderer, "Hero", layer);
        _collisionSystem = new BaseCollisionSystem(map, _collider, renderer, layer, _position, _xOffset, _yOffset);
    }
    
    public void Update(double deltatime)
    {
        CheckInput(deltatime);
        UpdatePhysics(deltatime);
        UpdateCollider();
        HandleMovement(deltatime);
        _collisionSystem.CheckGroundAndWalls(ref _position, ref _velocity);
        _animator.RunAnimation(deltatime, _isFacingRight, _position);

        //UpdateAirBornStatus();
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

        /*if (_input.GetKeyDown(ConsoleKey.Spacebar) && _collisionSystem.IsGroundDown)
            Jump();*/
        
        if (_input.GetKeyDown(ConsoleKey.Spacebar) && _collisionSystem.IsGroundDown)
        {
            Jump();
        }
    }
    
    private void SwitchAnimation()
    {
        if (_velocity.Y > 0.5)
        {
            _animator.TargetAnimation = _animator.FallBase;
        }
        else if (_velocity.Y < -0.5)
        {
            _animator.TargetAnimation = _animator.JumpBase;
        }
        else if (_velocity.X < -0.5 || _velocity.X > 0.5)
        {
            _animator.TargetAnimation = _animator.MoveBase;
        }
        else if(_collisionSystem.IsGroundDown)
        {
            _animator.TargetAnimation = _animator.IdleBase;
        }
    }
    
    private void HandleMovement(double deltatime)
    {
        // если уперся в стену, не идём дальше
        if (_collisionSystem.IsGroundRightOrLeftSide && (_velocity.X >= 0 || _velocity.X <= 0))
        {
            _animator.TargetAnimation = _animator.IdleBase;
            return;
        }
        
        _position.X += _velocity.X;
        _collider.Position = _position;
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

    private void UpdateCollider() => _collider.Position = new Vector2(_position.X, _position.Y);
    
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