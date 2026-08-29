namespace MonochromeEngine.Scene;

public interface ISceneManager
{
    void PushScene(IScene scene);
    void PopScene();
    void ChangeScene(IScene newScene);
    void Update(double deltaTime);
    void Draw();
}
