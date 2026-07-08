using System;

namespace MonochromeEngine.Engine;

public class AdvanceInput: IUpdatable
{
    // Битовые маски для кнопок (как константы на Ассемблере)
    public const byte MASK_JUMP  = 1;
    public const byte MASK_RIGHT = 2;
    public const byte MASK_LEFT  = 4;
    public const byte MASK_DOWN  = 8;
    public const byte MASK_UP    = 16;

    public byte MaskJump => MASK_JUMP;
    public byte MaskRight => MASK_RIGHT;
    public byte MaskLeft => MASK_LEFT;
    public byte MaskDown => MASK_DOWN;

    private byte _current;  // Состояние кнопок СЕЙЧАС
    private byte _previous; // Состояние кнопок в ПРОШЛОМ кадре

    public void Update(double deltaTime)
    {
        // 1. Старый кадр уходит в историю
        _previous = _current;
        _current = 0; // Сбрасываем текущий кадр перед опросом

        // 2. Читаем буфер консоли
        while (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;

            // Побитовое сложение (OR), собираем нажатые кнопки в один байт
            if (key == ConsoleKey.Spacebar)
            {
                _current |= MASK_JUMP;
                Console.Write("Jump");
            }
            if (key == ConsoleKey.D || key == ConsoleKey.RightArrow) _current |= MASK_RIGHT;
            if (key == ConsoleKey.A || key == ConsoleKey.LeftArrow)  _current |= MASK_LEFT;
            if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)  _current |= MASK_DOWN;
            if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)    _current |= MASK_UP;
        }
    }

    // --- НЕСОВСКАЯ ПОБИТОВАЯ МАГИЯ ДЛЯ ПЕРСОНАЖА ---

    // Удержание кнопки: проверяем, взведен ли нужный бит сейчас (через побитовое AND)
    public bool IsHeld(byte buttonMask) => (_current & buttonMask) != 0;

    // ТОТ САМЫЙ РЕЗКИЙ КЛИК ИЗ NES: Кнопка нажата сейчас AND НЕ была нажата ранее
    // Формула: Current AND (NOT Previous)
    public bool IsPressedNow(byte buttonMask) 
    {
        return (_current & buttonMask) != 0 && (_previous & buttonMask) == 0;
    }
}