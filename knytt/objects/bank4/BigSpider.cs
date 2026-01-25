using Godot;

public partial class BigSpider : Spider
{
    private const int RADAR_BOTTOM = 8;
    private const int RADAR_TOP = -1;

    public override void _Ready()
    {
        base._Ready();
        direction = Juni.GlobalPosition.X < GlobalPosition.X ? -1 : 1;
        sprite.FlipH = direction == -1;
        if (GlobalPosition.X + centerOffset + border > GDArea.GlobalPosition.X + GDKnyttArea.Width)
        {
            GlobalPosition = new Vector2(GDArea.GlobalPosition.X + GDKnyttArea.Width - centerOffset - border, GlobalPosition.Y);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var jgp = Juni.ApparentPosition;
        float juni_radar_value = jgp.Y;
        float obj_radar_value = Center.Y;
        bool out_of_radar = juni_radar_value < obj_radar_value - RADAR_TOP || juni_radar_value > obj_radar_value + RADAR_BOTTOM;

        if (!out_of_radar) { tryRun(); }
        base._PhysicsProcess(delta);
    }
}
