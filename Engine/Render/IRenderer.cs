namespace MonochromeEngine.Engine.Render;

public interface IRenderer
{
    // Render works with a base type or needs to be generic to support both char[,] and ConsolePixel[,]
    void Render(object frame);
}
