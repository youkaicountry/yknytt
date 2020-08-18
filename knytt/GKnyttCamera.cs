using Godot;
using YUtil.Math;

public class GKnyttCamera : Camera2D
{
    public GKnyttGame Game { get; private set; }
    
    public Godot.Vector2 Target { get; private set; }

    public bool Scrolling { get; private set; }
    float speed = 0f;

    public void initialize(GKnyttGame game)
    {
        this.Game = game;
    }

    // Give position in global space
    public void jumpTo(Godot.Vector2 position)
    {
        this.GlobalPosition = position;
    }

    public void scrollTo(Godot.Vector2 target, float speed)
    {
        this.speed = speed;
        this.Scrolling = true;
        this.Target = target;
    }

    public override void _Process(float delta)
    {
        if (this.Scrolling)
        {
            var gp = this.GlobalPosition;
            var s = speed * delta;
            this.Scrolling = !(MathTools.MoveTowards(ref gp.x, Target.x, s)&MathTools.MoveTowards(ref gp.y, Target.y, s));
            
            this.GlobalPosition = gp;
        }
    }
}
