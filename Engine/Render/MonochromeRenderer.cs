namespace MonochromeEngine.Engine.Render;

// Чёрно-белый режим: светлые пиксели — '█', тёмные и прозрачные — пусто
public class MonochromeRenderer : ConsoleRendererBase
{
    private const byte Threshold = 128;
    private static readonly Cell Filled = new Cell('█', 0xFFFFFF);

    public override Cell PixelToCell(byte r, byte g, byte b, byte a)
    {
        bool isActive = a >= 10 && (r > Threshold || g > Threshold || b > Threshold);
        return isActive ? Filled : Cell.Empty;
    }

    public override Cell BackgroundCell(int rgb) => Cell.Empty;

    public override void Render(Cell[,] frame)
    {
        int h = frame.GetLength(0);
        int w = frame.GetLength(1);

        Sb.Clear();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Cell c = frame[y, x];
                Sb.Append(c.IsEmpty ? ' ' : c.Symbol);
            }
            Sb.AppendLine();
        }

        WriteFrame();
    }
}
