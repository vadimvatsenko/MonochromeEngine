using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Render;

namespace MonochromeEngine.Scene;

public class SceneManager : ISceneManager
{
    private readonly Stack<IScene> _sceneStack = new Stack<IScene>();
    private readonly MonoRenderer _renderer;
    private readonly int _mapWidth;
    private readonly int _mapHeight;
    private readonly List<char[,]> _layers;

    public SceneManager(MonoRenderer renderer, int mapWidth, int mapHeight, List<char[,]> layers)
    {
        _renderer = renderer;
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _layers = layers;
    }

    public void PushScene(IScene scene)
    {
        _sceneStack.Push(scene);
        _sceneStack.Peek().Initialize();
    }

    public void PopScene()
    {
        if (_sceneStack.Count > 0)
        {
            _sceneStack.Pop();
        }
    }

    public void ChangeScene(IScene newScene)
    {
        if (_sceneStack.Count > 0)
        {
            _sceneStack.Pop();
        }
        PushScene(newScene);
    }

    public void Update(double deltaTime)
    {
        if (_sceneStack.Count > 0)
        {
            _sceneStack.Peek().Update(deltaTime);
        }
    }

    public void Draw()
    {
        if (_sceneStack.Count == 0)
        {
            return;
        }
        
        // 1. Очистка всех слоев кроме background (индекс 0) перед отрисовкой
        for (int i = 1; i < _layers.Count; i++)
        {
            _renderer.Clear(_layers[i]);
        }
        
        // 2. Отрисовываем сцену (объекты рисуют в слои)
        _sceneStack.Peek().Draw();
        
        // 3. Композиция и рендер
        var frame = _renderer.Compose(_mapWidth, _mapHeight, _layers.ToArray());
        _renderer.Render(frame);
    }
}
