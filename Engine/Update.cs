using System.Diagnostics;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Engine;

public class Update
{
    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private readonly MonoRenderer _renderer;
    private readonly List<char[,]> _layers = new List<char[,]>();
    private readonly Map _map;

    // Налаштування цільового FPS
    // Stopwatch — точніший таймер, ніж DateTime.Now
    private  readonly Stopwatch _sw;

    private readonly int _targetFps;

    // Час одного кадру в мілісекундах.
    // 1000 мс / 60 ≈ 16.666... мс на кадр
    private readonly double _targetFrameMs;

    // Час, коли "має" закінчитися наступний кадр (у мс від старту програми)
    private double _nextFrameMs = 0;

    // Зберігаємо час старту попереднього кадру (в мс)
    // deltaTime
    private double _lastFrameStartMs;

    // Лічильники для FPS
    private double _fps = 0;
    private int _frames = 0; // скільки кадрів пройшло за поточну секунду
    private double _fpsTimerStartMs; // старт відліку "секунди"

    public Update(int targetFps, MonoRenderer renderer, Map map, List<char[,]> layers)
    {
        _map = map;
        _renderer = renderer;
        _layers = layers;
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
            
            // === 1) ПОЧАТОК КАДРУ ==================================================
            double frameStartMs = _sw.Elapsed.TotalMilliseconds;

            // deltaTime — час між стартом цього кадру і стартом попереднього кадру
            // В секундах (бо в іграх dt зазвичай у секундах)
            double deltaTime = (frameStartMs - _lastFrameStartMs) / 1000.0;

            // Оновлюємо "останній час" для наступної ітерації
            _lastFrameStartMs = frameStartMs;
            
            ///////////////Тут зазвичай викликають ігрову логіку////////////////

            // Чистка всіх слоїв открім background
            
            for (int i = 1; i < _layers.Count; i++)
            {
                _renderer.Clear(_layers[i]);
            }
            
            _updatables.ForEach(updatable => updatable.Update(deltaTime));

            string fpsText = $"FPS: {_fps:F2} | deltaTime: {deltaTime:F4}s";
            _renderer.DrawString(_layers[_layers.Count - 1], _map.Width / 2 - fpsText.Length / 2, 2, fpsText);
            // рамка UI
            _renderer.DrawRect(_layers[_layers.Count - 1], 0, 0, _map.Width, 5, '░');
            _renderer.DrawRect(_layers[_layers.Count - 1], 1, 0, _map.Width - 2, 5, '░');
            // --
            var frame = _renderer.Compose(_map.Width, _map.Height, _layers.ToArray());
            _renderer.Render(frame);

            ////////////////////////////////

            // 2) ОБМЕЖЕННЯ FPS
            // Ми хочемо, щоб кожен кадр закінчувався не раніше, ніж через targetFrameMs
            // Тому плануємо "час завершення" поточного кадру:
            _nextFrameMs += _targetFrameMs;

            // Скільки часу ще залишилось до nextFrameMs
            double nowMs = _sw.Elapsed.TotalMilliseconds;
            double remainingMs = _nextFrameMs - nowMs;

            // Якщо залишилось більше ~1 мс — можна приспати потік (економить CPU)
            if (remainingMs > 1)
            {
                // Sleep приймає ціле число мілісекунд -> дробова частина губиться
                Thread.Sleep((int)remainingMs);
            }

            // Доробляємо "точність" коротким активним очікуванням (busy-wait)
            // Це дає рівніший таймінг, ніж один лише Sleep
            while (_sw.Elapsed.TotalMilliseconds < _nextFrameMs)
            {
                // нічого не робимо — просто чекаємо
            }

            // 3) ПІДРАХУНОК FPS (раз на ~1 секунду)
            _frames++;

            double passedMs = _sw.Elapsed.TotalMilliseconds - _fpsTimerStartMs;

            // Якщо пройшла (або майже) секунда — виводимо FPS
            if (passedMs >= 1000)
            {
                _fps = _frames / (passedMs / 1000.0);

                
                //Console.WriteLine($"FPS: {_fps:F2} | deltaTime: {deltaTime:F4}s");
                // Показуємо також останній deltaTime для наочності

                // Скидаємо лічильники на наступну секунду
                _frames = 0;
                _fpsTimerStartMs = _sw.Elapsed.TotalMilliseconds;
            }
        }
    }
}

    

        