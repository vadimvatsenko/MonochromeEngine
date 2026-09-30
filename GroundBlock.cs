using MonochromeEngine.Engine;
using MonochromeEngine.Engine.BaseObjects;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine;

public class GroundBlock: GameObject, IUpdatable, IRenderable
{
    // Для склона: высота поверхности от верха блока (в строках) в каждом столбике пикселей.
    // null — обычный квадратный блок
    public int[]? SlopeHeights { get; set; }

    public GroundBlock(string name, 
        Vector2 position, 
        Cell[,] layer, 
        Map map, 
        IRenderer renderer, 
        SpritesLoaderSystem spritesLoaderSystem,
        int spriteIndex
        ) : base(name, position, layer, map, renderer, spritesLoaderSystem)
    {
        Animator.SetIsStatic(true);
        Animator.TargetAnimation = Animator.IdleBase;
        Animator.SetFrameIndex(spriteIndex);
    }

    public void Update(double deltatime) => Animator.UpdateAnimation(deltatime);
    
    public void Draw() => Animator.Draw(Position, IsFacingRight);
}
