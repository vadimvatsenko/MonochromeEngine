using MonochromeEngine.Engine.Render;

namespace MonochromeEngine.Engine.Utils;

public class ColoredSprite 
{
    public string Owner { get; private set; }
    
    public Dictionary<string, List<ConsolePixel[,]>> Animations { get; private set; } 
        = new Dictionary<string, List<ConsolePixel[,]>>(StringComparer.OrdinalIgnoreCase);

    public ColoredSprite(string owner)
    {
        Owner = owner;
    }

    public void AddAnimation(string name, ConsolePixel[,] anim)
    {
        if (!Animations.ContainsKey(name))
        {
            Animations.Add(name, new List<ConsolePixel[,]> { anim });
        }
        else
        {
            Animations[name].Add(anim);
        }
    }

    public List<ConsolePixel[,]> GetAnimationFrames(string animationName)
    {
        if (Animations.TryGetValue(animationName, out var frames))
        {
            return frames;
        }
        return new List<ConsolePixel[,]>();
    }
}
