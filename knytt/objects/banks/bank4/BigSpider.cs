using Godot;

public class BigSpider : Spider
{
    private const int RADAR_BOTTOM = 8;
    private const int RADAR_TOP = -1;

    public override void _Ready()
    {
        base._Ready();
        direction = Juni.GlobalPosition.x < GlobalPosition.x ? -1 : 1;
        sprite.FlipH = direction == -1;
        if (GlobalPosition.x + centerOffset + border > GDArea.GlobalPosition.x + GDKnyttArea.Width)
        {
            GlobalPosition = new Vector2(GDArea.GlobalPosition.x + GDKnyttArea.Width - centerOffset - border, GlobalPosition.y);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var jgp = Juni.ApparentPosition;
        float juni_radar_value = jgp.y;
        float obj_radar_value = Center.y;
        bool out_of_radar = juni_radar_value < obj_radar_value - RADAR_TOP || juni_radar_value > obj_radar_value + RADAR_BOTTOM;

        if (!out_of_radar) { tryRun(); }
        base._PhysicsProcess(delta);
    }
}
