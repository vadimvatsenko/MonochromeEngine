using System.Text;

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

    protected override void AppendCell(StringBuilder sb, Cell cell, ref int style) =>
        sb.Append(cell.IsEmpty ? ' ' : cell.Symbol);

    protected override string ResetStyle => "";
}
