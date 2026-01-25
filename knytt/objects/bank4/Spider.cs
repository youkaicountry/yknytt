using Godot;

public partial class Spider : GDKnyttBaseObject
{
    [Export] protected float speed = 50;
    [Export] protected float centerOffset = 12;
    [Export] protected float border = 12;

    protected AnimatedSprite2D sprite;

    protected int direction = 1;
    protected bool isRunning;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!isRunning) { return; }
        float diff = speed * direction * (float)delta;
        var diff_vec = new Vector2(diff, 0);
        var center = GlobalPosition + new Vector2(centerOffset, 0);
        if (!GDArea.isIn(center + diff_vec, x_border: border) || moveAndCollide(diff_vec) != null)
        {
            isRunning = false;
            direction = -direction;
            sprite.FlipH = !sprite.FlipH;
            sprite.Play("default");
        }
    }

    protected void tryRun()
    {
        if (!isRunning)
        {
            isRunning = true;
            sprite.Play("walk");
            GetNode<AudioStreamPlayer2D>("RunPlayer").Play();
        }
    }
}
