using MonochromeEngine.Scene;

public class MainMenuScene : IScene
{
    private readonly Input _input;

    public MainMenuScene(Input input) => _input = input;

    public void Initialize() { /* Сброс настроек меню */ }

    public void Update(double deltaTime)
    {
        // Если игрок нажал Enter — запускаем первый уровень
        if (_input.GetKeyDown(ConsoleKey.Enter))
        {
            SceneManager.ChangeScene(new GameplayScene(_input, 1)); // Загружаем 1-й уровень
        }
    }

    public void Draw()
    {
        // Отрисовка текста меню через твой _renderer
        // _renderer.DrawString(..., "=== MONOCHROME ENGINE ===");
        // _renderer.DrawString(..., "Press ENTER to Start");
    }
}