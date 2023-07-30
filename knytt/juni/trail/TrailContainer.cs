using Godot;

public class TrailContainer : Node2D
{
    [Export] public int TrailCount {get; set;} = 100;
    [Export] public int TrailFrames {get; set;} = 5;

    PackedScene trail_scene;

    private int step;

    public GDKnyttGame Game { get; private set; }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
        this.reset();
    }

    public void reset()
    {
        step = 0;
        for (int i = 0; i < GetChildCount(); i++)
        {
           var child = GetChild(i);
           child.QueueFree(); 
        }
    }

    public override void _Ready()
    {
        trail_scene = ResourceLoader.Load("res://knytt/juni/trail/JuniTrail.tscn") as PackedScene;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (step++ % TrailFrames == 0)
        {
            var s = trail_scene.Instance<Sprite>();
            positionTrail(s, Game.Juni);
            AddChild(s);
        }
    }

    private void positionTrail(Sprite s, Juni juni)
    {
        s.Position = juni.Position;
        s.Frame = juni.Sprite.Frame;
        s.FlipH = juni.Sprite.FlipH;
    }
}
