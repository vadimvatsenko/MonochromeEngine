using MonochromeEngine;
using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Monochrome Engine - Monochrome Engine";
        Console.CursorVisible = false;
        
        Input objInput = new Input();
        
        SpritesLoaderSystem spritesLoaderSystem = new SpritesLoaderSystem();
        Map map = new Map(512,192);
        MonoRenderer renderer = new MonoRenderer();
        
        var backgroundLayer = renderer.CreateLayer(map.Width, map.Height);
        var groundLayer = renderer.CreateLayer(map.Width, map.Height);
        var itemsLayer = renderer.CreateLayer(map.Width, map.Height);
        var uiLayer = renderer.CreateLayer(map.Width, map.Height);
        
        renderer.Fill(backgroundLayer, '.');

        List<char[,]> allLayers = new List<char[,]>()
        {
            backgroundLayer,
            groundLayer,
            itemsLayer,
            uiLayer
        };

        BaseGameObject baseGameObject = new BaseGameObject(renderer, objInput, itemsLayer, spritesLoaderSystem, map);

        int stepBlocksX = 32;
        int stepBlocksY = 16;
        
        int calcCountGroundBlocksX = map.Width / stepBlocksX;
        int calcCountGroundBlocksY = map.Height / stepBlocksY;

        List<Block> allBlocks = new List<Block>();
        for (int x = 0; x < calcCountGroundBlocksX; x++)
        {
            for (int y = 0; y < calcCountGroundBlocksY; y++)
            {
                // Убрали проверку потолка (y == 0)
                bool isLeftWall   = (x == 0);
                bool isRightWall  = (x == calcCountGroundBlocksX - 1);
                bool isFloor      = (y == calcCountGroundBlocksY - 1);

                // Строим блок, если это левая/правая стена или пол
                if (isLeftWall || isRightWall || isFloor)
                {
                    Vector2 blockPosition = new Vector2(x * stepBlocksX, y * stepBlocksY);
                    Block block = new Block(renderer, groundLayer, spritesLoaderSystem, map, blockPosition);
            
                    allBlocks.Add(block);
                    map.SetBlock(block);
                }
            }
        }
        
        Update update = new Update(60, renderer, map, allLayers);
        
        update.AddUpdatable(baseGameObject);
        update.AddUpdatable(objInput);

        foreach (var b in allBlocks)
        {
            update.AddUpdatable(b);
        }
        
        update.RunUpdate();
    }
}