using Godot;

public class LaserRobot : GDKnyttBaseObject
{
    private bool isOn = false;
    private float speed = -100;

    public override void _PhysicsProcess(float delta)
    {
        var diff = new Vector2(speed * delta, 0);
        Translate(diff);

        var new_pos = Center + diff;
        if (new_pos.x < GDArea.GlobalPosition.x) { collide(null, true); }
        if (new_pos.x > GDArea.GlobalPosition.x + GDKnyttArea.Width) { collide(null, false); }
    }

    private void collide(object body, bool is_on)
    {
        speed = is_on ? 50 : -100;
        GetNode<CollisionShape2D>("LaserArea/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<CollisionShape2D>("LeftLaserChecker/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<CollisionShape2D>("RightLaserChecker/CollisionShape2D").SetDeferred("disabled", !is_on);
        GetNode<AnimatedSprite>("AnimatedSprite").Play(is_on ? "on" : "off");
        GetNode<RawAudioPlayer2D>(is_on ? "OnPlayer" : "OffPlayer").Play();
    }
}
