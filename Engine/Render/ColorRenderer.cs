namespace MonochromeEngine.Engine.Render;

// Цветной режим: любой цвет RGB через escape-коды консоли.
// Работает в Windows Terminal и в обычной консоли Windows 10/11
public class ColorRenderer : ConsoleRendererBase
{
    private static readonly ScreenCell EmptyScreenCell = new ScreenCell(' ', AnyColor, DefaultColor);

    // Чёрные пиксели тоже рисуются: контур героя закрывает траву за ним
    public override Cell PixelToCell(byte r, byte g, byte b, byte a) =>
        a >= 10 ? new Cell('█', (r << 16) | (g << 8) | b) : Cell.Empty;

    public override Cell BackgroundCell(int rgb) => new Cell('█', rgb);

    protected override void ToScreen(Cell[,] frame, ScreenCell[,] screen)
    {
        int h = screen.GetLength(0);
        int w = screen.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Cell c = frame[y, x];
                if (c.IsEmpty) screen[y, x] = EmptyScreenCell;
                // Закрашенная клетка — пробел с цветом фона: 1 байт вместо 3 у '█'
                else if (c.Symbol == '█') screen[y, x] = new ScreenCell(' ', AnyColor, c.Color);
                else screen[y, x] = new ScreenCell(c.Symbol, c.Color, DefaultColor);
            }
        }
    }
}
