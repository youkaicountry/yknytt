using Godot;

public class WaterMonster : BaseWaterMonster
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet",
            (p, i) =>
            {
                (p as HomingBullet).globalJuni = Juni;
                p.Translate(new Vector2(12, 8));
                p.VelocityX = random.Next(-50, 50);
                p.VelocityY = random.Next(-150, -50);
            });
    }
}
