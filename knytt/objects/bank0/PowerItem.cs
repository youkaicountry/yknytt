using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class PowerItem : GDKnyttBaseObject
{
    public static readonly Dictionary<int, int> Object2Power = new Dictionary<int, int>()
    {
        [3] = 0, [4] = 1, [5] = 2, [6] = 3, [7] = 4, [8] = 5, [9] = 6, [10] = 7,
        [21] = 8, [22] = 9, [23] = 10, [24] = 11, [35] = 12
    };

    public int Power { get; private set; }

    public override void _Ready()
    {
        this.Power = Object2Power[ObjectID.y];
        // Check if Juni has the powerup, hide if it is so.
        if (Juni.Powers.getPower(Power)) { QueueFree(); Deleted = true; }
        var anim = GetNode<AnimatedSprite>("AnimatedSprite");
        string custom_anim = $"{GDArea.GDWorld.KWorld.WorldDirectoryName} {Power}";
        anim.Animation = anim.Frames.HasAnimation(custom_anim) ? custom_anim : $"Power{Power}";
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.setPower(Power, true);
        GDArea.playEffect(Coords);
        juni.playSound("powerup");
        checkDoors(juni);
        QueueFree();
        Deleted = true;
    }

    private void checkDoors(Juni juni)
    {
        if (Power >= 8 && Power <= 11)
        {
            var doors = juni.Game.CurrentArea.Objects.findObjects(new KnyttPoint(15, Power - 8 + 27));
            foreach (Door door in doors)
            {
                door.gotKey();
            }
        }
    }
}
