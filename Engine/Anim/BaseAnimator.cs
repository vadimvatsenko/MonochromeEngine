using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.Anim;

public class BaseAnimator
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly IRenderer _renderer;
    private readonly Cell[,] _layer;
    private bool _isStatic = false;
    private readonly string _name;
    
    // Sprites
    public List<Cell[,]> IdleBase { get; private set; }
    public List<Cell[,]> MoveBase { get; private set; }
    public List<Cell[,]> AttackBase { get; private set; }
    public List<Cell[,]> DeathBase { get; private set; }
    public List<Cell[,]> JumpBase { get; private set; }
    public List<Cell[,]> FallBase { get; private set; }
    public List<Cell[,]> HitBase { get; private set; }
    public List<Cell[,]> BowBase {get; private set; }

    private List<Cell[,]> _targetAnimation;

    public List<Cell[,]> TargetAnimation
    {
        get { return _targetAnimation; }
        set { _targetAnimation = value; }
    }
    
    // animation settings
    private bool _loop = false;
    private bool _isFinished = false;
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.125;
    
    
    public BaseAnimator
        (SpritesLoaderSystem spritesLoaderSystem, 
            IRenderer renderer, 
            string name, 
            Cell[,] layer, 
            bool isStatic = false)
    {
        _name = name;
        _renderer = renderer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _isStatic = isStatic;
        _layer = layer;
        
        if (_spritesLoaderSystem.Sprites.TryGetValue(name, out Sprite sprites))
        {
            IdleBase = sprites.GetAnimationFrames("IdleBase");
            MoveBase = sprites.GetAnimationFrames("MoveBase");
            BowBase = sprites.GetAnimationFrames("BowBase");
            JumpBase = sprites.GetAnimationFrames("JumpBase");
            FallBase = sprites.GetAnimationFrames("FallBase");
            AttackBase = sprites.GetAnimationFrames("AttackBase");
            HitBase = sprites.GetAnimationFrames("HitBase");
            DeathBase = sprites.GetAnimationFrames("DeadBase");
        }

        _targetAnimation = MoveBase;
    }
    
    public void UpdateAnimation(double deltatime)
    {
        if (_targetAnimation == null || _targetAnimation.Count == 0) return;

        if (!_isStatic)
        {
            _animTimer += deltatime;

            if (_animTimer >= FRAME_DURATION)
            {
                _spriteIndex = (_spriteIndex + 1) % _targetAnimation.Count;

                _animTimer = 0;
            }
        }
    }

    public void SetFrameIndex(int index)
    {
        _spriteIndex = index;
    }

    public void SetIsStatic(bool isStatic)
    {
        _isStatic = isStatic;
    }

    public void Draw(Vector2 position, bool isFacingRight)
    {
        if (_targetAnimation == null || _targetAnimation.Count == 0) return;

        // Корректируем индекс, если он почему-то вылетел за границы
        if (_spriteIndex >= _targetAnimation.Count)
        {
            _spriteIndex = 0;
        }

        var currentFrame = _targetAnimation[_spriteIndex];
        int height = currentFrame.GetLength(0);
        int width = currentFrame.GetLength(1);

        // Позиция на экране = позиция в мире минус камера.
        // X округляем вниз до чётного: пиксель спрайта — 2 символа, а камера ходит целыми пикселями.
        // Иначе объект на полпикселя "дрожит" туда-обратно относительно фона
        int worldX = (int)Math.Floor(position.X);
        worldX -= worldX & 1;
        int originX = worldX - Camera.X;
        int originY = (int)position.Y - Camera.Y;

        // Объект целиком за экраном — не рисуем: на длинном уровне таких тысячи
        if (originX + width <= 0 || originY + height <= 0 ||
            originX >= _layer.GetLength(1) || originY >= _layer.GetLength(0))
        {
            return;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                int readX = !isFacingRight ? (width - 2 - x) : x;

                Cell tileLeft = currentFrame[y, readX];
                Cell tileRight = currentFrame[y, readX + 1];

                if (!tileLeft.IsEmpty)
                {
                    _renderer.DrawCell(_layer, originX + x, originY + y, tileLeft);
                }

                if (!tileRight.IsEmpty)
                {
                    _renderer.DrawCell(_layer, originX + x + 1, originY + y, tileRight);
                }
            }
        }
    }
}