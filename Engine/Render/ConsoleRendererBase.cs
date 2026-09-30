using System.Text;

namespace MonochromeEngine.Engine.Render;

// Общая часть рендеров: работа со слоями одинакова, отличаются только
// превращение пикселя в клетку и вывод кадра в консоль
public abstract class ConsoleRendererBase : IRenderer
{
    protected readonly StringBuilder Sb = new StringBuilder();

    public abstract Cell PixelToCell(byte r, byte g, byte b, byte a);
    public abstract Cell BackgroundCell(int rgb);
    public abstract void Render(Cell[,] frame);

    // стрворить масив слоя
    public Cell[,] CreateLayer(int w, int h)
    {
        return new Cell[h, w]; // [y,x]
    }

    // використовується для background, заповнює одною клітинкою повністю
    public void Fill(Cell[,] layer, Cell cell)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            layer[y, x] = cell;
    }

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

    // Композитний кадр
    public Cell[,] Compose(int w, int h, params Cell[][,] layers)
    {
        var outFrame = new Cell[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Cell result = Cell.Empty; // якщо взагалі нічого
                foreach (var layer in layers)
                {
                    Cell c = layer[y, x];
                    if (!c.IsEmpty) result = c;
                }

                outFrame[y, x] = result;
            }
        }

        return outFrame;
    }

    // Весь кадр пишется одним Console.Write — так в разы быстрее, чем по символу
    protected void WriteFrame()
    {
        Console.SetCursorPosition(0, 0);
        Console.Write(Sb);
    }
}
