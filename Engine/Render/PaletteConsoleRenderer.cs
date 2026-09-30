using System.Runtime.InteropServices;

namespace MonochromeEngine.Engine.Render;

// Цвет "как в DOS": кадр кладётся прямо в буфер консоли одним вызовом WriteConsoleOutput —
// без текста и цветовых кодов, как игры DOS писали прямо в видеопамять. Цветов 16, но какие
// именно — выбираем сами, как палитру EGA в Commander Keen: берём 16 самых частых цветов
// спрайтов и тайлов (у NES-уровня их обычно меньше 16, тогда цвета точные).
// Пиксели полублоками, как в HalfBlockColorRenderer: окно консоли под кадр 576x208 — 288x104.
// Только для обычной консоли Windows (conhost): Windows Terminal не даёт менять палитру
public class PaletteConsoleRenderer : HalfBlockColorRenderer
{
    private const int PaletteSize = 16;

    private readonly IntPtr _out = GetStdHandle(-11);

    // Сколько раз встречался каждый цвет в загруженных картинках — по этому выбирается палитра
    private readonly Dictionary<int, int> _colorUsage = new Dictionary<int, int>();
    private readonly Dictionary<int, byte> _colorIndex = new Dictionary<int, byte>();
    private readonly int[] _palette = new int[PaletteSize];
    private bool _paletteDirty = true;

    private CharInfo[] _buffer = Array.Empty<CharInfo>();

    public override Cell PixelToCell(byte r, byte g, byte b, byte a)
    {
        Cell cell = base.PixelToCell(r, g, b, a);
        if (!cell.IsEmpty) CountColor(cell.Color);
        return cell;
    }

    public override Cell BackgroundCell(int rgb)
    {
        // Фон заливает почти весь экран — он точно должен попасть в палитру
        CountColor(rgb, 1_000_000);
        return base.BackgroundCell(rgb);
    }

    public override void Render(Cell[,] frame)
    {
        if (_paletteDirty) ApplyPalette();

        (int w, int h) = ScreenSize(frame.GetLength(1), frame.GetLength(0));
        if (_buffer.Length != w * h) _buffer = new CharInfo[w * h];
        int frameHeight = frame.GetLength(0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Пиксель кадра — пара одинаковых клеток по X, берём левую
                byte top = IndexOf(frame[2 * y, 2 * x]);
                byte bottom = 2 * y + 1 < frameHeight ? IndexOf(frame[2 * y + 1, 2 * x]) : (byte)0;

                // Атрибут клетки: младшие 4 бита — цвет символа, старшие — цвет фона
                _buffer[y * w + x] = top == bottom
                    ? new CharInfo { Char = ' ', Attributes = (short)(bottom << 4) }
                    : new CharInfo { Char = '▀', Attributes = (short)(top | (bottom << 4)) };
            }
        }

        var area = new SmallRect { Left = 0, Top = 0, Right = (short)(w - 1), Bottom = (short)(h - 1) };
        WriteConsoleOutputW(_out, _buffer, new Coord { X = (short)w, Y = (short)h }, new Coord(), ref area);
    }

    private void CountColor(int rgb, int weight = 1)
    {
        if (!_colorUsage.ContainsKey(rgb)) _paletteDirty = true;
        _colorUsage[rgb] = _colorUsage.GetValueOrDefault(rgb) + weight;
    }

    private byte IndexOf(Cell cell) => cell.IsEmpty ? (byte)0 : IndexOf(cell.Color);

    private byte IndexOf(int rgb)
    {
        if (_colorIndex.TryGetValue(rgb, out byte index)) return index;

        // Цвета нет в палитре (их было больше 16) — берём ближайший
        byte best = 0;
        int bestDistance = int.MaxValue;
        for (int i = 0; i < PaletteSize; i++)
        {
            int dr = ((rgb >> 16) & 0xFF) - ((_palette[i] >> 16) & 0xFF);
            int dg = ((rgb >> 8) & 0xFF) - ((_palette[i] >> 8) & 0xFF);
            int db = (rgb & 0xFF) - (_palette[i] & 0xFF);
            int distance = dr * dr + dg * dg + db * db;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = (byte)i;
            }
        }
        _colorIndex[rgb] = best;
        return best;
    }

    // Палитра ставится один раз (и заново, только если загрузились картинки с новыми цветами)
    private void ApplyPalette()
    {
        _paletteDirty = false;
        _colorIndex.Clear();

        // Номер 0 — чёрный: им красятся пустые клетки
        List<int> colors = _colorUsage.Where(kv => kv.Key != 0)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .Take(PaletteSize - 1)
            .Prepend(0)
            .ToList();

        Array.Clear(_palette);
        for (int i = 0; i < colors.Count; i++)
        {
            _palette[i] = colors[i];
            _colorIndex[colors[i]] = (byte)i;
        }

        var info = new ScreenBufferInfoEx { cbSize = Marshal.SizeOf<ScreenBufferInfoEx>(), ColorTable = new uint[PaletteSize] };
        if (!GetConsoleScreenBufferInfoEx(_out, ref info)) return;

        SmallRect window = info.srWindow;
        for (int i = 0; i < PaletteSize; i++)
        {
            int rgb = _palette[i];
            // COLORREF хранит цвет как 0x00BBGGRR
            info.ColorTable[i] = (uint)(((rgb & 0xFF) << 16) | (rgb & 0xFF00) | ((rgb >> 16) & 0xFF));
        }
        SetConsoleScreenBufferInfoEx(_out, ref info);

        // SetConsoleScreenBufferInfoEx меняет размер окна — возвращаем прежний
        SetConsoleWindowInfo(_out, true, ref window);
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    private struct CharInfo
    {
        [FieldOffset(0)] public char Char;
        [FieldOffset(2)] public short Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ScreenBufferInfoEx
    {
        public int cbSize;
        public Coord dwSize;
        public Coord dwCursorPosition;
        public short wAttributes;
        public SmallRect srWindow;
        public Coord dwMaximumWindowSize;
        public short wPopupAttributes;
        public bool bFullscreenSupported;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public uint[] ColorTable;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool WriteConsoleOutputW(IntPtr hConsoleOutput, CharInfo[] buffer, Coord bufferSize,
        Coord bufferCoord, ref SmallRect writeRegion);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref ScreenBufferInfoEx info);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref ScreenBufferInfoEx info);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool absolute, ref SmallRect window);
}
