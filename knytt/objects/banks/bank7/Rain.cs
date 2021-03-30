using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class Rain : GDKnyttBaseObject
{
    PackedScene drop_scene;
    Queue<Raindrop> _drop_q;
    Raindrop add_next;
    int drop_count;

    const int MAX_DROPS = 5;

    public override void _Ready()
    {
        drop_scene = ResourceLoader.Load("res://knytt/objects/banks/bank7/Raindrop.tscn") as PackedScene;
        _drop_q = new Queue<Raindrop>();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (add_next != null && add_next.GetParent() == null && GetChildCount() < MAX_DROPS)
        {
            add_next.Position = new Vector2((float)random.NextDouble() * GDKnyttAssetManager.TILE_WIDTH, 12f);
            add_next.max_distance = (KnyttArea.AREA_HEIGHT - Coords.y) * GDKnyttAssetManager.TILE_HEIGHT;
            add_next.reset(this);
            CallDeferred("add_child", add_next);
            add_next = null;
        }

        if (add_next == null && ((float)random.NextDouble()) * .4f < delta)
        {
            add_next = nextRaindrop();
        }
    }

    private Raindrop nextRaindrop()
    {
        if (_drop_q.Count > 0) { return _drop_q.Dequeue(); }
        if (GetChildCount() >= MAX_DROPS) { return null; }
        var raindrop = drop_scene.Instance() as Raindrop;
        raindrop.Name = $"Raindrop{drop_count++}";
        return raindrop;
    }

    public void recycleRaindrop(Raindrop raindrop)
    {
        if (raindrop.GetParent() == this) { CallDeferred("remove_child", raindrop); }
        //RemoveChild(raindrop);
        raindrop.SetPhysicsProcess(false);
        raindrop.Visible = false;
        _drop_q.Enqueue(raindrop);
    }

    public override void _ExitTree()
    {
        while (_drop_q.Count > 0)
        {
            var node = _drop_q.Dequeue();
            node.QueueFree();
        }

        if (add_next != null) { add_next.QueueFree(); add_next = null; }
    }
}
