using MonochromeEngine.Engine;
using MonochromeEngine.Engine.BaseObjects;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Creatures;

public class Bat : GameObject, IUpdatable, IRenderable
{
    // Patrol settings
    private readonly float _speed = 50f;
    private readonly float _patrolRange = 150f;
    private readonly double _startX;
    private readonly double _startY;
    private int _direction = 1; // 1 for right, -1 for left

    // Flight wave
    private double _timeElapsed = 0;
    private readonly float _arcAmplitude = 10f;

    // Peep sound
    private double _peepTimer = 0;
    private readonly double _peepInterval = 2.0; 


    public Bat(
        string name, 
        Vector2 position, 
        Cell[,] layer, 
        Map map, 
        IRenderer renderer, 
        SpritesLoaderSystem spritesLoaderSystem) : base(name, position, layer, map, renderer, spritesLoaderSystem)
    {
        Animator.TargetAnimation = Animator.IdleBase;
        _startX = Position.X;
        _startY = Position.Y;
    }

    public void Update(double deltatime)
    {
        Patrol(deltatime);
        Animator.UpdateAnimation(deltatime);

        _peepTimer += deltatime;
        if (_peepTimer >= _peepInterval)
        {
            _peepTimer = 0;
            // Используем новую полифоническую систему или async-звук
            // Здесь просто для примера, как мы делали ранее:
            /*byte[] soundData = MonochromeEngine.Engine.Audio.SpectrumBeeper.GenerateSquareWave(880, 50);
            MonochromeEngine.Engine.Audio.WinMMAudio.PlayRawPcm(soundData);*/
        }
    }

    private void Patrol(double deltatime)
    {
        // 1. Двигаемся линейно по X
        Position.X += _speed * _direction * (float)deltatime;

        // 2. Добавляем "волнистость" по Y
        _timeElapsed += deltatime * 5.0; // Скорость волны
        float yOffset = (float)Math.Sin(_timeElapsed) * _arcAmplitude;

        Position.Y = _startY + yOffset; 

        // Границы X
        if (Position.X > _startX + _patrolRange)
        {
            _direction = -1;
            IsFacingRight = false;
        }
        else if (Position.X < _startX - _patrolRange)
        {
            _direction = 1;
            IsFacingRight = true;
        }
    }

    public void Draw() => Animator.Draw(Position, IsFacingRight);
}
