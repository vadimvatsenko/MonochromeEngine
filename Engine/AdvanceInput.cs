namespace MonochromeEngine.Engine;

public class AdvanceInput : IUpdatable
{
    // Порог времени (в секундах), после которого нажатие считается удержанием
    private const double HoldThreshold = 0.2; 

    // Перечисление для удобства работы с абстрактными действиями
    public enum GameAction { Left, Right, Up, Down, Jump, Attack, Cancel }

    // Внутренний класс для отслеживания состояния кнопки
    private class KeyState
    {
        public bool IsActionActive { get; set; } // Была ли нажата в этом кадре?
        public bool IsActionPrevious { get; set; } // Была ли нажата в прошлом кадре?
        public double DurationHeld { get; set; } // Сколько времени удерживается
        public bool HoldTriggered { get; set; } // Чтобы не спамить событием удержания
    }

    // Словарь состояний для каждого действия
    private readonly Dictionary<GameAction, KeyState> _states = new();

    // СОБЫТИЯ НАЖАТИЯ (Срабатывают один раз при клике)
    public event Action? OnLeftDown;
    public event Action? OnRightDown;
    public event Action? OnJumpDown;

    // СОБЫТИЯ УДЕРЖАНИЯ (Срабатывают, если зажать кнопку)
    public event Action? OnLeftHold;
    public event Action? OnRightHold;
    public event Action? OnJumpHold;

    public AdvanceInput()
    {
        // Инициализируем состояния для всех действий
        foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
        {
            _states[action] = new KeyState();
        }
    }
    public void Update(double deltatime)
    {
        // 1. Сбрасываем текущую активность перед чтением ввода
        foreach (var state in _states.Values)
        {
            state.IsActionPrevious = state.IsActionActive;
            state.IsActionActive = false;
        }

        // 2. Считываем ВСЕ нажатые клавиши в этом кадре
        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            MapKeyToAction(keyInfo.Key);
        }

        // 3. Анализируем состояния и вызываем нужные события
        ProcessActionStates(deltatime);
    }
    
    private void MapKeyToAction(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.LeftArrow or ConsoleKey.A:   _states[GameAction.Left].IsActionActive = true; break;
            case ConsoleKey.RightArrow or ConsoleKey.D:  _states[GameAction.Right].IsActionActive = true; break;
            case ConsoleKey.Spacebar:                    _states[GameAction.Jump].IsActionActive = true; break;
            // Добавь остальные кнопки по аналогии...
        }
    }

    private void ProcessActionStates(double deltaTime)
    {
        foreach (var kvp in _states)
        {
            GameAction action = kvp.Key;
            KeyState state = kvp.Value;

            if (state.IsActionActive)
            {
                // Кнопка нажата в этом кадре
                if (!state.IsActionPrevious)
                {
                    // Кнопка ТОЛЬКО ЧТО нажата (Клик)
                    TriggerDownEvent(action);
                    state.DurationHeld = 0;
                    state.HoldTriggered = false;
                }
                else
                {
                    // Кнопка удерживается со старого кадра
                    state.DurationHeld += deltaTime;
                    
                    if (state.DurationHeld >= HoldThreshold && !state.HoldTriggered)
                    {
                        TriggerHoldEvent(action);
                        state.HoldTriggered = true; // Защита от спама, сработает один раз за удержание
                    }
                }
            }
            else
            {
                // Кнопка НЕ нажата в этом кадре. 
                // Если она была нажата в прошлом — значит её отпустили.
                if (state.IsActionPrevious)
                {
                    state.DurationHeld = 0;
                    state.HoldTriggered = false;
                }
            }
        }
    }

    // Вспомогательные методы для вызова правильных ивентов
    private void TriggerDownEvent(GameAction action)
    {
        switch (action)
        {
            case GameAction.Left: OnLeftDown?.Invoke(); break;
            case GameAction.Right: OnRightDown?.Invoke(); break;
            case GameAction.Jump: OnJumpDown?.Invoke(); break;
        }
    }

    private void TriggerHoldEvent(GameAction action)
    {
        switch (action)
        {
            case GameAction.Left: OnLeftHold?.Invoke(); break;
            case GameAction.Right: OnRightHold?.Invoke(); break;
            case GameAction.Jump: OnJumpHold?.Invoke(); break;
        }
    }
}