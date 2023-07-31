using Godot;

public class TrailContainer : Node2D
{
    [Export] public int TrailCount {get; set;} = 100;
    [Export] public int TrailFrames {get; set;} = 5;

    private bool _on = false;
    public bool On {
        get { return _on; }
        set
        {
            if (_on)
            {
                if (value) { return; }
            }
            else
            {
                if (value) { reset(); }
            }

            _on = value;
            Visible = value;
        }
    }

    PackedScene trail_scene;

    private int step;
    private int replace;

    public GDKnyttGame Game { get; private set; }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
        this.reset();
    }

    public void reset()
    {
        step = 0;
        replace = 0;
        for (int i = 0; i < GetChildCount(); i++)
        {
           var child = GetChild(i);
           child.QueueFree(); 
        }
    }

    public override void _Ready()
    {
        trail_scene = ResourceLoader.Load("res://knytt/juni/trail/JuniTrail.tscn") as PackedScene;
        Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (step++ % TrailFrames == 0)
        {
            Sprite s;
            var children = GetChildCount();
            if (children >= TrailCount)
            {
                if (replace >= children) { replace = 0; }
                s = GetChild<Sprite>(replace++);
            }
            else
            {
                s = trail_scene.Instance<Sprite>();
                AddChild(s);
            }
            
            positionTrail(s, Game.Juni);
            
        }
    }

    private void positionTrail(Sprite s, Juni juni)
    {
        s.Position = juni.Position;
        s.Frame = juni.Sprite.Frame;
        s.FlipH = juni.Sprite.FlipH;
    }
}
