namespace MonochromeEngine.Engine;

public class Input : IUpdatable
{
    public Action OnLeft;
    public Action OnRight;
    public Action OnUp;
    public Action OnDown;
    public Action OnAttack;
    public Action OnJump;
    public Action OnCancel;
    
    public void Update(double deltaTime)
    {
        if (!Console.KeyAvailable) return;
        
        ConsoleKeyInfo  keyInfo = Console.ReadKey(true);
        
        switch (keyInfo.Key)
        {
            case ConsoleKey.LeftArrow or ConsoleKey.A:
                OnLeft?.Invoke();
                break;
            case ConsoleKey.D or ConsoleKey.RightArrow:
                OnRight?.Invoke();
                break;
            case ConsoleKey.W or ConsoleKey.UpArrow:
                OnUp?.Invoke();
                break;
            case ConsoleKey.S or ConsoleKey.DownArrow:
                OnDown?.Invoke();
                break;
            case ConsoleKey.Spacebar:
                OnJump?.Invoke();
                break;
            case ConsoleKey.E:
                OnAttack?.Invoke();
                break;
        }
    }
}