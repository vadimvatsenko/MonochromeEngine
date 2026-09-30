using System.Text;

namespace MonochromeEngine.Engine.Render;

// Цветной режим: любой цвет RGB через escape-коды консоли.
// Работает в Windows Terminal и в обычной консоли Windows 10/11
public class ColorRenderer : ConsoleRendererBase
{
    private const int DefaultStyle = -2;
    // Отметка "это текст": цвет букв, а не заливка клетки
    private const int TextFlag = 1 << 24;

    // Чёрные пиксели тоже рисуются: контур героя закрывает траву за ним
    public override Cell PixelToCell(byte r, byte g, byte b, byte a) =>
        a >= 10 ? new Cell('█', (r << 16) | (g << 8) | b) : Cell.Empty;

    public override Cell BackgroundCell(int rgb) => new Cell('█', rgb);

    protected override void AppendCell(StringBuilder sb, Cell cell, ref int style)
    {
        // Закрашенная клетка — это пробел с цветом фона: 1 байт вместо 3 у '█'
        bool isBlock = cell.Symbol == '█';
        int wanted = cell.IsEmpty ? DefaultStyle : isBlock ? cell.Color : cell.Color | TextFlag;

        if (wanted != style)
        {
            style = wanted;
            if (wanted == DefaultStyle)
            {
                sb.Append("\u001b[49m");
            }
            else if (isBlock)
            {
                AppendRgb(sb.Append("\u001b[48;2;"), cell.Color);
            }
            else
            {
                AppendRgb(sb.Append("\u001b[49;38;2;"), cell.Color);
            }
        }

        sb.Append(isBlock || cell.IsEmpty ? ' ' : cell.Symbol);
    }

    protected override string ResetStyle => "\u001b[0m";

    private static void AppendRgb(StringBuilder sb, int rgb) =>
        sb.Append((rgb >> 16) & 0xFF).Append(';').Append((rgb >> 8) & 0xFF).Append(';').Append(rgb & 0xFF).Append('m');
}
