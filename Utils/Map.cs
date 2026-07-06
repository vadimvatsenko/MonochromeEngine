namespace MonochromeEngine.Utils;

public class Map
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public List<Block> Blocks { get; private set; } = new List<Block>();

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        
    }
    
    public void SetBlock(Block block)
    {
        Blocks.Add(block);
    }
}