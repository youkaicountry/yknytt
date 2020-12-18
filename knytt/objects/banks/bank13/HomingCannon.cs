using Godot;

public class HomingCannon : Cannon
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "DropStuff", 
            (p, i) =>
            {
                p.Translate(new Vector2(8f, 8f));
                p.DirectionMMF2 = 12;
                p.VelocityMMF2 = Mathf.Max(0, Center.x - Juni.ApparentPosition.x) / 8f + 13f + random.Next(6);
                p.GravityMMF2 = 10 + random.Next(5);
            });
    }
}
