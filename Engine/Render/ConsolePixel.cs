namespace MonochromeEngine.Engine.Render;

public struct ConsolePixel
{
    public char Symbol;
    public ConsoleColor Color;

    public ConsolePixel(char symbol, ConsoleColor color)
    {
        Symbol = symbol;
        Color = color;
    }
}
