using MonochromeEngine.Utils;

namespace MonochromeEngine;

public class Hero
{
    private readonly List<Sprite> _sprites = new List<Sprite>();

    public Hero(List<Sprite> sprites)
    {
        _sprites = sprites;
    }
    
    public void Show()
    {
        foreach (var sprite in _sprites)
        {
            //Console.WriteLine(sprite.Owner);
            {
                foreach (var s in sprite.animation)
                {
                    Console.WriteLine(s.Key);
                }
            }
        }
    }
}