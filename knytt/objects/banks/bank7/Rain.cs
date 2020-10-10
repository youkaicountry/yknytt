using Godot;
using YKnyttLib;

public class Rain : GDKnyttBaseObject
{   
    PackedScene drop_scene;

    protected override void _impl_initialize()
    {
        drop_scene = ResourceLoader.Load("res://knytt/objects/banks/bank7/Raindrop.tscn") as PackedScene;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (((float)GDKnyttDataStore.random.NextDouble())*.4f < delta)
        {
            var raindrop = drop_scene.Instance() as Raindrop;
            raindrop.Position = new Vector2((float)GDKnyttDataStore.random.NextDouble()*GDKnyttAssetManager.TILE_WIDTH, 12f);
            raindrop.max_distance = (KnyttArea.AREA_HEIGHT - Coords.y) * GDKnyttAssetManager.TILE_HEIGHT;
            AddChild(raindrop);
        }
    }

    protected override void _impl_process(float delta)
    {
        
    }
}
