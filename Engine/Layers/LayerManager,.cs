using MonochromeEngine.Engine.Render;
using MonochromeEngine.Engine.Utils;

namespace MonochromeEngine.Engine.Layers;

public class LayerManager
{
    private readonly MonoRenderer _renderer;
    private readonly Map _map;
    
    private char[,] _playerLayer;
    private char[,] _backgroundLayer;
    private char[,] _groundLayer;
    private char[,] _itemsLayer;
    private char[,] _enemiesLayer;
    private char[,] _uiLayer;

    public LayerManager(MonoRenderer renderer, Map map)
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
    
    public char[,] GetLayerFromType(RenderLayerType type)
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

    public List<char[,]> GetAllLayers()
    {
        return new List<char[,]>()
        {
            _backgroundLayer,
            _playerLayer,
            _groundLayer,
            _itemsLayer,
            _enemiesLayer,
            _uiLayer
        };
    }
}