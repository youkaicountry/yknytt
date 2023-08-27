using Godot;
using System.Collections.Generic;

public class PowerItem : GDKnyttBaseObject
{
    public readonly Dictionary<int, int> Object2Power = new Dictionary<int, int>()
    {
        [3] = 0, [4] = 1, [5] = 2, [6] = 3, [7] = 4, [8] = 5, [9] = 6, [10] = 7,
        [21] = 8, [22] = 9, [23] = 10, [24] = 11, [35] = 12
    };

    int power;

    public override void _Ready()
    {
        this.power = Object2Power[ObjectID.y];
        // Check if Juni has the powerup, hide if it is so.
        if (Juni.Powers.getPower(power)) { QueueFree(); }
        var anim = GetNode<AnimatedSprite>("AnimatedSprite");
        string custom_anim = $"{GDArea.GDWorld.KWorld.WorldDirectoryName} {power}";
        anim.Animation = anim.Frames.HasAnimation(custom_anim) ? custom_anim : $"Power{power}";
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.setPower(power, true);
        GDArea.playEffect(Coords);
        juni.playSound("powerup");
        QueueFree();
    }
}
