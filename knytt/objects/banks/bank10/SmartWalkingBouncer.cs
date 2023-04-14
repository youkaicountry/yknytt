using Godot;

public partial class SmartWalkingBouncer : WalkingBouncer
{
    public override void _Ready()
    {
        base._Ready();
        Juni.Jumped += juniJumped;
    }

    private void _on_tree_exiting()
    {
        Juni.Jumped -= juniJumped;
    }

    public void juniJumped(Juni juni)
    {
        if (!inAir && juni.Hologram == null && Mathf.Abs(juni.ApparentPosition.X - Center.X) < 150 + random.Next(80))
        {
            jump();
        }
    }
}
