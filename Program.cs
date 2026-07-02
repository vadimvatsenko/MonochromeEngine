using MonochromeEngine;
using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();

        List<Sprite> heroSprites = spritesLoaderSystem.Sprites.Where(s => s.Owner == "Hero knight").ToList();

        Map map = new Map(128, 56);
        MonoRenderer renderer = new MonoRenderer();
        
        var backgroundLayer = renderer.CreateLayer(map.Width, map.Height);
        
        Hero hero = new Hero(heroSprites);

        Update update = new Update(60);
        update.RunUpdate();
        
        Console.ReadKey();
    }
}