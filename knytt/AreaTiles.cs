using System.Collections.Generic;
using Godot;

public class AreaTiles : Node2D
{
    public AreaLayer[] Layers { get; private set; }

    public void initTiles(TileSet A, TileSet B)
    {
        var llayers = new List<AreaLayer>();

        foreach (AreaLayer c in this.GetChildren())
        {
            llayers.Add(c);
            c.initLayer(A, B);
        }

        this.Layers = llayers.ToArray();

        // TODO: Enable collisions only on layer 3
    }

    public void setTile(int layer, int x, int y, int tilenum)
    {
        this.Layers[layer].setTile(x, y, tilenum);
    }
}
