using MonochromeEngine.Engine.Anim;
using MonochromeEngine.Engine.Collider;
using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.BaseObjects;

public class GameObject
{
    private string _name;
    public BoxCollider2D Collider { get; protected set; }
    protected BaseAnimator Animator;
    protected CollisionSystem CollisionSystem;
    protected Map Map;
    protected Cell[,] Layer;
    private IRenderer _renderer;
    private SpritesLoaderSystem _spritesLoaderSystem;
    
    // Movement
    protected Vector2 Position;
    protected bool IsFacingRight = true;
    protected readonly int XOffset = 32;
    protected readonly int YOffset = 16;

    public GameObject(string name, Vector2 position, Cell[,] layer, Map map, IRenderer renderer,
        SpritesLoaderSystem spritesLoaderSystem)
    {
        _name = name;
        Position = position;
        Layer = layer;
        Map = map;
        _renderer = renderer;
        _spritesLoaderSystem = spritesLoaderSystem;
        Collider = new BoxCollider2D(Position, new Vector2(XOffset, YOffset));
        CollisionSystem = new CollisionSystem(Map, Collider);
        Animator = new BaseAnimator(_spritesLoaderSystem, _renderer, _name, Layer);
    }
}