using Godot;
using System;

public class Crawler : GDKnyttBaseObject
{
    [Export] bool horizontal = true;
    [Export] float speed = 50;
    [Export] int keepDistance = 4;
    [Export] int radarBottom = 0;
    [Export] int radarTop = 0;

    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        var jgp = Juni.GlobalPosition;
        float juni_radar_value = horizontal ? jgp.y : jgp.x;
        float obj_radar_value = horizontal ? Center.y : Center.x;
        bool out_of_radar = (radarTop != 0 && juni_radar_value < obj_radar_value - radarTop) ||
                            (radarBottom != 0 && juni_radar_value > obj_radar_value + radarBottom);

        float juni_coord_value = horizontal ? jgp.x : jgp.y;
        float obj_coord_value = horizontal ? Center.x : Center.y;
        bool too_close = Mathf.Abs(juni_coord_value - obj_coord_value) < keepDistance;
        bool stop = out_of_radar || too_close;

        if (!stop)
        {
            int direction = juni_coord_value < obj_coord_value ? -1 : 1;
            float diff = speed * direction * delta;
            var diff_vec = horizontal ? new Vector2(diff, 0) : new Vector2(0, diff);
            if (moveAndCollide(diff_vec) != null || !GDArea.isIn(Center + diff_vec))
            {
                stop = true;
            }
            else
            {
                if (sprite.Animation == "default")
                {
                    sprite.Play("walk");
                    if (horizontal) { sprite.FlipH = direction < 0; } else { sprite.FlipV = direction > 0; }
                }
            }
        }

        if (stop)
        {
            if (sprite.Animation == "walk") { sprite.Play("default"); }
        }
    }
}
