using MonochromeEngine;
using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();

        List<Sprite> heroSprites = spritesLoaderSystem.Sprites.Where(s => s.Owner == "Hero knight").ToList();
        List<Sprite> coinsSprites = spritesLoaderSystem.Sprites.Where(s => s.Owner == "Coin").ToList();
        
        Console.WriteLine($"Loaded {coinsSprites.Count} sprites");

        Map map = new Map(128, 56);
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

        Coin coin = new Coin(renderer, backgroundLayer, coinsSprites);

        Update update = new Update(60, renderer, map, allLayers);
        update.AddUpdatable(coin);
        update.RunUpdate();
        
        Console.ReadKey();
    }
}