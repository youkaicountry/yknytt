using Godot;
using System;

public class GDKnyttObjectLayer : Node2D
{
    public GDObjectLayers ObjectLayers { get; private set; }

    public void initLayer(GDObjectLayers layers)
    {
        ObjectLayers = layers;
    }

    // TODO: Add icons to a layer
    public void addObject(int x, int y, GDKnyttObjectBundle bundle)
    {
        var node = bundle.getNode(this);
        node.Position = new Vector2(x*GDKnyttAssetManager.TILE_WIDTH, y*GDKnyttAssetManager.TILE_HEIGHT);
        this.AddChild(node);
    }
}
