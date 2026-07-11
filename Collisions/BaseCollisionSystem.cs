using MonochromeEngine.Engine;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Utils;

namespace MonochromeEngine.Collisions;

public class BaseCollisionSystem
{
    private readonly Map _map;
    private readonly Collider2D _collider;
    private readonly MonoRenderer _renderer;
    private readonly char[,] _layer;
    private readonly int _xOffset;
    private readonly int _yOffset;
    // Ray
    private Ray _downGroundRay;
    private Ray _upGroundRay;
    private Ray _rightOrLeftGroundRay;
    private bool _isGroundUp;
    private bool _isGroundRightOrLeftSide;
    private bool _isGroundDown;
    public bool IsGroundDown => _isGroundDown;
    public bool IsGroundRightOrLeftSide => _isGroundRightOrLeftSide;
    public bool IsGroundUp => _isGroundUp;
    

    public BaseCollisionSystem(Map map, Collider2D collider, MonoRenderer renderer, char[,] layer, Vector2 position, int xOffset, int yOffset)
    {
        _map = map;
        _collider = collider;
        _renderer = renderer;
        _xOffset = xOffset;
        _yOffset = yOffset;
        
        _downGroundRay = new Ray(
            position + xOffset / 2, 
            Vector2.Down, 
            yOffset); // OK
        
        _upGroundRay = new Ray(position, Vector2.Up, 1);
        _rightOrLeftGroundRay 
            = new Ray(
                new Vector2(position.X + xOffset / 2, position.Y), 
                Vector2.Right, 
                yOffset / 2); // OK
    }
    
    public void CheckGroundAndWalls(ref Vector2 position, ref Vector2 velocity)
    {
        // Сначала синхронизируем колайдер с текущей позицией из физики
        // проверка через колайдер
        _isGroundRightOrLeftSide = false;

        foreach (var block in _map.Blocks)
        {
            if (_collider.IsColliding(block.Collider))
            {
                double playerCenterX = position.X + (_xOffset / 2f);
                double blockCenterX = block.Collider.Position.X + (block.Collider.Size.X / 2f);
                
                // Персонаж считается пересекающим стену, только если его ноги зашли в блок 
                // МЕНЬШЕ, чем на определенный порог (например, на 3-4 пикселя).
                // Если он провалился глубже — это уже приземление сверху, а не удар об стену!
                int skinWidthY = 4;
                bool isOverlappingVertically =
                    (position.Y < block.Collider.Position.Y + block.Collider.Size.Y) &&
                    (position.Y + _yOffset - skinWidthY > block.Collider.Position.Y);

                if (isOverlappingVertically)
                {
                    // Уперся в левую сторону стены (идешь вправо)
                    if (playerCenterX < blockCenterX && velocity.X > 0)
                    {
                        _isGroundRightOrLeftSide = true;
                        velocity.X = 0;
                        // Выталкиваем влево
                        position.X = block.Collider.Position.X - _xOffset; 
                        break;
                    }
                    // Уперся в правую сторону стены (идешь влево)
                    else if (playerCenterX > blockCenterX && velocity.X < 0)
                    {
                        _isGroundRightOrLeftSide = true;
                        velocity.X = 0;
                        // Выталкиваем вправо
                        position.X = block.Collider.Position.X + block.Collider.Size.X; 
                        break;
                    }
                }
            }
        }

        // проверка пола через луч
        _upGroundRay.UpdatePosition(position);
        _downGroundRay.UpdatePosition(position);
        _isGroundUp = _map.Blocks.Any(s => _upGroundRay.CheckDetection(s.Collider));

        var groundBlock = _map.Blocks.FirstOrDefault(s => _downGroundRay.CheckDetection(s.Collider));
        if (groundBlock != null)
        {
            _isGroundDown = true;
            velocity.Y = 0;
            // Выталкиваем строго вверх
            position.Y = groundBlock.Collider.Position.Y - _yOffset; 
        }
        else
        {
            _isGroundDown = false;
        }
    }
    
    public void DrawGizmos(int offset = 16)
    {
        char isRightSymbol = _isGroundRightOrLeftSide ? '+' : '^';
        _renderer.DrawLine(_layer, (int)_rightOrLeftGroundRay.Position.X,
            (int)_rightOrLeftGroundRay.Position.Y,
            (int)_rightOrLeftGroundRay.Position.X + (offset / 2),
            (int)_rightOrLeftGroundRay.Position.Y, isRightSymbol);

        char isGroundSymbol = _isGroundDown ? '+' : '-';
        _renderer.DrawLine(_layer, (int)_downGroundRay.Position.X + _xOffset / 2,
            (int)_downGroundRay.Position.Y,
            (int)_downGroundRay.Position.X + _xOffset / 2,
            (int)_downGroundRay.Position.Y + _yOffset, isGroundSymbol);
    }
}