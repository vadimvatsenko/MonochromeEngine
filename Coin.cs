using MonochromeEngine.Engine;
using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Coin: IUpdatable
{
    private readonly List<Sprite> _coinSprites = new List<Sprite>();
    
    private readonly char[,] _layer;
    private readonly MonoRenderer _renderer;
    public Coin(MonoRenderer renderer, char[,] layer, List<Sprite> sprites)
    {
        _renderer = renderer;
        _layer = layer;
        _coinSprites = sprites;
        
    }

    public void Update(double deltatime)
    {
        foreach (Sprite sprite in _coinSprites)
        {
            foreach (var s in sprite.animation)
            {
                for (int i = 0; i < s.Value.Count; i++)
                {
                    
                }
            }
        }
    }
}