using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Coin : IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;

    private List<char[,]> _idleBase;
    private List<char[,]> _despawnBase;
    private List<char[,]> _targetAnimation;

    private bool _loop = false;
    private bool _isFinished = false;
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.16; 
    
    public Coin(MonoRenderer renderer, char[,] layer, SpritesLoaderSystem spritesLoaderSystem)
    {
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;

        if (spritesLoaderSystem.Sprites.TryGetValue("Coin", out Sprite coinSprite))
        {
            _idleBase = coinSprite.GetAnimationFrames("IdleBase");
            _despawnBase = coinSprite.GetAnimationFrames("DespawnBase");
        }
        
        _targetAnimation = _despawnBase;
    }

    public void Update(double deltatime)
    {
        
        _renderer.Clear(_layer);
        
        for (int y = 0; y < _targetAnimation[_spriteIndex].GetLength(0); y++)
        {
            for (int x = 0; x < _targetAnimation[_spriteIndex].GetLength(1); x++)
            {
                char tile = _targetAnimation[_spriteIndex][y, x];
                
                if (tile == ' ') continue;
                
                //int startX = x * 2;
                for (int i = 0; i < 2; i++)
                {
                    _renderer.DrawChar(_layer, x + 10, y + 10, tile);
                }
            }
        }

        _animTimer += deltatime;

        if (_animTimer >= FRAME_DURATION)
        {
            _spriteIndex = (_spriteIndex + 1) % _targetAnimation.Count;
            _animTimer = 0;
        }

        if (_spriteIndex >= _targetAnimation.Count - 1)
        {
            // finish animation
        }
    }
}
