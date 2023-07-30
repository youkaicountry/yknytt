using Godot;

public class JuniTrail : Sprite
{
    [Export] public int TrailCount {get; set;} = 120;
    [Export] public int TrailFrames {get; set;} = 2;

    PackedScene trail_scene;

    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        
    }
}
