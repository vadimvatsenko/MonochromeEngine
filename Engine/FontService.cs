using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine;

public class FontService
{
    private readonly SpritesLoaderSystem _spritesLoader;

    public FontService(SpritesLoaderSystem spritesLoader)
    {
        _spritesLoader = spritesLoader;
    }

    // Метод для получения спрайта для конкретного символа
    public char[,] GetCharSprite(char c)
    {
        // В SpritesLoaderSystem имена собираются по структуре:
        // Папка "Fonts" -> Подпапка "MainFont" -> Имя анимации "IdleBase"
        // Наш текущий загрузчик видит "MainFont" как Owner (владельца),
        // а "Idle" + "Base" как анимацию.
        
        string key = "MainFont";
        string animName = $"Idle{char.ToUpper(c)}"; // Например: "IdleA", "IdleB"
        
        if (_spritesLoader.Sprites.TryGetValue(key, out Sprite sprite))
        {
            var frames = sprite.GetAnimationFrames(animName);
            if (frames != null && frames.Count > 0)
                return frames[0];
        }
        return null;
    }

    public void DrawText(MonoRenderer renderer, char[,] layer, string text, int x, int y)
    {
        int cursorX = x;
        foreach (char c in text)
        {
            char[,] charSprite = GetCharSprite(c);
            if (charSprite != null)
            {
                // Итерируемся по маске спрайта и рисуем каждый символ
                int h = charSprite.GetLength(0);
                int w = charSprite.GetLength(1);
                for (int sy = 0; sy < h; sy++)
                {
                    for (int sx = 0; sx < w; sx++)
                    {
                        char pixel = charSprite[sy, sx];
                        if (pixel != ' ') // Предполагаем ' ' - прозрачный пиксель
                        {
                            renderer.DrawChar(layer, cursorX + sx, y + sy, pixel);
                        }
                    }
                }
                cursorX += w + 2; // +2 для отступа между буквами
            }
        }
    }
}
