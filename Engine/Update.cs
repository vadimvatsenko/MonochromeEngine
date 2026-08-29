using System.Diagnostics;
using MonochromeEngine.Scene;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Engine;

public class Update
{
    private readonly ISceneManager _sceneManager;
    private readonly int _targetFps;
    private readonly double _targetFrameMs;
    private double _nextFrameMs = 0;
    private double _lastFrameStartMs;
    private double _fps = 0;
    private int _frames = 0;
    private double _fpsTimerStartMs;
    private readonly Stopwatch _sw;

    public Update(int targetFps, ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _sw = Stopwatch.StartNew();
        _targetFps = targetFps;
        _targetFrameMs = 1000.0 / _targetFps;
        _lastFrameStartMs = _sw.Elapsed.TotalMilliseconds;
        _fpsTimerStartMs = _sw.Elapsed.TotalMilliseconds;
    }

    public void RunUpdate()
    {
        while (true)
        {
            double frameStartMs = _sw.Elapsed.TotalMilliseconds;
            double deltaTime = (frameStartMs - _lastFrameStartMs) / 1000.0;
            _lastFrameStartMs = frameStartMs;
            
            _sceneManager.Update(deltaTime);
            _sceneManager.Draw();

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
