using MonochromeEngine.Engine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MonochromeEngine.Engine.Utils;

public class ColoredSpritesLoader
{
    private readonly string _path = Path.Combine("..", "..", "..", "Sprites");

    private readonly Dictionary<string, ColoredSprite> _coloredSpritesMap = new Dictionary<string, ColoredSprite>(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ColoredSprite> ColoredSprites => _coloredSpritesMap;

    public void LoadColoredSprites()
    {
        if (!Directory.Exists(_path)) return;

        foreach (var dir in Directory.EnumerateDirectories(_path))
        {
            foreach (var subBaseFolder in Directory.EnumerateDirectories(dir))
            {
                string ownerName = Path.GetFileName(subBaseFolder);
                
                if (!_coloredSpritesMap.ContainsKey(ownerName))
                {
                    _coloredSpritesMap.Add(ownerName, new ColoredSprite(ownerName));
                }
                
                ColoredSprite currentOwnerSprite = _coloredSpritesMap[ownerName];
                
                foreach (var animFolder in Directory.EnumerateDirectories(subBaseFolder))
                {
                    string animFolderName = Path.GetFileName(animFolder);
                    foreach (var dirAnimFolder in Directory.EnumerateDirectories(animFolder))
                    {
                        string animDirFolderName = Path.GetFileName(dirAnimFolder);
                        string fullAnimationName = animFolderName + animDirFolderName;
                        
                        var pngFiles = Directory.EnumerateFiles(dirAnimFolder, "*.png").OrderBy(SpritesLoaderSystem.NaturalSortKey);

                        foreach (var fileNamePath in pngFiles)
                        {
                            ConsolePixel[,] currentFrame = LoadColoredMask(fileNamePath);
                            currentOwnerSprite.AddAnimation(fullAnimationName, currentFrame);
                        }
                    }
                }
            }
        }
    }

    private ConsolePixel[,] LoadColoredMask(string path)
    {
        using var img = Image.Load<Rgba32>(path);
        int spriteW = img.Width;
        int spriteH = img.Height;
    
        var mask = new ConsolePixel[spriteH, spriteW * 2];
    
        img.ProcessPixelRows(accessor => {
            for (int y = 0; y < spriteH; y++) {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < spriteW; x++) {
                    var p = row[x];
                    
                    // Преобразуем Rgba32 в ConsoleColor
                    ConsoleColor color = MapToConsoleColor(p);
                    char symbol = '█'; // Или используйте p.A > 0 ? '█' : ' '
                    
                    int xx = x * 2;
                    mask[y, xx] = new ConsolePixel(symbol, color);
                    mask[y, xx + 1] = new ConsolePixel(symbol, color);
                }
            }
        });
        return mask;
    }

    private ConsoleColor MapToConsoleColor(Rgba32 pixel)
    {
        // Упрощенная логика маппинга цветов. 
        // Для полноценного рендера можно расширить до всех 16 цветов консоли.
        if (pixel.R > 200 && pixel.G < 50 && pixel.B < 50) return ConsoleColor.Red;
        if (pixel.R < 50 && pixel.G > 200 && pixel.B < 50) return ConsoleColor.Green;
        if (pixel.R < 50 && pixel.G < 50 && pixel.B > 200) return ConsoleColor.Blue;
        if (pixel.R > 200 && pixel.G > 200 && pixel.B < 50) return ConsoleColor.Yellow;
        if (pixel.R > 200 && pixel.G > 200 && pixel.B > 200) return ConsoleColor.White;
        return ConsoleColor.Gray;
    }
}
