using Godot;
using System;

public class WhiteExplosion : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "ShockWave",
            (p, i) =>
            {
                p.Translate(new Vector2(12, 12));
                p.VelocityMMF2 = 50 + random.Next(40);
                p.DirectionMMF2 = i % 32;
                p.DecelerationMMF2 = 12; // 20 in original
                p.DisappearWhenStopped = true;
            });

        for (int i = 0; i < 32 * 3; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
    }
}
