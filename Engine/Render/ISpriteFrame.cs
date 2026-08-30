namespace MonochromeEngine.Engine.Render;

public interface ISpriteFrame
{
    int Width { get; }
    int Height { get; }
    // Позволяет получить данные в обобщенном виде, если потребуется
    object GetRawData(); 
}
