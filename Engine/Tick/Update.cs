using System.Diagnostics;
using MonochromeEngine.Scene;

namespace MonochromeEngine.Engine.Tick;

public class Update
{
    private readonly ISceneManager _sceneManager;
    private readonly int _targetFps;
    private readonly double _targetFrameMs;
    private double _nextFrameMs = 0;
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
        _fpsTimerStartMs = _sw.Elapsed.TotalMilliseconds;
    }

    public void RunUpdate()
    {
        while (true)
        {
            // Шаг игры фиксированный, как на NES и в Commander Keen: каждый кадр мир сдвигается
            // на одно и то же расстояние, поэтому прокрутка ровная. Если кадр не успел
            // нарисоваться вовремя, игра на миг замедляется, а не дёргается рывком
            double deltaTime = _targetFrameMs / 1000.0;

            _sceneManager.Update(deltaTime);
            _sceneManager.Draw();

            _nextFrameMs += _targetFrameMs;
            double nowMs = _sw.Elapsed.TotalMilliseconds;

            // Отстали (медленный кадр) — не догоняем пачкой быстрых кадров, а начинаем отсчёт заново
            if (_nextFrameMs < nowMs) _nextFrameMs = nowMs;

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
