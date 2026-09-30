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
        IRenderer renderer = new ColorRenderer();

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

        Update update = new Update(60, sceneManager);

        update.RunUpdate();
    }
}
