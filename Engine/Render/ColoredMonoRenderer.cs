namespace MonochromeEngine.Engine.Render;

public class ColoredMonoRenderer : IRenderer
{
    private readonly ConsolePixel _transparent = new ConsolePixel(' ', ConsoleColor.White);

    public void Fill(ConsolePixel[,] layer, char s, ConsoleColor color)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                layer[y, x] = new ConsolePixel(s, color);
    }

    public ConsolePixel[,] CreateLayer(int w, int h)
    {
        return new ConsolePixel[h, w];
    }

    public void Clear(ConsolePixel[,] layer)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                layer[y, x] = _transparent;
    }

    public void DrawChar(ConsolePixel[,] layer, int x, int y, char c, ConsoleColor color)
    {
        int h = layer.GetLength(0);
        int w = layer.GetLength(1);
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        layer[y, x] = new ConsolePixel(c, color);
    }

    public ConsolePixel[,] Compose(int w, int h, params ConsolePixel[][,] layers)
    {
        var outFrame = new ConsolePixel[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                outFrame[y, x] = _transparent;
                foreach (var layer in layers)
                {
                    if (layer[y, x].Symbol != _transparent.Symbol)
                    {
                        outFrame[y, x] = layer[y, x];
                    }
                }
            }
        }
        return outFrame;
    }

    public void Render(object frame)
    {
        if (frame is ConsolePixel[,] pixelFrame)
        {
            Render(pixelFrame);
        }
        else
        {
            throw new ArgumentException("Frame must be of type ConsolePixel[,]");
        }
    }

    private void Render(ConsolePixel[,] frame)
    {
        Console.SetCursorPosition(0, 0);
        int h = frame.GetLength(0);
        int w = frame.GetLength(1);
        
        ConsoleColor currentColor = Console.ForegroundColor;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                ConsolePixel pixel = frame[y, x];
                if (pixel.Color != currentColor)
                {
                    Console.ForegroundColor = pixel.Color;
                    currentColor = pixel.Color;
                }
                Console.Write(pixel.Symbol);
            }
            Console.WriteLine();
        }
    }
}
