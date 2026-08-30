namespace MonochromeEngine.Engine.Render;

public class ColoredFrame : ISpriteFrame
{
    public ConsolePixel[,] Data { get; }
    public int Width => Data.GetLength(1);
    public int Height => Data.GetLength(0);

    public ColoredFrame(ConsolePixel[,] data)
    {
        Data = data;
    }

    public object GetRawData() => Data;
}
