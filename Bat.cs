using MonochromeEngine.Animation;
using MonochromeEngine.Collisions;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Bat : IUpdatable
{
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    private BoxCollider2D _collider;
    
    // Sprites
    private List<char[,]> _idleBase;
    private List<char[,]> _deathBase;
    private List<char[,]> _hitBase;
    private List<char[,]> _targetAnimation;

    // Movement & Arc
    private Vector2 _startPosition;
    private Vector2 _position;
    private bool _isFacingRight = false;
    private readonly int _xOffset = 32;
    private readonly int _yOffset = 16;
    
    // Arc patrol settings
    private double _timeElapsed = 0;
    private readonly float _patrolSpeed = 2f;    // Speed along X
    private readonly float _arcAmplitude = 40f;  // Height of the arc
    private readonly float _patrolRange = 200f;  // Range of the arc

    // animation settings
    private int _spriteIndex = 0;
    private double _animTimer = 0;
    private const double FRAME_DURATION = 0.125;

    public Bat(MonoRenderer renderer, char[,] layer, SpritesLoaderSystem spritesLoaderSystem, Map map, Vector2 startPosition)
    {
        _renderer = renderer;
        _layer = layer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        _startPosition = startPosition;
        _position = startPosition;

        _collider = new BoxCollider2D(_position, new Vector2(_xOffset, _yOffset));

        if (_spritesLoaderSystem.Sprites.TryGetValue("Bat", out Sprite sprites))
        {
            _idleBase = sprites.GetAnimationFrames("IdleBase");
            _hitBase = sprites.GetAnimationFrames("HitBase");
            _deathBase = sprites.GetAnimationFrames("DeadBase");
        }

        _targetAnimation = _idleBase;
    }

    public void Update(double deltatime)
    {
        PatrolArc(deltatime);
        Animation(deltatime);
    }
    
    private void PatrolArc(double deltatime)
    {
        _timeElapsed += deltatime * _patrolSpeed;
        
        // Horizontal movement (back and forth)
        float xOffset = (float)Math.Sin(_timeElapsed) * _patrolRange;
        // Vertical movement (arc)
        float yOffset = (float)Math.Sin(_timeElapsed * 2) * _arcAmplitude;
        
        _position.X = _startPosition.X + xOffset;
        _position.Y = _startPosition.Y + yOffset;
        
        _isFacingRight = xOffset > 0;
        _collider.Position = _position;
    }

    private void Animation(double deltatime)
    {
        if (_targetAnimation == null || _targetAnimation.Count == 0) return;

        if (_spriteIndex >= _targetAnimation.Count) _spriteIndex = 0;

        var currentFrame = _targetAnimation[_spriteIndex];
        int height = currentFrame.GetLength(0);
        int width = currentFrame.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                int readX = !_isFacingRight ? (width - 2 - x) : x;

                char tileLeft = currentFrame[y, readX];
                char tileRight = currentFrame[y, readX + 1];

                if (tileLeft != ' ') _renderer.DrawChar(_layer, (int)(_position.X + x), (int)(y + _position.Y), tileLeft);
                if (tileRight != ' ') _renderer.DrawChar(_layer, (int)(x + 1 + _position.X), (int)(y + _position.Y), tileRight);
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
