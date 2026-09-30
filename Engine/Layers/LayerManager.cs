using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.Layers;

public class LayerManager
{
    private readonly IRenderer _renderer;
    private readonly Map _map;
    
    private Cell[,] _playerLayer;
    private Cell[,] _backgroundLayer;
    private Cell[,] _groundLayer;
    private Cell[,] _itemsLayer;
    private Cell[,] _enemiesLayer;
    private Cell[,] _uiLayer;

    public LayerManager(IRenderer renderer, Map map)
    {
        _renderer = renderer;
        _map = map;
        
        _playerLayer = _renderer.CreateLayer(map.Width, map.Height);
        _backgroundLayer = _renderer.CreateLayer(map.Width, map.Height);
        _groundLayer = _renderer.CreateLayer(map.Width, map.Height);
        _itemsLayer = _renderer.CreateLayer(map.Width, map.Height);
        _enemiesLayer = _renderer.CreateLayer(map.Width, map.Height);
        _uiLayer = _renderer.CreateLayer(map.Width, map.Height);
    }
    
    public Cell[,] GetLayerFromType(RenderLayerType type)
    {
        switch (type)
        {
            case RenderLayerType.Player:
                return _playerLayer;
            case RenderLayerType.Enemies:
                return _enemiesLayer;
            case RenderLayerType.Background:
                return _backgroundLayer;
            case RenderLayerType.Ground:
                return _groundLayer;
            case RenderLayerType.Items:
                return _itemsLayer;
            case RenderLayerType.UI:
                return _uiLayer;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public List<Cell[,]> GetAllLayers()
    {
        return new List<Cell[,]>()
        {
            _backgroundLayer,
            _groundLayer,
            _playerLayer,
            _itemsLayer,
            _enemiesLayer,
            _uiLayer
        };
    }
}