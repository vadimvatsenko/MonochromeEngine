using MonochromeEngine.Engine.Render;

namespace MonochromeEngine.Engine.Utils;

public class Sprite 
{
    public string Owner { get; private set; }
    
    // Делаем словарь приватным сеттером, чтобы извне его никто случайно не стёр
    public Dictionary<string, List<Cell[,]>> Animations { get; private set; } 
        = new Dictionary<string, List<Cell[,]>>(StringComparer.OrdinalIgnoreCase);

    public Sprite(string owner)
    {
        Owner = owner;
    }

    public void AddAnimation(string name, Cell[,] anim)
    {
        if (!Animations.ContainsKey(name))
        {
            Animations.Add(name, new List<Cell[,]> { anim });
        }
        else
        {
            Animations[name].Add(anim);
        }
    }

    // Безопасный метод для получения кадров анимации
    public List<Cell[,]> GetAnimationFrames(string animationName)
    {
        if (Animations.TryGetValue(animationName, out var frames))
        {
            return frames;
        }
        
        // Если анимация не найдена, возвращаем пустой список (или логгируем ошибку)
        return new List<Cell[,]>();
    }
}
