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

        Input input = new Input();
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();
        Map map = new Map(576, 208);
        MonoRenderer renderer = new MonoRenderer();
        
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
