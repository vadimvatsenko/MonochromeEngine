using MonochromeEngine;
using MonochromeEngine.Creatures;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Utils;
using Raylib_cs;

public class Program
{
    public static void Main(string[] args)
    {
        // 32 тайла в ширину 512 пикс
        // 12 тайла высота / 192
        Console.Title = "Monochrome Engine - Monochrome Engine";
        Console.CursorVisible = false;
        
        Input objInput = new Input();
        
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();
        Map map = new Map(576,208);
        MonoRenderer renderer = new MonoRenderer();
        
        var backgroundLayer = renderer.CreateLayer(map.Width, map.Height);
        var groundLayer = renderer.CreateLayer(map.Width, map.Height);
        var itemsLayer = renderer.CreateLayer(map.Width, map.Height);
        var enemiesLayer = renderer.CreateLayer(map.Width, map.Height);
        var uiLayer = renderer.CreateLayer(map.Width, map.Height);
        
        renderer.Fill(backgroundLayer, ' ');

        List<char[,]> allLayers = new List<char[,]>()
        {
            backgroundLayer,
            groundLayer,
            enemiesLayer,
            itemsLayer,
            uiLayer
        };

        Hero baseGameObject = new Hero(renderer, objInput, itemsLayer, spritesLoaderSystem, map);
        //Hero baseGameObject = new Hero(renderer, objInput, itemsLayer, spritesLoaderSystem, map);
        Bat bat = new Bat(renderer, enemiesLayer, spritesLoaderSystem, map);
        Skeleton skeleton = new Skeleton(renderer, enemiesLayer, spritesLoaderSystem, map);

        int stepBlocksX = 32;
        int stepBlocksY = 16;
        
        List<GroundBlock> allBlocks = new List<GroundBlock>();

        // GetLength(0) — это количество СТРОК (высота карты по Y)
        int mapHeight = LevelModel.Map11.GetLength(0);
        // GetLength(1) — это количество СТОЛБЦОВ в строке (ширина карты по X)
        int mapWidth = LevelModel.Map11.GetLength(1);

        // 1. Внешний цикл ВСЕГДА идёт по Y (сверху вниз)
        for (int y = 0; y < mapHeight; y++)
        {
            // 2. Внутренний цикл ВСЕГДА идёт по X (слева направо)
            for (int x = 0; x < mapWidth; x++)
            {
                // Вытаскиваем ID тайла из массива по правильным индексам [строка, столбец]
                int tileId = LevelModel.Map11[y, x];

                // Если это 0 (воздух), блок создавать не нужно! Пропускаем итерацию.
                if (tileId == 0) continue;

                // Теперь координаты X и Y умножаются на свои шаги правильно
                Vector2 pos = new Vector2(x * stepBlocksX, y * stepBlocksY);
        
                GroundBlock groundBlock = new GroundBlock(
                    renderer, 
                    groundLayer, 
                    spritesLoaderSystem, 
                    map, 
                    pos,
                    tileId - 1
                );

                allBlocks.Add(groundBlock);
                map.SetBlock(groundBlock);
            }
        }
        
        Update update = new Update(60, renderer, map, allLayers);
        
        update.AddUpdatable(baseGameObject);
        update.AddUpdatable(objInput);
        update.AddUpdatable(bat);
        update.AddUpdatable(skeleton);

        foreach (var b in allBlocks)
        {
            update.AddUpdatable(b);
        }
        
        update.RunUpdate();
        
        
    }
}

