using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonochromeEngine.Engine.Render;

// Общая часть рендеров: работа со слоями и вывод кадра одинаковы. Режимы отличаются только
// превращением пикселя в клетку и тем, как клетки кадра становятся клетками экрана
public abstract class ConsoleRendererBase : IRenderer
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    // Консоль не показывает кадр, пока он не дописан целиком (где поддерживается) — картинка не "плывёт"
    private const string BeginSyncUpdate = "\u001b[?2026h";
    private const string EndSyncUpdate = "\u001b[?2026l";

    // Цвета клетки экрана: обычный цвет консоли / не важно (у пробела не видно цвета букв)
    protected const int DefaultColor = -1;
    protected const int AnyColor = -2;
    private const int UnknownColor = int.MinValue;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private readonly StringBuilder _sb = new StringBuilder();
    private readonly Stream _stdout = Console.OpenStandardOutput();
    private byte[] _bytes = Array.Empty<byte>();

    private Cell[,]? _composed;

    // Кадр, переведённый в клетки экрана, и то, что сейчас на экране — выводим только отличия
    private ScreenCell[,]? _next;
    private ScreenCell[,]? _onScreen;
    private int _windowWidth;
    private int _windowHeight;

    // Клетка консоли, как она выводится: символ, цвет символа и цвет фона
    protected readonly struct ScreenCell
    {
        public readonly char Symbol;
        public readonly int Fg;
        public readonly int Bg;

        public ScreenCell(char symbol, int fg, int bg)
        {
            Symbol = symbol;
            Fg = fg;
            Bg = bg;
        }

        public bool SameAs(ScreenCell other) => Symbol == other.Symbol && Fg == other.Fg && Bg == other.Bg;
    }

    protected ConsoleRendererBase()
    {
        // Коды перемещения курсора и цвета старая консоль понимает только после включения этого режима
        IntPtr handle = GetStdHandle(StdOutputHandle);
        if (GetConsoleMode(handle, out uint mode))
        {
            SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
        }

        // Пишем байты UTF-8 напрямую — консоль должна их так и читать (иначе '█' станет мусором)
        Console.OutputEncoding = Encoding.UTF8;
    }

    public abstract Cell PixelToCell(byte r, byte g, byte b, byte a);
    public abstract Cell BackgroundCell(int rgb);

    // Сколько клеток экрана нужно под кадр такого размера (по умолчанию одна клетка кадра = одна клетка экрана)
    protected virtual (int Width, int Height) ScreenSize(int frameWidth, int frameHeight) => (frameWidth, frameHeight);

    // Перевести весь кадр в клетки экрана (один вызов на кадр — быстрее, чем по вызову на клетку)
    protected abstract void ToScreen(Cell[,] frame, ScreenCell[,] screen);

    // стрворить масив слоя
    public Cell[,] CreateLayer(int w, int h)
    {
        return new Cell[h, w]; // [y,x]
    }

    // Слой [y,x] лежит в памяти одной строкой — так его можно обходить без двойных индексов
    private static Span<Cell> AsSpan(Cell[,] layer) =>
        MemoryMarshal.CreateSpan(ref Unsafe.As<byte, Cell>(ref MemoryMarshal.GetArrayDataReference(layer)), layer.Length);

    // використовується для background, заповнює одною клітинкою повністю
    public void Fill(Cell[,] layer, Cell cell) => AsSpan(layer).Fill(cell);

    // очистка слоя вікликаєтся кожен кадр в Update
    public void Clear(Cell[,] layer) => Fill(layer, Cell.Empty);

    // Малює клітинку, використовується для відмальовування об'єктів
    public void DrawCell(Cell[,] layer, int x, int y, Cell cell)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        layer[y, x] = cell;
    }

    public void DrawString(Cell[,] layer, int x, int y, string s, int color)
    {
        for (int i = 0; i < s.Length; i++)
            DrawCell(layer, x + i, y, new Cell(s[i], color));
    }

    public void DrawLine(Cell[,] layer, int x0, int y0, int x1, int y1, Cell cell)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCell(layer, x0, y0, cell);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public void DrawRect(Cell[,] layer, int x, int y, int w, int h, Cell cell)
    {
        for (int i = 0; i < w; i++)
        {
            DrawCell(layer, x + i, y, cell);
            DrawCell(layer, x + i, y + h - 1, cell);
        }

        for (int j = 0; j < h; j++)
        {
            DrawCell(layer, x, y + j, cell);
            DrawCell(layer, x + w - 1, y + j, cell);
        }
    }

    // Композитний кадр. Буфер кадра один на все кадры — не выделяем по мегабайту 60 раз в секунду.
    // Render переводит клетки к себе, так что следующий Compose его спокойно перезаписывает
    public Cell[,] Compose(int w, int h, params Cell[][,] layers)
    {
        if (_composed == null || _composed.GetLength(0) != h || _composed.GetLength(1) != w)
        {
            _composed = new Cell[h, w];
        }

        Span<Cell> result = AsSpan(_composed);
        result.Fill(Cell.Empty); // якщо взагалі нічого

        // Слои по порядку: непрозрачная клетка следующего слоя закрывает предыдущие
        foreach (var layer in layers)
        {
            Span<Cell> src = AsSpan(layer);
            int count = Math.Min(src.Length, result.Length);
            for (int i = 0; i < count; i++)
            {
                if (!src[i].IsEmpty) result[i] = src[i];
            }
        }

        return _composed;
    }

    // Выводим только клетки, изменившиеся с прошлого кадра: консоль — самое медленное место,
    // а небо и неподвижные блоки от кадра к кадру не меняются
    public void Render(Cell[,] frame)
    {
        (int w, int h) = ScreenSize(frame.GetLength(1), frame.GetLength(0));

        // Первый кадр или окно консоли поменяло размер — перерисовываем всё
        bool full = _onScreen == null || _onScreen.GetLength(0) != h || _onScreen.GetLength(1) != w ||
                    Console.WindowWidth != _windowWidth || Console.WindowHeight != _windowHeight;
        if (full)
        {
            _onScreen = new ScreenCell[h, w];
            _next = new ScreenCell[h, w];
            _windowWidth = Console.WindowWidth;
            _windowHeight = Console.WindowHeight;
        }
        ScreenCell[,] onScreen = _onScreen!;
        ScreenCell[,] next = _next!;

        ToScreen(frame, next);

        _sb.Clear();
        _sb.Append(BeginSyncUpdate);

        // Цвета консоли неизвестны — первые же будут записаны
        int fg = UnknownColor;
        int bg = UnknownColor;

        for (int y = 0; y < h; y++)
        {
            int x = 0;
            while (x < w)
            {
                if (!full && next[y, x].SameAs(onScreen[y, x]))
                {
                    x++;
                    continue;
                }

                // Начало изменившегося куска — ставим курсор (строки и столбцы в консоли с 1)
                _sb.Append("\u001b[").Append(y + 1).Append(';').Append(x + 1).Append('H');

                while (x < w && (full || !next[y, x].SameAs(onScreen[y, x]) || ChangedSoon(next, onScreen, y, x, w)))
                {
                    AppendCell(next[y, x], ref fg, ref bg);
                    onScreen[y, x] = next[y, x];
                    x++;
                }
            }
        }

        _sb.Append("\u001b[0m").Append(EndSyncUpdate);
        WriteToConsole();
    }

    // Код цвета пишем, только когда цвет меняется — иначе строка вырастет в разы
    private void AppendCell(ScreenCell cell, ref int fg, ref int bg)
    {
        if (cell.Fg != AnyColor && cell.Fg != fg)
        {
            fg = cell.Fg;
            if (fg == DefaultColor) _sb.Append("\u001b[39m");
            else AppendRgb("\u001b[38;2;", fg);
        }

        if (cell.Bg != bg)
        {
            bg = cell.Bg;
            if (bg == DefaultColor) _sb.Append("\u001b[49m");
            else AppendRgb("\u001b[48;2;", bg);
        }

        _sb.Append(cell.Symbol);
    }

    private void AppendRgb(string prefix, int rgb) =>
        _sb.Append(prefix).Append((rgb >> 16) & 0xFF).Append(';').Append((rgb >> 8) & 0xFF).Append(';')
            .Append(rgb & 0xFF).Append('m');

    // Короткий неизменившийся промежуток дешевле переписать, чем прыгать курсором через него
    private static bool ChangedSoon(ScreenCell[,] next, ScreenCell[,] onScreen, int y, int x, int w)
    {
        const int MaxGap = 6;
        for (int i = x; i < Math.Min(w, x + MaxGap); i++)
        {
            if (!next[y, i].SameAs(onScreen[y, i])) return true;
        }
        return false;
    }

    // Весь кадр — одной записью байтов в консоль, без посимвольного вывода
    private void WriteToConsole()
    {
        string text = _sb.ToString();
        int count = Encoding.UTF8.GetByteCount(text);
        if (_bytes.Length < count) _bytes = new byte[count * 2];
        Encoding.UTF8.GetBytes(text, 0, text.Length, _bytes, 0);
        _stdout.Write(_bytes, 0, count);
        _stdout.Flush();
    }
}
