using System.Collections.Generic;
using Godot;
using YKnyttLib;

public static class GDKnyttObjectFactory
{
    private static Dictionary<KnyttPoint, string> ObjectLookup;

    static GDKnyttObjectFactory()
    {
        ObjectLookup = new Dictionary<KnyttPoint, string>();
        ObjectLookup[new KnyttPoint(0, 3)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 4)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 5)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 6)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 7)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 8)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 9)]  = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 10)] = "PowerItem";
        ObjectLookup[new KnyttPoint(0, 17)] = "Sign";
        ObjectLookup[new KnyttPoint(0, 18)] = "Sign";
        ObjectLookup[new KnyttPoint(0, 19)] = "Sign";
        ObjectLookup[new KnyttPoint(1, 1)] = "LiquidPool";
        ObjectLookup[new KnyttPoint(7, 8)] = "Rain";
        ObjectLookup[new KnyttPoint(7, 9)] = "RaindropObject";
    }

    public static GDKnyttObjectBundle buildKnyttObject(KnyttPoint object_id)
    {
        if (!ObjectLookup.ContainsKey(object_id))
        {
            GD.Print(string.Format("Object {0}:{1} unimplemented.", object_id.x, object_id.y));
            return null;
        }
        
        string fname = string.Format("res://knytt/objects/banks/bank{0}/{1}.tscn", object_id.x, ObjectLookup[object_id]);
        var scene = ResourceLoader.Load<PackedScene>(fname);
        return new GDKnyttObjectBundle(object_id, scene);
    }
}

public class GDKnyttObjectBundle
    {
        public KnyttPoint object_id;
        PackedScene scene;
        public Texture icon;

        public GDKnyttObjectBundle(KnyttPoint object_id, PackedScene scene, Texture icon=null)
        {
            this.object_id = object_id;
            this.scene = scene;
            this.icon = icon;
        }
        
        public GDKnyttBaseObject getNode(GDKnyttObjectLayer layer, KnyttPoint coords)
        {
            var node = scene.Instance() as GDKnyttBaseObject;
            node.initialize(object_id, layer, coords);
            return node;
        }
    }