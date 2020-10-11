using Godot;
using YKnyttLib;

public class RaindropObject : GDKnyttBaseObject
{
    PackedScene drop_scene;

    public override void _Ready()
    {
        drop_scene = ResourceLoader.Load("res://knytt/objects/banks/bank7/Raindrop.tscn") as PackedScene;

        var raindrop = drop_scene.Instance() as Raindrop;
        raindrop.max_distance = (KnyttArea.AREA_HEIGHT - Coords.y) * GDKnyttAssetManager.TILE_HEIGHT;
        AddChild(raindrop);
    }
}
