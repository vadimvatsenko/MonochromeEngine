namespace MonochromeEngine.Engine.Render;

// Всё, что рисует игра, идёт через этот интерфейс. Режим (ч/б или цвет)
// выбирается в Program.cs — остальной код о нём не знает
public interface IRenderer
{
    // Как пиксель картинки становится клеткой: прозрачной или закрашенной каким-то цветом
    Cell PixelToCell(byte r, byte g, byte b, byte a);

    // Чем заливать фон уровня цвета rgb (в ч/б режиме фон пустой)
    Cell BackgroundCell(int rgb);

    Cell[,] CreateLayer(int w, int h);
    void Fill(Cell[,] layer, Cell cell);
    void Clear(Cell[,] layer);
    void DrawCell(Cell[,] layer, int x, int y, Cell cell);
    void DrawString(Cell[,] layer, int x, int y, string s, int color);
    void DrawLine(Cell[,] layer, int x0, int y0, int x1, int y1, Cell cell);
    void DrawRect(Cell[,] layer, int x, int y, int w, int h, Cell cell);

    // Слои накладываются по порядку: непрозрачная клетка верхнего слоя закрывает нижние
    Cell[,] Compose(int w, int h, params Cell[][,] layers);
    void Render(Cell[,] frame);
}
