using MonochromeEngine.Engine;
using MonochromeEngine.Engine.BaseObjects;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Creatures;

public class Hero: GameObject, IUpdatable, IRenderable, ITarget
{
    private readonly Input _input;
    private HealthComponent _healthComponent;
    // Movement
    private readonly float _speed = 100f;
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
    
    private bool _isHit = false;
    private double _hitAnimTimer = 0;


    public Hero(string name, 
        Vector2 position, 
        char[,] layer, 
        Map map, 
        MonoRenderer renderer, 
        SpritesLoaderSystem spritesLoaderSystem, 
        Input input) : base(name, position, layer, map, renderer, spritesLoaderSystem)
    {
        _input = input;
        _healthComponent = new HealthComponent(100, 100);
    }

    public void Update(double deltatime)
    {
        // Управление таймерами для прыжка
        if (CollisionSystem.IsGrounded)
        {
            _coyoteJumpActivated = _coyoteJumpWindow;
        }
        else
        {
            _coyoteJumpActivated -= deltatime;
        }

        if (_bufferJumpActivated > 0)
        {
            _bufferJumpActivated -= (float)deltatime;
        }

        // Таймер Hit анимации
        if (_hitAnimTimer > 0)
        {
            _hitAnimTimer -= deltatime;
            if (_hitAnimTimer <= 0) _isHit = false;
        }

        CheckInput(deltatime);
        UpdatePhysics(deltatime);
        
        // Передаем размер коллайдера и deltatime для корректного выталкивания
        CollisionSystem.ResolveCollisions(ref Position, ref _velocity, new Vector2(XOffset, YOffset), deltatime);
        
        // Автоматический прыжок, если сработал буфер и мы приземлились
        if (_bufferJumpActivated > 0 && CollisionSystem.IsGrounded)
        {
            Jump();
            _bufferJumpActivated = -1;
        }
        
        Animator.UpdateAnimation(deltatime);

        SwitchAnimation();
    }

    public void Draw()
    {
        Animator.Draw(Position, IsFacingRight);
    }

    private void CheckInput(double deltatime)
    {
        if (_input.GetKey(ConsoleKey.D) || _input.GetKey(ConsoleKey.RightArrow))
            MoveRight();
        else if(_input.GetKey(ConsoleKey.A) || _input.GetKey(ConsoleKey.LeftArrow))
            MoveLeft();
        else
            StopHorizontal(deltatime);

        if (_input.GetKeyDown(ConsoleKey.Spacebar))
        {
            if (_coyoteJumpActivated > 0)
            {
                Jump();
                _coyoteJumpActivated = -1;
            }
            else
            {
                // Если не в воздухе (или не в окне койота), ставим в буфер
                _bufferJumpActivated = _bufferJumpWindow;
            }
        }
        
    }
    
    private void SwitchAnimation()
    {
        // Анимация Hit имеет приоритет
        if (_isHit)
        {
            Animator.TargetAnimation = Animator.HitBase;
        }
        else if (_velocity.Y > 0.5)
        {
            Animator.TargetAnimation = Animator.FallBase;
        }
        else if (_velocity.Y < -0.5)
        {
            Animator.TargetAnimation = Animator.JumpBase;
        }
        else if (_velocity.X < -0.5 || _velocity.X > 0.5)
        {
            Animator.TargetAnimation = Animator.MoveBase;
        }
        else if(CollisionSystem.IsGrounded)
        {
            Animator.TargetAnimation = Animator.IdleBase;
        }
    }
    
    private void Jump()
    {
        _velocity = new Vector2(_velocity.X, -120);
    }

    public void TakeDamage(float damage)
    {
        _healthComponent.TakeDamage(damage);
        _isHit = true;
        _hitAnimTimer = 0.5; // Анимация длится 0.5 секунды
        
        // Knockback: отталкиваем игрока назад (если он смотрит вправо, отлетает влево)
        float knockbackForce = 50f;
        _velocity = new Vector2(IsFacingRight ? -knockbackForce : knockbackForce, -50f);
    }
    
    private void UpdatePhysics(double deltatime)
    {
        // Теперь применяем только гравитацию к velocity
        _velocity = _velocity + (_gravity * 1.8f) * deltatime;
    }
    
    private void MoveRight()
    {
        _velocity = new Vector2(_speed, _velocity.Y);
        IsFacingRight = true;
    }

    private void MoveLeft()
    {
        _velocity = new Vector2(-_speed, _velocity.Y);
        IsFacingRight = false;
    }
    
    private void StopHorizontal(double deltaTime) => _velocity = new Vector2(0, _velocity.Y);
    
}
