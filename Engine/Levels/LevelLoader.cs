using System.Text.Json;

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
