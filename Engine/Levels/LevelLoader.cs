using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MonochromeEngine.Engine.Levels;

public static class LevelLoader
{
    // Как и Sprites: путь от bin/Debug/net10.0 к папке проекта
    private static readonly string LevelsPath = Path.Combine("..", "..", "..", "Levels");

    // Пиксель спрайта рисуется двумя символами, чтобы в консоли был квадратным
    public const int CharsPerPixelX = 2;
    public const int CharsPerPixelY = 1;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Склоны: номер тайла -> высота поверхности (в пикселях от верха тайла) в каждом столбике пикселей.
    // Форма берётся прямо из картинки тайла: где в столбике начинается непрозрачное
    public static Dictionary<int, int[]> FindSlopes(LevelData level)
    {
        var slopes = new Dictionary<int, int[]>();
        if (level.Tileset.Count == 0) return slopes;

        using var image = Image.Load<Rgba32>(Path.Combine(level.Directory, level.Tileset.Image));
        int tw = level.TileWidth, th = level.TileHeight;

        foreach (int id in level.Tileset.Solid)
        {
            if (id <= 0 || id > level.Tileset.Count) continue;
            int ox = (id - 1) % level.Tileset.Columns * tw;
            int oy = (id - 1) / level.Tileset.Columns * th;

            var heights = new int[tw];
            for (int x = 0; x < tw; x++)
            {
                heights[x] = th;
                for (int y = 0; y < th; y++)
                {
                    if (image[ox + x, oy + y].A >= 10)
                    {
                        heights[x] = y;
                        break;
                    }
                }
            }

            if (IsSlope(heights)) slopes[id] = heights;
        }

        return slopes;
    }

    // Склон — поверхность плавно идёт вверх или вниз заметно. Скруглённые на 1-2 пикселя углы
    // и стенки с резким обрывом остаются обычными квадратными блоками
    private static bool IsSlope(int[] heights)
    {
        bool rising = true, falling = true;
        for (int i = 1; i < heights.Length; i++)
        {
            int step = heights[i] - heights[i - 1];
            if (step < 0) rising = false;
            if (step > 0) falling = false;
            if (Math.Abs(step) > 2) return false;
        }
        return (rising || falling) && heights.Max() - heights.Min() >= 3;
    }

    // null — файла нет, тогда сцена строит старый уровень из LevelModel
    public static LevelData? TryLoad(string fileName)
    {
        string path = Path.Combine(LevelsPath, fileName);
        if (!File.Exists(path)) return null;

        LevelData level = JsonSerializer.Deserialize<LevelData>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("Пустой файл уровня: " + path);
        level.Directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        return level;
    }
}
