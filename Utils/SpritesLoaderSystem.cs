using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MonochromeEngine.Utils;

public class SpritesLoaderSystem
{
    // Теперь храним спрайты в словаре по имени владельца (Owner) для мгновенного доступа
    private readonly Dictionary<string, Sprite> _spritesMap = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
    private readonly string path = Path.Combine("..", "..", "..", "Sprites");
    
    public SpritesLoaderSystem()
    {
        AllPathes();
    }
    
    // Публичный доступ к словарю
    public Dictionary<string, Sprite> Sprites => _spritesMap;

    private void AllPathes()
    {
        if (!Directory.Exists(path)) return;

        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            foreach (var subBaseFolder in Directory.EnumerateDirectories(dir))
            {
                // Имя владельца (например: "Hero", "Coin", "Skeleton")
                string ownerName = Path.GetFileName(subBaseFolder);
                
                // ИНЖЕНЕРНЫЙ ХОД: Если такого владельца еще нет в системе, 
                // создаем ОДИН объект Sprite для него раз и навсегда
                if (!_spritesMap.ContainsKey(ownerName))
                {
                    _spritesMap.Add(ownerName, new Sprite(ownerName));
                }
                
                Sprite currentOwnerSprite = _spritesMap[ownerName];
                
                foreach (var animFolder in Directory.EnumerateDirectories(subBaseFolder))
                {
                    string animFolderName = Path.GetFileName(animFolder); // Например: "Idle"

                    foreach (var dirAnimFolder in Directory.EnumerateDirectories(animFolder))
                    {
                        string animDirFolderName = Path.GetFileName(dirAnimFolder); // Например: "Base" или "Left"
                        
                        // Собираем полное имя анимации ("IdleBase")
                        string fullAnimationName = animFolderName + animDirFolderName;
                        
                        // Сортируем файлы по имени, чтобы кадры анимации (01.png, 02.png) шли строго по порядку!
                        var pngFiles = Directory.EnumerateFiles(dirAnimFolder, "*.png")
                                                .OrderBy(f => f);

                        foreach (var fileNamePath in pngFiles)
                        {
                            // Конвертируем картинку в текстовую маску
                            char[,] currentFrame = LoadCharMask2D(fileNamePath);
                            
                            // Добавляем кадр в ОДИН И ТОТ ЖЕ объект этого владельца
                            currentOwnerSprite.AddAnimation(fullAnimationName, currentFrame);
                        }
                    }
                }
            }
        }
    }

    private char[,] LoadCharMask2D(string path, int spriteW = 48, int spriteH = 48, char fillChar = '█', char emptyChar = ' ', byte threshold = 128, bool invert = false)
    {
        // Код метода LoadCharMask2D остается без изменений, он у тебя написан отлично!
        using var img = Image.Load<Rgba32>(path);
        int cx = img.Width / 2; int cy = img.Height / 2;
        int startX = cx - spriteW / 2; int startY = cy - spriteH / 2;
        var mask = new char[spriteH, spriteW * 2];
        img.ProcessPixelRows(accessor => {
            for (int sy = 0; sy < spriteH; sy++) {
                int y = startY + sy;
                if (y < 0 || y >= img.Height) {
                    for (int sx = 0; sx < spriteW; sx++) { int xx = sx * 2; mask[sy, xx] = emptyChar; mask[sy, xx + 1] = emptyChar; }
                    continue;
                }
                var row = accessor.GetRowSpan(y);
                for (int sx = 0; sx < spriteW; sx++) {
                    int x = startX + sx; char c = emptyChar;
                    if (x >= 0 && x < img.Width) {
                        var p = row[x];
                        bool isDark = p.A >= 10 && p.R < threshold && p.G < threshold && p.B < threshold;
                        if (invert) isDark = !isDark;
                        c = isDark ? fillChar : emptyChar;
                    }
                    int xx = sx * 2; mask[sy, xx] = c; mask[sy, xx + 1] = c;
                }
            }
        });
        return mask;
    }
}