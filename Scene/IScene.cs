namespace MonochromeEngine.Scene;

public interface IScene
{
    void Initialize();
    void Update(double deltaTime);
    void Draw();
}