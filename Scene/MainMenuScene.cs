using MonochromeEngine.Engine;
using MonochromeEngine.Scene;
using MonochromeEngine.Utils;

public class MainMenuScene : IScene
{
    private readonly Input _input;
    private readonly MonoRenderer _renderer;
    private readonly char[,] _uiLayer;
    private readonly FontService _fontService;
    
    // Зависимости для GameplayScene
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly Map _map;
    private readonly List<char[,]> _allLayers;

    public MainMenuScene(Input input, MonoRenderer renderer, char[,] uiLayer, FontService fontService, 
                         SpritesLoaderSystem spritesLoaderSystem, Map map, List<char[,]> allLayers)
    {
        _input = input;
        _renderer = renderer;
        _uiLayer = uiLayer;
        _fontService = fontService;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        _allLayers = allLayers;
    }

    public void Initialize() { }

    public void Update(double deltaTime)
    {
        if (_input.GetKeyDown(ConsoleKey.Enter))
        {
            SceneManager.ChangeScene(new GameplayScene(_input, _renderer, _spritesLoaderSystem, _map, _uiLayer, _allLayers));
        }
    }

    public void Draw()
    {
        _renderer.Fill(_uiLayer, ' ');
        _fontService.DrawText(_renderer, _uiLayer, "MONOCHROME", 10, 5);
        _fontService.DrawText(_renderer, _uiLayer, "START", 10, 15);
    }
}