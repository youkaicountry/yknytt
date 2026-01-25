using Godot;

public partial class SmileBall : Crawler
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        bool too_close = Mathf.Abs(Juni.GlobalPosition.X - Center.X) < keepDistance;
        if (too_close && sprite.Animation != "smile") { sprite.Play("smile"); }
        if (!too_close && sprite.Animation == "smile") { sprite.Play("default"); }
        sprite.FlipH = Juni.GlobalPosition.X < Center.X;
    }
}
