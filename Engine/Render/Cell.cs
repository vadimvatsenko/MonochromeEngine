namespace MonochromeEngine.Engine.Render;

// Одна клетка консоли: символ и его цвет (0xRRGGBB).
// Пробел (и '\0' у нового массива) — прозрачная клетка, сквозь неё видно слой ниже
public readonly struct Cell
{
    public static readonly Cell Empty = new Cell(' ', 0);

    public readonly char Symbol;
    public readonly int Color;

    public Cell(char symbol, int color)
    {
        Symbol = symbol;
        Color = color;
    }

    public bool IsEmpty => Symbol == ' ' || Symbol == '\0';
}
