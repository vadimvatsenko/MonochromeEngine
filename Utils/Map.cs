namespace MonochromeEngine.Utils;

public class Map
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public List<GroundBlock> Blocks { get; private set; } = new List<GroundBlock>();

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        
    }
    
    public void SetBlock(GroundBlock groundBlock)
    {
        Blocks.Add(groundBlock);
    }
}