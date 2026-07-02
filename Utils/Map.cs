namespace MonochromeEngine.Utils;

public class Map
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
    }
}