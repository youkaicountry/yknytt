using Godot;

public class Spider : GDKnyttBaseObject
{
    [Export] protected float speed = 50;
    [Export] protected float centerOffset = 12;
    [Export] protected float border = 12;

    protected AnimatedSprite sprite;

    protected int direction = 1;
    protected bool isRunning;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!isRunning) { return; }
        float diff = speed * direction * delta;
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
            GetNode<RawAudioPlayer2D>("RunPlayer").Play();
        }
    }
}
