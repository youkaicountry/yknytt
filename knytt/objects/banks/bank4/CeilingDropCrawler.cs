using Godot;

public class CeilingDropCrawler : DropCrawler
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "FireBullet",
            (p, i) =>
            {
                p.Translate(new Vector2(12 + i * 9, 11 + Mathf.Abs(i) * 3));
                p.VelocityMMF2 = i == 0 ? 25 + random.Next(10) : 25 + random.Next(15);
                p.DirectionMMF2 = i == -1 ? 17 + random.Next(3) :
                                  i == 0 ? 23 + random.Next(3) :
                                            29 + random.Next(3);
                p.GravityMMF2 = 10;
                p.DecelerationMMF2 = 10;
                p.ApllyPinballCorrections();
            });
    }

    private void _on_shoot_event()
    {
        if (GetNode<Timer>("ShotTimer").IsStopped() && GetNode<DistanceMod>("DistanceMod").IsEntered)
        {
            shoot();
            GetNode<Timer>("ShotTimer").Start(1.6f);
        }
    }
}
