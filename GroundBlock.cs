using MonochromeEngine.Engine;
using MonochromeEngine.Engine.BaseObjects;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Tick;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine;

public class GroundBlock: GameObject, IUpdatable, IRenderable
{
    public GroundBlock(string name, 
        Vector2 position, 
        char[,] layer, 
        Map map, 
        MonoRenderer renderer, 
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
