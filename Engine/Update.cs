using System.Diagnostics;
using MonochromeEngine.Scene;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Engine;

public class Update
{
    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private readonly MonoRenderer _renderer;
    private readonly List<char[,]> _layers = new List<char[,]>();
    private readonly Map _map;
    private readonly Input _input;

    private  readonly Stopwatch _sw;
    private readonly int _targetFps;
    private readonly double _targetFrameMs;
    private double _nextFrameMs = 0;
    private double _lastFrameStartMs;
    private double _fps = 0;
    private int _frames = 0;
    private double _fpsTimerStartMs;

    public Update(int targetFps, MonoRenderer renderer, Map map, Input input, List<char[,]> layers)
    {
        _map = map;
        _renderer = renderer;
        _layers = layers;
        _input = input;
        _sw = Stopwatch.StartNew();
        _targetFps = targetFps;
        _targetFrameMs = 1000.0 / _targetFps;
        _lastFrameStartMs = _sw.Elapsed.TotalMilliseconds;
        _fpsTimerStartMs = _sw.Elapsed.TotalMilliseconds;
    }

    public void AddUpdatable(IUpdatable updatable)
    {
        if (!_updatables.Contains(updatable))
        {
            _updatables.Add(updatable);
        }
    }

    public void RemoveUpdatable(IUpdatable updatable)
    {
        if (_updatables.Contains(updatable))
        {
            _updatables.Remove(updatable);
        }
    }

    public void RunUpdate()
    {
        while (true)
        {
            double frameStartMs = _sw.Elapsed.TotalMilliseconds;
            double deltaTime = (frameStartMs - _lastFrameStartMs) / 1000.0;
            _lastFrameStartMs = frameStartMs;
            
            for (int i = 1; i < _layers.Count; i++)
            {
                _renderer.Clear(_layers[i]);
            }
            
            _updatables.ForEach(updatable => updatable.Update(deltaTime));

            string fpsText = $"FPS: {_fps:F2} | deltaTime: {deltaTime:F4}s";
            _renderer.DrawString(_layers[_layers.Count - 1], _map.Width / 2 - fpsText.Length / 2, 2, fpsText);
            _renderer.DrawRect(_layers[_layers.Count - 1], 0, 0, _map.Width, 5, '░');
            _renderer.DrawRect(_layers[_layers.Count - 1], 1, 0, _map.Width - 2, 5, '░');
            
            var frame = _renderer.Compose(_map.Width, _map.Height, _layers.ToArray());
            _renderer.Render(frame);

            _nextFrameMs += _targetFrameMs;
            double nowMs = _sw.Elapsed.TotalMilliseconds;
            double remainingMs = _nextFrameMs - nowMs;

            if (remainingMs > 1)
            {
                Thread.Sleep((int)remainingMs);
            }
            while (_sw.Elapsed.TotalMilliseconds < _nextFrameMs) { }

            _frames++;
            double passedMs = _sw.Elapsed.TotalMilliseconds - _fpsTimerStartMs;
            if (passedMs >= 1000)
            {
                _fps = _frames / (passedMs / 1000.0);
                _frames = 0;
                _fpsTimerStartMs = _sw.Elapsed.TotalMilliseconds;
            }
        }
    }
}
