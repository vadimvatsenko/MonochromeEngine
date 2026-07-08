using System.Text;

namespace MonochromeEngine.Engine;

public class MonoRenderer: IRenderer
{
    private const char Transparent = '\0';
    private StringBuilder _sb;

    // використовується для background, заповнює одним символом повністю квадратну область
    public void Fill(char[,] layer, char s)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            layer[y, x] = s;
    }

    // стрворить масив слоя
    public char[,] CreateLayer(int w, int h)
    {
        return new char[h, w]; // [y,x]
    }

    // очистка слоя вікликаєтся кожен кадр в Update
    public void Clear(char[,] layer)
    {
        // Заповнення прозорістю 
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            layer[y, x] = Transparent;
    }

    // Малює символ, використовується для відмальовування об'єктів
    public void DrawChar(char[,] layer, int x, int y, char c)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        layer[y, x] = c;
    }

    // Draw String
    public void DrawString(char[,] layer, int x, int y, string s)
    {
        for (int i = 0; i < s.Length; i++)
            DrawChar(layer, x + i, y, s[i]);
    }
    
    public void DrawLine(char[,] layer, int x0, int y0, int x1, int y1, char symbol)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Рисуем точку (символ) луча
            DrawChar(layer, x0, y0, symbol);

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

    // Квадрат
    public void DrawRect(char[,] layer, int x, int y, int w, int h, char c)
    {
        for (int i = 0; i < w; i++)
        {
            DrawChar(layer, x + i, y, c);
            DrawChar(layer, x + i, y + h - 1, c);
        }

        for (int j = 0; j < h; j++)
        {
            DrawChar(layer, x, y + j, c);
            DrawChar(layer, x + w - 1, y + j, c);
        }
    }

    // Compose + Render
    // Композитний кадр
    
    public char[,] Compose(int w, int h, params char[][,] layers)
    {
        var outFrame = new char[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                char result = ' '; // якщо взагалі нічого
                foreach (var layer in layers)
                {
                    char c = layer[y, x];
                    if (c != Transparent)
                        result = c;
                }

                outFrame[y, x] = result;
            }
        }

        return outFrame;
    }


    public void Render(char[,] frame)
    {
        int h = frame.GetLength(0);
        int w = frame.GetLength(1);

        // Инициализируем буфер один раз с запасом под перенос строк
        int requiredCapacity = (w + Environment.NewLine.Length) * h;
        if (_sb == null)
        {
            _sb = new StringBuilder(requiredCapacity);
        }
        else
        {
            _sb.Clear(); // Очищаем без перевыделения памяти
        }
    
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                _sb.Append(frame[y, x]);
            }
            _sb.AppendLine(); // Использует системный перевод строки без лишних пробелов
        }

        Console.SetCursorPosition(0, 0);
        Console.Write(_sb); // Console.Write умеет принимать StringBuilder напрямую, .ToString() не нужен!
    }
}