using Godot;

public partial class FloorDropCrawler : DropCrawler
{
    public override void _Ready()
    {
        base._Ready();
        // TODO: play "Fire Hit" in FireBullet, not "Fire Hit Lite": for MultiShots and DropCrawlers
        GDArea.Bullets.RegisterEmitter(this, "FireBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(12 + i * 9, 12 - Mathf.Abs(i) * 2));
                p.VelocityMMF2 = i == 0 ? 45 + random.Next(20) : 25 + random.Next(15);
                p.DirectionMMF2 = i == -1 ? 11 + random.Next(3) :
                                  i == 0 ? 7 + random.Next(3) :
                                            3 + random.Next(3);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }
}
