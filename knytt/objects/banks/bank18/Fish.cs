using System.Collections.Generic;
using Godot;
using YUtil.Math;

public class Fish : GDKnyttBaseObject
{
    struct FishData
    {
        public float speed;

        public FishData(float speed)
        {
            this.speed = speed;
        }
    }

    static Dictionary<int, FishData> ID2Data;

    static Fish()
    {
        ID2Data = new Dictionary<int, FishData>();
        ID2Data[1] = new FishData(30f);
        ID2Data[2] = new FishData(30f);
        ID2Data[3] = new FishData(30f);
        ID2Data[4] = new FishData(30f);
        ID2Data[5] = new FishData(30f);
    }

    FishData current_data;
    PathFollow2D path;
    bool forward = true;
    AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();
        var ff_scene = ResourceLoader.Load("res://knytt/objects/banks/bank18/FishFollower.tscn") as PackedScene;
        path = ff_scene.Instance() as PathFollow2D;
        GetNode($"Paths/{ObjectID.y}").AddChild(path);
        sprite = path.GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play($"{ObjectID.y}");
        current_data = ID2Data[ObjectID.y];
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        float ipo = path.UnitOffset;
        var ogp = path.GlobalPosition;
        path.Offset += current_data.speed * delta * (forward ? 1f : -1f);
        calcSpriteDirection(ogp);

        if (path.UnitOffset.AlmostEqualsWithAbsTolerance(forward ? 1f : 0f, .000001f))
        {
            forward = !forward;
        }
    }

    private void calcSpriteDirection(Godot.Vector2 old_pos)
    {
        var new_pos = path.GlobalPosition;
        //GD.Print(new_pos.x - old_pos.x);
        sprite.FlipH = (new_pos.x < old_pos.x);
    }
}
