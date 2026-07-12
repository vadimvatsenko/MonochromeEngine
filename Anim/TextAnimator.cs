using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Animation;

public class TextAnimator
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly MonoRenderer _renderer;
    private readonly char[,] _layer;
    private bool _isStatic = false;
    private readonly string _name;
    
    private const string WORDS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private Dictionary<char, List<char[,]>> _wordsDictionary = new Dictionary<char, List<char[,]>>();
    
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
    
    
    public TextAnimator(SpritesLoaderSystem spritesLoaderSystem, MonoRenderer renderer, string name, char[,] layer, bool isStatic = false)
    {
        _name = name;
        _renderer = renderer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _isStatic = isStatic;
        _layer = layer;

        foreach (var w in WORDS)
        {
            if (_spritesLoaderSystem.Sprites.TryGetValue(name, out Sprite sprites))
            {
                _wordsDictionary.Add(w, sprites.GetAnimationFrames($"Idle + {w}"));
            }
        }

        Console.WriteLine(_wordsDictionary.Count);
        Console.ReadKey();


        //_targetAnimation = A;
    }
    
    public void RunAnimation(double deltatime, bool isFacingRight, Vector2 position)
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
                int readX = !isFacingRight ? (width - 2 - x) : x;

                // Берем пару символов, составляющих один консольный пиксель
                char tileLeft = currentFrame[y, readX];
                char tileRight = currentFrame[y, readX + 1];

                // Отрисовка левой половинки пикселя
                if (tileLeft != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(position.X + x), (int)(y + position.Y), tileLeft);
                }

                // Отрисовка правой половинки пикселя
                if (tileRight != ' ')
                {
                    _renderer.DrawChar(_layer, (int)(x + 1 + position.X), (int)(y + position.Y), tileRight);
                }
            }
        }

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
}