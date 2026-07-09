using System.Runtime.InteropServices;
using MonochromeEngine.Engine;

public class Input : IUpdatable
{
    // Импортируем функцию Windows, которая проверяет состояние клавиши прямо сейчас
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static readonly HashSet<ConsoleKey> _currentKeys = new();
    private static readonly HashSet<ConsoleKey> _previousKeys = new();

    // Список клавиш, которые мы хотим отслеживать (чтобы не опрашивать лишнее)
    private static readonly ConsoleKey[] _trackedKeys = 
    {
        ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D, 
        ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow,
        ConsoleKey.Spacebar, ConsoleKey.Escape, ConsoleKey.Enter
    };

    /// <summary>
    /// Опрашивает состояние клавиш. Вызывать в САМОМ НАЧАЛЕ каждого кадра!
    /// </summary>
    public void Update(double deltaTime)
    {
        // Переносим текущее состояние в предыдущее
        _previousKeys.Clear();
        foreach (var key in _currentKeys)
        {
            _previousKeys.Add(key);
        }

        // Опрашиваем Windows состояние каждой нужной нам клавиши
        _currentKeys.Clear();
        foreach (var key in _trackedKeys)
        {
            // Если старший бит установлен (результат < 0), значит клавиша зажата
            if (GetAsyncKeyState((int)key) < 0)
            {
                _currentKeys.Add(key);
            }
        }
    }

    // --- API ДЛЯ ИГРЫ ---
    public bool GetKeyDown(ConsoleKey key) => _currentKeys.Contains(key) && !_previousKeys.Contains(key);
    public bool GetKey(ConsoleKey key) => _currentKeys.Contains(key);
    public bool GetKeyUp(ConsoleKey key) => !_currentKeys.Contains(key) && _previousKeys.Contains(key);
}