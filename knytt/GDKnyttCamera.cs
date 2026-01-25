using Godot;
using YUtil.Math;

public partial class GDKnyttCamera : Camera2D
{
    public GDKnyttGame Game { get; private set; }

    public Godot.Vector2 Target { get; private set; }

    public bool Scrolling { get; private set; }
    float speed = 0f;

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
    }

    // Give position in global space
    public void jumpTo(Godot.Vector2 position)
    {
        this.GlobalPosition = position;
        this.Scrolling = false;
    }

    public void scrollTo(Godot.Vector2 target, float speed)
    {
        this.speed = speed;
        this.Scrolling = true;
        this.Target = target;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (delta <= 0) { GD.Print("Invalid delta: ", delta); return; }

        if (this.Scrolling)
        {
            var gp = this.GlobalPosition;
            var s = speed * delta;
            double gpX = gp.X, gpY = gp.Y;
            bool reachedX = MathTools.MoveTowards(ref gpX, Target.X, s);
            bool reachedY = MathTools.MoveTowards(ref gpY, Target.Y, s);
            gp.X = (float)gpX;
            gp.Y = (float)gpY;
            this.Scrolling = !(reachedX & reachedY);

            this.GlobalPosition = gp;
        }
    }
}
