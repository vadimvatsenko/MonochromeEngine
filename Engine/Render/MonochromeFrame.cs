namespace MonochromeEngine.Engine.Render;

public class MonochromeFrame : ISpriteFrame
{
    public char[,] Data { get; }
    public int Width => Data.GetLength(1);
    public int Height => Data.GetLength(0);

    public MonochromeFrame(char[,] data)
    {
        Data = data;
    }

    public object GetRawData() => Data;
}
