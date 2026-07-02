using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MonochromeEngine.Utils;

public class SpritesLoaderSystem
{
    string path = Path.Combine("..", "..", "..", "Sprites");
    
    private List<Sprite> _sprites = new List<Sprite>();
    
    public SpritesLoaderSystem()
    {
        AllPathes();
    }
    
    public List<Sprite> Sprites { get => _sprites; }

    private async void AllPathes()
    {
        // підпапки Directory.EnumerateDirectories(path)
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            string dirBaseFolder = Path.GetFileName(dir); // Character Fonts Items
            //Console.ResetColor();
            
            foreach (var subBaseFolder in Directory.EnumerateDirectories(dir))
            {
                // Отримає назву файла Path.GetFileName(file)
                // Hero MainFont Items
                string subFolderName = Path.GetFileName(subBaseFolder);
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.Write($"\t{subFolderName} ");
                //Console.WriteLine();
                
                foreach (var animFolder in Directory.EnumerateDirectories(subBaseFolder))
                {
                    string animFolderName = Path.GetFileName(animFolder);
                    //Console.ForegroundColor = ConsoleColor.Blue;
                    //Console.Write($"\t \t{animFolderName} ");
                    //Console.WriteLine();

                    foreach (var dirAnimFolder in Directory.EnumerateDirectories(animFolder))
                    {
                        string animDirFolderName = Path.GetFileName(dirAnimFolder);
                        //Console.ForegroundColor = ConsoleColor.Cyan;
                        //Console.Write($"\t \t \t{animDirFolderName} ");
                        //Console.WriteLine();
                        
                        foreach (var fileNamePath in Directory.EnumerateFiles(dirAnimFolder, "*.png"))
                        {
                            //Console.ForegroundColor = ConsoleColor.Magenta;
                            //Console.WriteLine($"\t \t \t \t{fileNamePath} ");
                            Sprite sprite = new Sprite(subFolderName);
                            char[,] currentSprite = LoadCharMask2D(fileNamePath);
                            sprite.AddAnimation(animFolderName+animDirFolderName, currentSprite);
                        
                            _sprites.Add(sprite);
                        }
                    }
                }
            }
        }
    }

    
    private char[,] LoadCharMask2D(
        string path,
        int spriteW = 48,
        int spriteH = 48,
        char fillChar = '█',
        char emptyChar = ' ',
        byte threshold = 128,
        bool invert = false)
    {
        using var img = Image.Load<Rgba32>(path);

        // Центр изображения
        int cx = img.Width / 2;
        int cy = img.Height / 2;

        // Левый верхний угол окна (spriteW x spriteH), вырезаем от центра
        int startX = cx - spriteW / 2;
        int startY = cy - spriteH / 2;

        // ВАЖНО: ширина x2 (каждый пиксель -> 2 символа)
        var mask = new char[spriteH, spriteW * 2];

        img.ProcessPixelRows(accessor =>
        {
            for (int sy = 0; sy < spriteH; sy++)
            {
                int y = startY + sy;

                // Если строка вне картинки — просто заполняем пустотой
                if (y < 0 || y >= img.Height)
                {
                    for (int sx = 0; sx < spriteW; sx++)
                    {
                        int xx = sx * 2;
                        mask[sy, xx] = emptyChar;
                        mask[sy, xx + 1] = emptyChar;
                    }
                    continue;
                }

                var row = accessor.GetRowSpan(y);

                for (int sx = 0; sx < spriteW; sx++)
                {
                    int x = startX + sx;

                    char c = emptyChar;

                    if (x >= 0 && x < img.Width)
                    {
                        var p = row[x];

                        bool isDark = p.A >= 10 &&
                                      p.R < threshold &&
                                      p.G < threshold &&
                                      p.B < threshold;

                        if (invert) isDark = !isDark;

                        c = isDark ? fillChar : emptyChar;
                    }

                    int xx = sx * 2;
                    mask[sy, xx] = c;
                    mask[sy, xx + 1] = c; // второй символ подряд
                }
            }
        });

        return mask;
    }
}