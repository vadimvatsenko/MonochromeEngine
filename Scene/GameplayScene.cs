using MonochromeEngine.Creatures;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Utils;
using MonochromeEngine.Scene;

namespace MonochromeEngine.Scene;

public class GameplayScene : IScene
{
    private readonly Input _input;
    private readonly MonoRenderer _renderer;
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly Map _map;
    private readonly char[,] _uiLayer;
    private List<IUpdatable> _updatables;
    private List<char[,]> _allLayers;
    private readonly ISceneManager _sceneManager;

    public GameplayScene(ISceneManager sceneManager, Input input, MonoRenderer renderer, SpritesLoaderSystem spritesLoaderSystem, Map map, char[,] uiLayer, List<char[,]> allLayers)
    {
        _sceneManager = sceneManager;
        _input = input;
        _renderer = renderer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        _uiLayer = uiLayer;
        _allLayers = allLayers;
    }

    public void Initialize()
    {
        _updatables = new List<IUpdatable>();

        Hero hero = new Hero(_renderer, _input, _uiLayer, _spritesLoaderSystem, _map);
        Bat bat = new Bat(_renderer, _uiLayer, _spritesLoaderSystem, _map, new Vector2(100, 50));
        Skeleton skeleton = new Skeleton(_renderer, _uiLayer, _spritesLoaderSystem, _map);
        
        Console.WriteLine("GameplayScene Initialized. Objects created.");

        int stepBlocksX = 32;
        int stepBlocksY = 16;
        
        List<GroundBlock> allBlocks = new List<GroundBlock>();

        int mapHeight = LevelModel.Map11.GetLength(0);
        int mapWidth = LevelModel.Map11.GetLength(1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int tileId = LevelModel.Map11[y, x];
                if (tileId == 0) continue;

                Vector2 pos = new Vector2(x * stepBlocksX, y * stepBlocksY);
                GroundBlock groundBlock = new GroundBlock(_renderer, _uiLayer, _spritesLoaderSystem, _map, pos, tileId - 1);
                allBlocks.Add(groundBlock);
                _map.SetBlock(groundBlock);
                _updatables.Add(groundBlock);
            }
        }
        
        _updatables.Add(hero);
        _updatables.Add(_input);
        _updatables.Add(bat);
        _updatables.Add(skeleton);
        
        Console.WriteLine($"Objects added to updatables. Count: {_updatables.Count}");
    }

    public void Update(double deltaTime)
    {
        foreach (var updatable in _updatables)
        {
            updatable.Update(deltaTime);
        }
    }

    public void Draw()
    {
        // Заполняем фоновый слой, чтобы не было черноты
        _renderer.Fill(_allLayers[0], ' ');
        // Здесь можно добавить отрисовку других статичных элементов, если нужно
    }
}
