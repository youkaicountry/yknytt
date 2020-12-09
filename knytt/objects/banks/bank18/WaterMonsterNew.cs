using Godot;

public class WaterMonsterNew : BaseWaterMonsterNew
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet", 15,
            (p, i) => 
            {
                (p as HomingBullet).globalJuni = Juni;
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(12, 8));
                p.VelocityX = random.Next(-50, 50);
                p.VelocityY = random.Next(-150, -50);
            });
    }
}
