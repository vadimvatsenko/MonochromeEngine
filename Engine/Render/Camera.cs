namespace MonochromeEngine.Engine.Render;

// Смещение видимой части уровня. Объекты хранят мировые координаты,
// а при отрисовке вычитается позиция камеры
public static class Camera
{
    public static int X { get; private set; }
    public static int Y { get; private set; }

    // Держит цель в центре экрана, не выезжая за края уровня
    public static void Follow(Vector2 target, int viewWidth, int viewHeight, int levelWidth, int levelHeight)
    {
        X = Math.Clamp((int)target.X - viewWidth / 2, 0, Math.Max(0, levelWidth - viewWidth));
        Y = Math.Clamp((int)target.Y - viewHeight / 2, 0, Math.Max(0, levelHeight - viewHeight));
    }

    public static void Reset()
    {
        X = 0;
        Y = 0;
    }
}
