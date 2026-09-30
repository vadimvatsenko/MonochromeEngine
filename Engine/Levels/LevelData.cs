using System.Text.Json.Serialization;

namespace MonochromeEngine.Engine.Levels;

// Формат файла уровня из редактора уровней TileSlicer (вкладка "Редактор уровней").
// Координаты в файле — в пикселях картинки, в движке пиксель = 2 символа по X и 1 по Y
public class LevelData
{
    public int Version { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public string Background { get; set; } = "";
    public LevelTileset Tileset { get; set; } = new LevelTileset();
    // Ground[y][x]: 0 — пусто, иначе номер тайла с 1
    public int[][] Ground { get; set; } = [];
    public List<string> EntityTypes { get; set; } = new List<string>();
    public List<LevelEntity> Entities { get; set; } = new List<LevelEntity>();

    // Папка файла уровня — картинка тайлов лежит рядом
    [JsonIgnore]
    public string Directory { get; set; } = "";
}

public class LevelTileset
{
    public string Image { get; set; } = "";
    public int Columns { get; set; }
    public int Count { get; set; }
    // Номера тайлов, сквозь которые нельзя пройти
    public List<int> Solid { get; set; } = new List<int>();
}

public class LevelEntity
{
    public string Type { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public Dictionary<string, string> Props { get; set; } = new Dictionary<string, string>();
}
