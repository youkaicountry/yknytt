using Godot;
using System;

public class Crawler : GDKnyttBaseObject
{
    [Export] protected bool horizontal = true;
    [Export] protected float speed = 50;
    [Export] protected int keepDistance = 4;
    [Export] protected int radarBottom = 0;
    [Export] protected int radarTop = 0;

    public override void _PhysicsProcess(float delta)
    {
        var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        var jgp = Juni.ApparentPosition;

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
            if (!GDArea.isIn(Center + diff_vec, x_border: 10) || moveAndCollide(diff_vec) != null)
            {
                stop = true;
            }
            else
            {
                if (sprite.Animation != "walk")
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
