using MonochromeEngine;
using MonochromeEngine.Creatures;
using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Scene;
using MonochromeEngine.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Monochrome Engine - Monochrome Engine";
        Console.CursorVisible = false;
        
        Input input = new Input();
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

        SceneManager sceneManager = new SceneManager(renderer, map.Width, map.Height, allLayers);
        
        // Temporarily create MainMenuScene or jump straight to Gameplay if MainMenu isn't fully refactored.
        // Assuming we need to refactor MainMenuScene constructor first, which I will do in next turns.
        // For now, let's push GameplayScene directly.
        GameplayScene gameplayScene = new GameplayScene(sceneManager, input, renderer, spritesLoaderSystem, map, uiLayer, allLayers);
        sceneManager.PushScene(gameplayScene);

        Update update = new Update(60, sceneManager);
        
        update.RunUpdate();
    }
}
