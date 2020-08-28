using System.Collections.Generic;
using Godot;
using YKnyttLib;

public class GDObjectLayers : Node2D
{
    public GDKnyttObjectLayer[] Layers { get; private set; }
    public GDKnyttArea GDArea { get; private set; }

    public List<KnyttPoint> UsedAssets { get; }

    public GDObjectLayers()
    {
        this.UsedAssets = new List<KnyttPoint>(64);
    }

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

        //Load objects
        for (int layer = 0; layer < KnyttArea.AREA_SPRITE_LAYERS; layer++)
        {
            var data = area.Area.ObjectLayers[layer];
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var oid = data.getObjectID(x, y);
                    if (oid.isZero()) { continue; }

                    var bundle = GDArea.GDWorld.AssetManager.GetObject(oid);
                    this.UsedAssets.Add(oid);
                    if (bundle == null) { continue; }
                    
                    this.Layers[layer].addObject(new KnyttPoint(x, y), bundle);
                }
            }
        }
    }

    public void returnObjects()
    {
        foreach (KnyttPoint id in UsedAssets)
        {
            GDArea.GDWorld.AssetManager.returnObject(id);
        }
    }
}
