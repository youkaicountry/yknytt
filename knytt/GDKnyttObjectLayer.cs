using Godot;
using YKnyttLib;

public class GDKnyttObjectLayer : Node2D
{
    public GDObjectLayers ObjectLayers { get; private set; }

    public void initLayer(GDObjectLayers layers)
    {
        ObjectLayers = layers;
    }

    // TODO: Add icons to a layer
    public void addObject(KnyttPoint coords, GDKnyttObjectBundle bundle)
    {
        var node = bundle.getNode(this, coords);
        node.Position = new Vector2(coords.x*GDKnyttAssetManager.TILE_WIDTH, coords.y*GDKnyttAssetManager.TILE_HEIGHT);
        this.AddChild(node);
    }
}
