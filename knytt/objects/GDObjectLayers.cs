using Godot;
using System.Collections.Generic;
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
        for (int layer = KnyttArea.AREA_SPRITE_LAYERS - 1; layer >= 0; layer--)
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

        /*  With overlay:       Without overlay:
        1   Tiles 1             Tiles 1
        2                       Tiles 2
        3   Tiles 3             Tiles 3
        4   Objects 4 bullets   Objects 4 bullets
        5   Objects 4           Objects 4
        6   Objects 5 bullets   Objects 5 bullets
        7   Objects 5 + effect  Objects 5 + shift effect
        8   Juni                Juni
        9   Objects 6 bullets   Objects 6 bullets
        10  Objects 6           Objects 6
        11                      Objects 7 bullets
        12  Tiles 2             Objects 7
        13  Juni's attachment   Juni's attachment
        14  Objects 7 bullets
        15  Objects 7
        16  Tint                Tint
        17  Signs and titles    Signs and titles
        */
        if (GDArea.Area.getExtraData("Overlay")?.ToLower() == "true")
        {
            turnOffObjects(Layers[3]);
            Layers[3].ZIndex = 15;
        }
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
        foreach (GDKnyttBaseObject obj in layer.GetChildren())
        {
            obj.SafeBullets = true;
            foreach (Node child in obj.GetChildren())
            {
                switch (child)
                {
                    case CollisionShape2D shape: shape.SetDeferred("disabled", true); break;
                    case CollisionPolygon2D polygon: polygon.SetDeferred("disabled", true); break;
                    case Area2D area: area.CollisionLayer = 0; area.CollisionMask = 0; break;
                    case PhysicsBody2D body: body.CollisionLayer = 0; body.CollisionMask = 0; break;
                }
            }
        }
    }

    public List<GDKnyttBaseObject> findObjects(List<KnyttPoint> obj_ids)
    {
        List<GDKnyttBaseObject> finds = new List<GDKnyttBaseObject>();
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (GDKnyttBaseObject knytt_object in layer.GetChildren())
            {
                if (obj_ids.Contains(knytt_object.ObjectID) && !knytt_object.Deleted)
                {
                    finds.Add(knytt_object);
                }
            }
        }
        return finds;
    }

    public List<GDKnyttBaseObject> findObjects(KnyttPoint obj_id)
    {
        return findObjects(new List<KnyttPoint>() {obj_id});
    }

    public GDKnyttBaseObject findObject(KnyttPoint obj_id)
    {
        var finds = findObjects(obj_id);
        return finds.Count != 0 ? finds[0] : null;
    }

    public void checkCollectables(JuniValues powers)
    {
        if (powers == null) { return; }
        powers.resetMarks(GDArea.Area.MapPosition);

        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (GDKnyttBaseObject kbo in layer.GetChildren())
            {
                var knytt_object = kbo;
                if (knytt_object is CustomObject obj && obj.node != null && !obj.node.Deleted && !obj.node.IsInsideTree())
                {
                    knytt_object = obj.node;
                }
                if (knytt_object is PowerItem pitem && !powers.getPower(pitem.Power))
                {
                    var collectable = pitem.Power >= 8 && pitem.Power <= 11 ?
                        JuniValues.Collectable.Key + (int)(pitem.Power - JuniValues.PowerNames.RedKey + 1) :
                        JuniValues.Collectable.Powerup;
                    powers.setMark(GDArea.Area.MapPosition, collectable);
                }
                if (knytt_object is Door door && !door.checkKey(GDArea.GDWorld.Game.Juni))
                {
                    int color = knytt_object is KeyDoor keydoor ? keydoor.power - JuniValues.PowerNames.RedKey + 1 : 0;
                    powers.setMark(GDArea.Area.MapPosition, JuniValues.Collectable.Door + color);
                }
                if (knytt_object.ObjectID.x == 19 && !powers.getCollectable(knytt_object.ObjectID.y))
                {
                    powers.setMark(GDArea.Area.MapPosition, 
                        knytt_object.ObjectID.y <= 50 ? JuniValues.Collectable.Creature :
                        knytt_object.ObjectID.y <= 150 ? JuniValues.Collectable.Coin : JuniValues.Collectable.Artifact);
                }
            }
        }
    }
}
