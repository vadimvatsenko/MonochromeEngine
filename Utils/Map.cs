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

    public bool IsBlockAt(int x, int y)
    {
        // Проверяем, попадает ли точка x, y в границы какого-либо коллайдера блока
        return Blocks.Any(block => 
            x >= block.Collider.Position.X && 
            x <= block.Collider.Position.X + block.Collider.Size.X &&
            y >= block.Collider.Position.Y &&
            y <= block.Collider.Position.Y + block.Collider.Size.Y);
    }
}