using Godot;

public class WaterMonsterNew : BaseWaterMonsterNew
{
    protected override void registerEmitter()
    {
        GDArea.Bullets.RegisterEmitter(this, "HomingBullet", 15,
            (p, i) => 
            {
                (p as HomingBullet).globalJuni = Juni;
                p.DisapperarPlayer = GetNode<RawAudioPlayer2D>("HitPlayer");
                p.Translate(new Vector2(12, 8));
                p.VelocityX = GDKnyttDataStore.random.Next(-50, 50);
                p.VelocityY = GDKnyttDataStore.random.Next(-150, -50);
            });
    }
}
