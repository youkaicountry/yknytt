using Godot;

public class SmileBall : Crawler
{
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        bool too_close = Mathf.Abs(Juni.GlobalPosition.x - Center.x) < keepDistance;
        if (too_close && sprite.Animation != "smile") { sprite.Play("smile"); }
        if (!too_close && sprite.Animation == "smile") { sprite.Play("default"); }
        sprite.FlipH = Juni.GlobalPosition.x < Center.x;
    }
}
