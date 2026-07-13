using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Levels;
using MonochromeEngine.Scene;

public class GameplayScene : IScene
{
    private readonly Input _input;
    private int _currentLevelIndex;
    private List<IUpdatable> _updatables;
    // Твои карты, рендереры, слои...

    public GameplayScene(Input input, int levelIndex)
    {
        _input = input;
        _currentLevelIndex = levelIndex;
    }

    public void Initialize()
    {
        _updatables = new List<IUpdatable>();
        
        // Загружаем карту в зависимости от индекса уровня
        if (_currentLevelIndex == 1) 
            LoadLevel(LevelModel.Map11); // 
        else if (_currentLevelIndex == 2)
        {
            //LoadLevel(LevelModel.Map12);
        }
            
// Спавним игрока, мобов и добавляем их в _updatables [cite: 37, 108]
    }

    public void Update(double deltaTime)
    {
        // Обновляем все игровые объекты сцены [cite: 47]
        foreach (var updatable in _updatables)
        {
            updatable.Update(deltaTime);
        }

        // ТРИГГЕР ПОБЕДЫ: если игрок дошел до двери/выхода
        if (PlayerReachedExit())
        {
            // Переключаем на этот же GameplayScene, но передаем следующий индекс уровня!
            SceneManager.ChangeScene(new GameplayScene(_input, _currentLevelIndex + 1));
        }

        // ТРИГГЕР СМЕРТИ: если упал в пропасть или умер [cite: 174]
        if (PlayerIsDead())
        {
            SceneManager.ChangeScene(new GameOverScene(_input));
        }
    }

    public void Draw()
    {
        // Отрисовка игрового кадра (всё то, что у тебя сейчас в цикле Update) 
    }
    
    private void LoadLevel(int[,] mapData) { /* твой генератор блоков */ }
    private bool PlayerReachedExit() => false;
    private bool PlayerIsDead() => false;
}