using Godot;

public class LaserRobot : GDKnyttBaseObject
{
    private bool isOn = false;
    private float speed = -100;

    public override void _PhysicsProcess(float delta)
    {
        var diff = new Vector2(speed * delta, 0);
        Translate(diff);
        if (!GDArea.isIn(Center + diff)) { collide(); }
    }

    private void _on_Area2D_body_entered(object body)
    {
        if (body is Juni juni) { juniDie(juni); } else { collide(); }
    }

    private void collide()
    {
        isOn = !isOn;
        speed = isOn ? 50 : -100;
        GetNode<CollisionShape2D>("LaserArea2D/CollisionShape2D").SetDeferred("disabled", !isOn);
        GetNode<AnimatedSprite>("AnimatedSprite").Play(isOn ? "on" : "off");
        GetNode<RawAudioPlayer2D>(isOn ? "OnPlayer" : "OffPlayer").Play();
    }
}
