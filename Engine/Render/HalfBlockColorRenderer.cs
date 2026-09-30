namespace MonochromeEngine.Engine.Render;

// Цветной режим "полублоками": одна клетка консоли показывает два пикселя —
// верхний цветом символа '▀', нижний цветом фона. Клеток в 4 раза меньше, чем в ColorRenderer:
// пиксель в кадре занимает 2 клетки по ширине, здесь — половину клетки по высоте.
// Окно консоли под кадр 576x208 нужно 288x104
public class HalfBlockColorRenderer : ColorRenderer
{
    protected override (int Width, int Height) ScreenSize(int frameWidth, int frameHeight) =>
        (frameWidth / 2, (frameHeight + 1) / 2);

    protected override void ToScreen(Cell[,] frame, ScreenCell[,] screen)
    {
        int h = screen.GetLength(0);
        int w = screen.GetLength(1);
        int frameHeight = frame.GetLength(0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Пиксель кадра — пара одинаковых клеток по X, берём левую
                int top = ColorOf(frame[2 * y, 2 * x]);
                int bottom = 2 * y + 1 < frameHeight ? ColorOf(frame[2 * y + 1, 2 * x]) : DefaultColor;

                if (top == bottom) screen[y, x] = new ScreenCell(' ', AnyColor, bottom);
                // Верх пустой — рисуем нижнюю половину, иначе пустой верх покрасился бы цветом букв
                else if (top == DefaultColor) screen[y, x] = new ScreenCell('▄', bottom, DefaultColor);
                else screen[y, x] = new ScreenCell('▀', top, bottom);
            }
        }
    }

    private static int ColorOf(Cell c) => c.IsEmpty ? DefaultColor : c.Color;
}
