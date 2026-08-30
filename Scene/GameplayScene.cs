using MonochromeEngine.Creatures;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Layers;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Scene;

public class GameplayScene : IScene
{
    private readonly Input _input;
    private readonly MonoRenderer _renderer;
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly Map _map;
    
    private List<IUpdatable> _updatables;
    private List<IRenderable> _renderables;
    private readonly ISceneManager _sceneManager;
    private readonly LayerManager _layerManager;

    public GameplayScene(ISceneManager sceneManager, 
        Input input, MonoRenderer renderer, 
        SpritesLoaderSystem spritesLoaderSystem, 
        Map map, 
        LayerManager layerManager)
    {
        _sceneManager = sceneManager;
        _input = input;
        _renderer = renderer;
        _spritesLoaderSystem = spritesLoaderSystem;
        _map = map;
        _layerManager = layerManager;
    }

    public void Initialize()
    {
        _updatables = new List<IUpdatable>();
        _renderables = new List<IRenderable>();

        Hero hero 
            = new Hero("Hero", 
                new Vector2(60, 150), 
                _layerManager.GetLayerFromType(RenderLayerType.Player), 
                _map, 
                _renderer, 
                _spritesLoaderSystem, 
                _input);
        Bat bat 
            = new Bat("Bat", 
                new Vector2(250, 100), 
                _layerManager.GetLayerFromType(RenderLayerType.Enemies), 
                _map, 
                _renderer, 
                _spritesLoaderSystem);
        Skeleton skeleton1 
            = new Skeleton("Skeleton", 
                new Vector2(100,150), 
                _layerManager.GetLayerFromType(RenderLayerType.Enemies), 
                _map, 
                _renderer, 
                _spritesLoaderSystem);
        
        Skeleton skeleton2 
            = new Skeleton("Skeleton", 
                new Vector2(200,150), 
                _layerManager.GetLayerFromType(RenderLayerType.Enemies), 
                _map, 
                _renderer, 
                _spritesLoaderSystem);
        
        Skeleton skeleton3 
            = new Skeleton("Skeleton", 
                new Vector2(450,100), 
                _layerManager.GetLayerFromType(RenderLayerType.Enemies), 
                _map, 
                _renderer, 
                _spritesLoaderSystem);
        
        int stepBlocksX = 32;
        int stepBlocksY = 16;
        
        int mapHeight = LevelModel.Map11.GetLength(0);
        int mapWidth = LevelModel.Map11.GetLength(1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int tileId = LevelModel.Map11[y, x];
                if (tileId == 0) continue;

                Vector2 pos = new Vector2(x * stepBlocksX, y * stepBlocksY);
                GroundBlock groundBlock 
                    = new GroundBlock
                        ("Cave", 
                            pos,
                            _layerManager.GetLayerFromType(RenderLayerType.Ground), 
                            _map, 
                            _renderer, 
                            _spritesLoaderSystem, 
                            tileId - 1);
                
                _map.SetBlock(groundBlock);
                _updatables.Add(groundBlock);
                _renderables.Add(groundBlock);
            }
        }
        
        _map.SetHero(hero);
        
        _updatables.Add(hero);
        _renderables.Add(hero);
        _updatables.Add(_input);
        _updatables.Add(bat);
        _renderables.Add(bat);
        
        _updatables.Add(skeleton1);
        _renderables.Add(skeleton1);
        
        _updatables.Add(skeleton2);
        _renderables.Add(skeleton2);
        
        _updatables.Add(skeleton3);
        _renderables.Add(skeleton3);
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
        _renderer.Fill(_layerManager.GetLayerFromType(RenderLayerType.Background), '░');
        
        foreach (var renderable in _renderables)
        {
            renderable.Draw();
        }
    }
    
}
