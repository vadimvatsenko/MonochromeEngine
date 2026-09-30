using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MonochromeEngine.Engine.Utils;

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
    public Dictionary<string, Sprite?> Sprites => _spritesMap;

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
                        
                        // Сортируем по номеру кадра: "(10).png" должен идти после "(9).png", а не после "(1).png"
                        var pngFiles = Directory.EnumerateFiles(dirAnimFolder, "*.png")
                                                .OrderBy(NaturalSortKey);

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

    // Числа в имени дополняем нулями, чтобы строки сортировались как числа
    public static string NaturalSortKey(string path) =>
        Regex.Replace(Path.GetFileName(path), @"\d+", m => m.Value.PadLeft(10, '0'));

    // Картинка тайлов уровня: режем на клетки, каждая клетка — кадр IdleBase,
    // чтобы GroundBlock мог выбрать тайл по номеру кадра
    public void AddTileset(string ownerName, string imagePath, int tileWidth, int tileHeight, int columns, int count)
    {
        using var img = Image.Load<Rgba32>(imagePath);
        var sprite = new Sprite(ownerName);

        for (int i = 0; i < count; i++)
        {
            int ox = i % columns * tileWidth;
            int oy = i / columns * tileHeight;
            sprite.AddAnimation("IdleBase", LoadCharMask2D(img, ox, oy, tileWidth, tileHeight));
        }

        _spritesMap[ownerName] = sprite;
    }

    private char[,] LoadCharMask2D(string path, char fillChar = '█', char emptyChar = ' ', byte threshold = 128, bool invert = false)
    {
        using var img = Image.Load<Rgba32>(path);
        return LoadCharMask2D(img, 0, 0, img.Width, img.Height, fillChar, emptyChar, threshold, invert);
    }

    private char[,] LoadCharMask2D(Image<Rgba32> img, int originX, int originY, int spriteW, int spriteH,
        char fillChar = '█', char emptyChar = ' ', byte threshold = 128, bool invert = false)
    {
        // Выделяем память: под каждый пиксель по ширине — 2 символа для "квадратности" в консоли
        var mask = new char[spriteH, spriteW * 2];

        img.ProcessPixelRows(accessor => {
            for (int y = 0; y < spriteH; y++) {
                var row = accessor.GetRowSpan(originY + y);

                for (int x = 0; x < spriteW; x++) {
                    var p = row[originX + x];
                
                    // Проверяем на "светлый" или "темный". 
                    // Твои спрайты 1-битные (черно-белые). 
                    // Если картинка на черном фоне (как на скриншоте), то пиксели персонажей — белые.
                    // Давай проверять яркость (R > threshold), либо подкрутим invert.
                    bool isActiveColor = p.A >= 10 && (p.R > threshold || p.G > threshold || p.B > threshold);
                
                    if (invert) isActiveColor = !isActiveColor;
                
                    char c = isActiveColor ? fillChar : emptyChar;
                
                    // Пишем два символа рядом, чтобы пиксель в консоли был квадратным
                    int xx = x * 2; 
                    mask[y, xx] = c; 
                    mask[y, xx + 1] = c;
                }
            }
        });
    
        return mask;
    }
}