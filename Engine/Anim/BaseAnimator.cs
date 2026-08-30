using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.Anim;

public class BaseAnimator
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly MonoRenderer _renderer;
    private readonly char[,] _layer;
    private bool _isStatic = false;
    private readonly string _name;
    
    // Sprites
    public List<char[,]> IdleBase { get; private set; }
    public List<char[,]> MoveBase { get; private set; }
    public List<char[,]> AttackBase { get; private set; }
    public List<char[,]> DeathBase { get; private set; }
    public List<char[,]> JumpBase { get; private set; }
    public List<char[,]> FallBase { get; private set; }
    public List<char[,]> HitBase { get; private set; }
    public List<char[,]> BowBase {get; private set; }

    private List<char[,]> _targetAnimation;

    public List<char[,]> TargetAnimation
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
            MonoRenderer renderer, 
            string name, 
            char[,] layer, 
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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                int readX = !isFacingRight ? (width - 2 - x) : x;

                char tileLeft = currentFrame[y, readX];
                char tileRight = currentFrame[y, readX + 1];

                if (tileLeft != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(position.X + x), (int)(y + position.Y), tileLeft);
                }

                if (tileRight != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(x + 1 + position.X), (int)(y + position.Y), tileRight);
                }
            }
        }
    }
}