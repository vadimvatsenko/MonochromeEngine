using System.Runtime.InteropServices;

namespace MonochromeEngine.Engine.Render;

// Цветной режим: любой цвет RGB через escape-коды консоли (ESC[38;2;R;G;Bm).
// Работает в Windows Terminal и в обычной консоли Windows 10/11
public class ColorRenderer : ConsoleRendererBase
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public ColorRenderer()
    {
        // Старая консоль понимает цветовые коды только после включения этого режима
        IntPtr handle = GetStdHandle(StdOutputHandle);
        if (GetConsoleMode(handle, out uint mode))
        {
            SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
        }
    }

    // Чёрные пиксели тоже рисуются: контур героя закрывает траву за ним
    public override Cell PixelToCell(byte r, byte g, byte b, byte a) =>
        a >= 10 ? new Cell('█', (r << 16) | (g << 8) | b) : Cell.Empty;

    public override Cell BackgroundCell(int rgb) => new Cell('█', rgb);

    public override void Render(Cell[,] frame)
    {
        int h = frame.GetLength(0);
        int w = frame.GetLength(1);

        Sb.Clear();
        for (int y = 0; y < h; y++)
        {
            // Код цвета пишем только когда цвет меняется — иначе строка вырастет в разы
            int currentColor = -1;
            for (int x = 0; x < w; x++)
            {
                Cell c = frame[y, x];
                if (c.IsEmpty)
                {
                    Sb.Append(' ');
                    continue;
                }

                if (c.Color != currentColor)
                {
                    currentColor = c.Color;
                    Sb.Append("\u001b[38;2;")
                        .Append((c.Color >> 16) & 0xFF).Append(';')
                        .Append((c.Color >> 8) & 0xFF).Append(';')
                        .Append(c.Color & 0xFF).Append('m');
                }
                Sb.Append(c.Symbol);
            }
            // Сброс цвета в конце строки, чтобы не красить то, что консоль допишет после кадра
            Sb.Append("\u001b[0m").AppendLine();
        }

        WriteFrame();
    }
}
