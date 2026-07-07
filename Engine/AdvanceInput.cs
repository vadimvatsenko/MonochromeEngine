using System;

namespace MonochromeEngine.Engine;

public class AdvanceInput: IUpdatable
{
    // Храним состояние кнопок для ТЕКУЩЕГО кадра
    public bool IsLeftActive { get; private set; }
    public bool IsRightActive { get; private set; }
    public bool IsJumpActive { get; private set; }

    // Храним состояние кнопок для ПРОШЛОГО кадра
    private bool _isLeftPrevious;
    private bool _isRightPrevious;
    private bool _isJumpPrevious;

    public void Update(double deltaTime)
    {
        // 1. Запоминаем, что было в прошлом кадре
        _isLeftPrevious = IsLeftActive;
        _isRightPrevious = IsRightActive;
        _isJumpPrevious = IsJumpActive;

        // 2. Сбрасываем текущий кадр перед чтением
        IsLeftActive = false;
        IsRightActive = false;
        IsJumpActive = false;

        // 3. Читаем все нажатые клавиши из консоли
        while (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.A || key == ConsoleKey.LeftArrow)   IsLeftActive = true;
            if (key == ConsoleKey.D || key == ConsoleKey.RightArrow)  IsRightActive = true;
            if (key == ConsoleKey.Spacebar)                           IsJumpActive = true;
        }
    }

    // --- УДОБНЫЕ СВОЙСТВА ДЛЯ ПЕРСОНАЖА ---

    // Клик по прыжку: СЕЙЧАС нажата, а РАНЬШЕ не была (сработает строго 1 раз при нажатии!)
    public bool IsJumpPressedNow => IsJumpActive && !_isJumpPrevious;

    // Удержание кнопок: просто проверяем, активна ли она прямо сейчас
    public bool IsLeftHeld => IsLeftActive;
    public bool IsRightHeld => IsRightActive;
}