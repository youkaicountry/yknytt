using Godot;

public partial class SmartWalkingBouncer : WalkingBouncer
{
    public override void _Ready()
    {
        base._Ready();
        Juni.Connect(nameof(Juni.Jumped), new Callable(this, nameof(juniJumped)));
    }

    public void juniJumped(Juni juni, bool real)
    {
        if (!inAir && real && Mathf.Abs(juni.ApparentPosition.X - Center.X) < 150 + random.Next(80))
        {
            jump();
        }
    }
}
