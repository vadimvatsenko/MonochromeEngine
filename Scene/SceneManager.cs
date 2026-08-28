using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Scene;

public static class SceneManager
{
    private static IScene _currentScene;
    private static MonoRenderer _renderer;
    private static int _mapWidth;
    private static int _mapHeight;
    private static List<char[,]> _layers;

    public static void Initialize(MonoRenderer renderer, int mapWidth, int mapHeight, List<char[,]> layers)
    {
        _renderer = renderer;
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _layers = layers;
    }

    public static void ChangeScene(IScene newScene)
    {
        _currentScene = newScene;
        _currentScene.Initialize();
    }

    public static void Update(double deltaTime)
    {
        _currentScene?.Update(deltaTime);
    }

    public static void Draw()
    {
        if (_currentScene == null) 
        {
            Console.WriteLine("SceneManager: CurrentScene is null!");
            return;
        }
        
        // Console.WriteLine("SceneManager: Drawing scene...");
        
        // 1. Очистка всех слоев кроме background (индекс 0) перед отрисовкой
        for (int i = 1; i < _layers.Count; i++)
        {
            _renderer.Clear(_layers[i]);
        }
        
        // 2. Отрисовываем сцену (объекты рисуют в слои)
        _currentScene.Draw();
        
        // 3. Композиция и рендер
        var frame = _renderer.Compose(_mapWidth, _mapHeight, _layers.ToArray());
        _renderer.Render(frame);
    }
}
