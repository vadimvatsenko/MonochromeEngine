using MonochromeEngine;
using MonochromeEngine.Creatures;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Layers;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;
using MonochromeEngine.Scene;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Monochrome Engine - Monochrome Engine";
        Console.CursorVisible = false;

        // Режим отрисовки (меняется только эта строка):
        //   new MonochromeRenderer()     — чёрно-белый, как раньше; окно консоли 576x208
        //   new ColorRenderer()          — цвет; окно консоли 576x208
        //   new HalfBlockColorRenderer() — цвет полублоками, почти в 2 раза быстрее;
        //                                  окно консоли 288x104 (шрифт вдвое крупнее)
        //   new PaletteConsoleRenderer() — "как в DOS": 16 цветов, прямая запись в буфер консоли,
        //                                  самый быстрый цвет; окно 288x104; только обычная консоль
        // Для проверки режим можно выбрать и при запуске: MonochromeEngine.exe mono | color | half | palette
        IRenderer renderer = args.FirstOrDefault() switch
        {
            "mono" => new MonochromeRenderer(),
            "color" => new ColorRenderer(),
            "half" => new HalfBlockColorRenderer(),
            "palette" => new PaletteConsoleRenderer(),
            _ => new PaletteConsoleRenderer()
        };

        Input input = new Input();
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem(renderer);
        Map map = new Map(576, 208);
        
        LayerManager layerManager 
            = new LayerManager(renderer, map);
        SceneManager sceneManager 
            = new SceneManager(renderer, map.Width, map.Height, layerManager.GetAllLayers());
        GameplayScene gameplayScene 
            = new GameplayScene(sceneManager, input, renderer, spritesLoaderSystem, map, layerManager);
        
        sceneManager.PushScene(gameplayScene);

        // 50 кадров: герой бежит 100 символов/с, это ровно 2 символа (1 пиксель) за кадр —
        // экран каждый кадр сдвигается на один пиксель, без пауз и рывков
        Update update = new Update(50, sceneManager);

        update.RunUpdate();
    }
}
