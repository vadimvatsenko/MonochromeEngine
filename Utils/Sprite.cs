namespace MonochromeEngine.Utils;

public class Sprite 
{
    public string Owner { get; private set; }
    
    // Делаем словарь приватным сеттером, чтобы извне его никто случайно не стёр
    public Dictionary<string, List<char[,]>> Animations { get; private set; } 
        = new Dictionary<string, List<char[,]>>(StringComparer.OrdinalIgnoreCase);

    public Sprite(string owner)
    {
        Owner = owner;
    }

    public void AddAnimation(string name, char[,] anim)
    {
        if (!Animations.ContainsKey(name))
        {
            Animations.Add(name, new List<char[,]> { anim });
        }
        else
        {
            Animations[name].Add(anim);
        }
    }

    // Безопасный метод для получения кадров анимации
    public List<char[,]> GetAnimationFrames(string animationName)
    {
        if (Animations.TryGetValue(animationName, out var frames))
        {
            return frames;
        }
        
        // Если анимация не найдена, возвращаем пустой список (или логгируем ошибку)
        return new List<char[,]>();
    }
}