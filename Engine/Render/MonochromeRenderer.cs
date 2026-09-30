namespace MonochromeEngine.Engine.Render;

// Чёрно-белый режим: светлые пиксели — '█', тёмные и прозрачные — пусто
public class MonochromeRenderer : ConsoleRendererBase
{
    private const byte Threshold = 128;
    private static readonly Cell Filled = new Cell('█', 0xFFFFFF);
    private static readonly ScreenCell EmptyScreenCell = new ScreenCell(' ', AnyColor, DefaultColor);

    public override Cell PixelToCell(byte r, byte g, byte b, byte a)
    {
        bool isActive = a >= 10 && (r > Threshold || g > Threshold || b > Threshold);
        return isActive ? Filled : Cell.Empty;
    }

    public override Cell BackgroundCell(int rgb) => Cell.Empty;

    protected override void ToScreen(Cell[,] frame, ScreenCell[,] screen)
    {
        int h = screen.GetLength(0);
        int w = screen.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Cell c = frame[y, x];
                screen[y, x] = c.IsEmpty ? EmptyScreenCell : new ScreenCell(c.Symbol, DefaultColor, DefaultColor);
            }
        }
    }
}
