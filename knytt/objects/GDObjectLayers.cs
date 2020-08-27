using System.Collections.Generic;
using Godot;

public class GDObjectLayers : Node2D
{
    public GDKnyttObjectLayer[] Layers { get; private set; }
    public GDKnyttArea GDArea { get; private set; }

    public void initLayers(GDKnyttArea area)
    {
        GDArea = area;
        var llayers = new List<GDKnyttObjectLayer>();

        foreach (GDKnyttObjectLayer c in this.GetChildren())
        {
            llayers.Add(c);
            c.initLayer(this);
        }

        this.Layers = llayers.ToArray();

        // TODO: Enable collisions only on layer 3
    }

    public void addObject(int layer, int x, int y, GDKnyttObjectBundle bundle)
    {
        this.Layers[layer].addObject(x, y, bundle);
    }
}
