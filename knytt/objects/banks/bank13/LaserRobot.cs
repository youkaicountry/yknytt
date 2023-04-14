using Godot;

public partial class LaserRobot : GDKnyttBaseObject
{
    private bool isOn = false;
    private float speed = -100;

    public override void _PhysicsProcess(double delta)
    {
        var diff = new Vector2(speed * (float)delta, 0);
        Translate(diff);

        var new_pos = Center + diff;
        if (new_pos.X < GDArea.GlobalPosition.X) { collide(null, true); }
        if (new_pos.X > GDArea.GlobalPosition.X + GDKnyttArea.Width) { collide(null, false); }
    }

    private void collide(Node2D body, bool is_on)
    {
        speed = is_on ? 50 : -100;
        GetNode<CollisionShape2D>("LaserArea/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<CollisionShape2D>("LeftLaserChecker/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<CollisionShape2D>("RightLaserChecker/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play(is_on ? "on" : "off");
        GetNode<AudioStreamPlayer2D>(is_on ? "OnPlayer" : "OffPlayer").Play();
    }
}
