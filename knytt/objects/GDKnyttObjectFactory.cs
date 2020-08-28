using Godot;
using YKnyttLib;

public static class GDKnyttObjectFactory
{
    public static GDKnyttObjectBundle buildKnyttObject(KnyttPoint object_id)
    {
        string fname = string.Format("res://knytt/objects/banks/bank{0}/Object{1}.tscn", object_id.x, object_id.y);
        var f = new File();
        if (!f.FileExists(fname)) 
        { 
            GD.Print(string.Format("Object {0}:{1} unimplemented.", object_id.x, object_id.y));
            return null;
        }
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