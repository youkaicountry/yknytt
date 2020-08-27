using Godot;
using System;
using YKnyttLib;

public class Object9 : GDKnyttBaseObject
{
    PackedScene drop_scene;

    protected override void _impl_initialize()
    {
        drop_scene = ResourceLoader.Load("res://knytt/objects/banks/bank7/Raindrop.tscn") as PackedScene;

        var raindrop = drop_scene.Instance() as Raindrop;
        raindrop.max_distance = (KnyttArea.AREA_HEIGHT - Coords.y) * GDKnyttAssetManager.TILE_HEIGHT;
        AddChild(raindrop);
    }

    protected override void _impl_process(float delta)
    {
    }
}
