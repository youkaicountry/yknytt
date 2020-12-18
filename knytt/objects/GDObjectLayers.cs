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

        if (GDArea.Area.getExtraData("Overlay") == "True") { turnOffObjects(Layers[3]); }
    }

    public void returnObjects()
    {
        foreach (KnyttPoint id in UsedAssets)
        {
            GDArea.GDWorld.AssetManager.returnObject(id);
        }
    }

    private void turnOffObjects(GDKnyttObjectLayer layer)
    {
        foreach (Node obj in layer.GetChildren())
        {
            foreach (Node child in obj.GetChildren())
            {
                switch (child)
                {
                    case Area2D area:        area.CollisionLayer = 0; area.CollisionMask = 0; break;
                    case PhysicsBody2D body: body.CollisionLayer = 0; body.CollisionMask = 0; break;
                }
            }
        }
    }

    public List<GDKnyttBaseObject> findObjects(KnyttPoint obj_id)
    {
        List<GDKnyttBaseObject> finds = new List<GDKnyttBaseObject>();
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (GDKnyttBaseObject knytt_object in layer.GetChildren())
            {
                if (obj_id.x == knytt_object.ObjectID.x && obj_id.y == knytt_object.ObjectID.y)
                {
                    finds.Add(knytt_object);
                }
            }
        }
        return finds;
    }

    public GDKnyttBaseObject findObject(KnyttPoint obj_id)
    {
        var finds = findObjects(obj_id);
        return finds.Count != 0 ? finds[0] : null;
    }
}
