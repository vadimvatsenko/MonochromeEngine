namespace MonochromeEngine.Scene;

public static class SceneManager
{
    private static IScene _currentScene;

    public static void ChangeScene(IScene newScene)
    {
        _currentScene = newScene;
        _currentScene.Initialize(); // Настраиваем сцену при входе (спавн мобов, загрузка карты)
    }

    public static void Update(double deltaTime)
    {
        _currentScene?.Update(deltaTime);
    }

    public static void Draw()
    {
        _currentScene?.Draw();
    }
}