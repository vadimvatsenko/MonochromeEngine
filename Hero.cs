using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Hero
{
    private List<Sprite> _sprites = new List<Sprite>();

    public Hero(List<Sprite> sprites)
    {
        _sprites = sprites;
    }
    
    public void Show()
    {
        foreach (var sprite in _sprites)
        {
            
        }
    }
}