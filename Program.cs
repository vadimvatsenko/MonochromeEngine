using MonochromeEngine;
using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Monochrome Engine - Monochrome Engine";
        Console.CursorVisible = false;
        
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();
        Map map = new Map(420,160);
        MonoRenderer renderer = new MonoRenderer();
        
        var backgroundLayer = renderer.CreateLayer(map.Width, map.Height);
        var itemsLayer = renderer.CreateLayer(map.Width, map.Height);
        var uiLayer = renderer.CreateLayer(map.Width, map.Height);
        
        renderer.Fill(backgroundLayer, '.');

        List<char[,]> allLayers = new List<char[,]>()
        {
            backgroundLayer,
            itemsLayer,
            uiLayer
        };

        BaseGameObject baseGameObject = new BaseGameObject(renderer, itemsLayer, spritesLoaderSystem, map);
        Update update = new Update(60, renderer, map, allLayers);
        
        update.AddUpdatable(baseGameObject);
        update.RunUpdate();
        
    }
}