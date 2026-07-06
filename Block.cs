using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Block: IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    public BoxCollider2D Collider { get; private set; }

    // Sprites
    private List<char[,]> _idleBase;
    private List<char[,]> _moveBase;
    private List<char[,]> _targetAnimation;

    // Movement
    private Vector2 _position = new Vector2(20, 20);
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
    
    public Block(MonoRenderer renderer, char[,] layer, SpritesLoaderSystem spritesLoaderSystem, Map map, Vector2 position)
    {
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        _position = position;
        
        Collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));

        if (_spritesLoaderSystem.Sprites.TryGetValue("Brick", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _moveBase = sprites.GetAnimationFrames("WalkRight");
            
        }

        _targetAnimation = _idleBase;
        
    }

    public void Update(double deltatime)
    {
        Animation(deltatime);
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
}