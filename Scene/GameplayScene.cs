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
    private readonly IRenderer _renderer;
    private readonly SpritesLoaderSystem _spritesLoaderSystem;
    private readonly Map _map;
    
    private List<IUpdatable> _updatables;
    private List<IRenderable> _renderables;
    private readonly ISceneManager _sceneManager;
    private readonly LayerManager _layerManager;

    public GameplayScene(ISceneManager sceneManager, 
        Input input, IRenderer renderer, 
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

    // Файл уровня из редактора (папка Levels). Если файла нет — строится старый уровень Map11
    private const string LevelFile = "level1.json";
    private const string LevelTilesOwner = "LevelTiles";

    // Размер всего уровня в символах — чтобы камера не выезжала за края
    private int _levelWidth;
    private int _levelHeight;
    // Чем залит фон: в цвете — цветом неба из файла уровня, в ч/б — пусто
    private Cell _backgroundCell;

    public void Initialize()
    {
        _updatables = new List<IUpdatable>();
        _renderables = new List<IRenderable>();
        _map.Clear();
        Camera.Reset();

        LevelData? level = LevelLoader.TryLoad(LevelFile);
        if (level != null)
        {
            BuildLevelFromFile(level);
        }
        else
        {
            BuildLegacyLevel();
        }
    }

    private void BuildLevelFromFile(LevelData level)
    {
        int background = level.Background.StartsWith('#') ? Convert.ToInt32(level.Background[1..], 16) : 0;
        _backgroundCell = _renderer.BackgroundCell(background);

        _spritesLoaderSystem.AddTileset(LevelTilesOwner, Path.Combine(level.Directory, level.Tileset.Image),
            level.TileWidth, level.TileHeight, level.Tileset.Columns, level.Tileset.Count);

        int stepX = level.TileWidth * LevelLoader.CharsPerPixelX;
        int stepY = level.TileHeight * LevelLoader.CharsPerPixelY;
        HashSet<int> solid = level.Tileset.Solid.ToHashSet();
        Dictionary<int, int[]> slopes = LevelLoader.FindSlopes(level);

        for (int y = 0; y < level.Ground.Length; y++)
        {
            for (int x = 0; x < level.Ground[y].Length; x++)
            {
                int tileId = level.Ground[y][x];
                if (tileId <= 0 || tileId > level.Tileset.Count) continue;

                // Украшения (трава, облака) — на фоновый слой, чтобы не закрывали героя
                bool isSolid = solid.Contains(tileId);
                RenderLayerType layer = isSolid ? RenderLayerType.Ground : RenderLayerType.Background;

                GroundBlock block
                    = new GroundBlock(LevelTilesOwner,
                        new Vector2(x * stepX, y * stepY),
                        _layerManager.GetLayerFromType(layer),
                        _map,
                        _renderer,
                        _spritesLoaderSystem,
                        tileId - 1);

                // Сквозь твёрдые тайлы не пройти, остальные (трава, облака) — просто картинка.
                // Тайлы не анимируются, поэтому в _updatables их не кладём
                if (isSolid) _map.SetBlock(block);
                if (slopes.TryGetValue(tileId, out int[]? heights))
                {
                    block.SlopeHeights = heights.Select(h => h * LevelLoader.CharsPerPixelY).ToArray();
                }
                _renderables.Add(block);
            }
        }

        _levelWidth = level.Width * stepX;
        _levelHeight = level.Height * stepY;

        foreach (LevelEntity entity in level.Entities)
        {
            SpawnEntity(entity.Type,
                new Vector2(entity.X * LevelLoader.CharsPerPixelX, entity.Y * LevelLoader.CharsPerPixelY));
        }

        // Герой нужен всегда: за ним едет камера, на него нападают враги
        if (_map.Hero == null) SpawnEntity("Hero", new Vector2(60, 0));

        _updatables.Add(_input);
    }

    private void SpawnEntity(string type, Vector2 position)
    {
        switch (type)
        {
            case "Hero":
                Hero hero
                    = new Hero("Hero",
                        position,
                        _layerManager.GetLayerFromType(RenderLayerType.Player),
                        _map,
                        _renderer,
                        _spritesLoaderSystem,
                        _input);
                _map.SetHero(hero);
                _updatables.Add(hero);
                _renderables.Add(hero);
                break;

            case "Skeleton":
                Skeleton skeleton
                    = new Skeleton("Skeleton",
                        position,
                        _layerManager.GetLayerFromType(RenderLayerType.Enemies),
                        _map,
                        _renderer,
                        _spritesLoaderSystem);
                _updatables.Add(skeleton);
                _renderables.Add(skeleton);
                break;

            case "Bat":
                Bat bat
                    = new Bat("Bat",
                        position,
                        _layerManager.GetLayerFromType(RenderLayerType.Enemies),
                        _map,
                        _renderer,
                        _spritesLoaderSystem);
                _updatables.Add(bat);
                _renderables.Add(bat);
                break;

            // Для других типов (Carrot и т.п.) в движке пока нет класса — пропускаем
        }
    }

    // Старый уровень, записанный в коде
    private void BuildLegacyLevel()
    {
        _backgroundCell = _renderer.BackgroundCell(0x000000);
        _levelWidth = _map.Width;
        _levelHeight = _map.Height;

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

        if (_map.Hero != null)
        {
            // Камера смотрит в центр героя
            var hero = _map.Hero.Collider;
            Camera.Follow(new Vector2(hero.Position.X + hero.Size.X / 2, hero.Position.Y + hero.Size.Y / 2),
                _map.Width, _map.Height, _levelWidth, _levelHeight);
        }
    }

    public void Draw()
    {
        // Заполняем фоновый слой, чтобы не было черноты
        _renderer.Fill(_layerManager.GetLayerFromType(RenderLayerType.Background), _backgroundCell);
        
        foreach (var renderable in _renderables)
        {
            renderable.Draw();
        }
    }
    
}
