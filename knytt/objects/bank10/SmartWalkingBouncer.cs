using Godot;

public class SmartWalkingBouncer : WalkingBouncer
{
    public override void _Ready()
    {
        base._Ready();
        Juni.Connect(nameof(Juni.Jumped), this, nameof(juniJumped));
    }

    public void juniJumped(Juni juni)
    {
        if (!inAir && juni.Hologram == null && Mathf.Abs(juni.ApparentPosition.x - Center.x) < 150 + random.Next(80))
        {
            jump();
        }
    }
}
