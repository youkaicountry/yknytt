using Godot;

public class DropCrawler : Crawler
{
    protected void shoot()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        if (moveAndCollide(new Vector2(-5, 0), testOnly: true) == null) { GDArea.Bullets.Emit(this, -1); }
        GDArea.Bullets.Emit(this, 0);
        if (moveAndCollide(new Vector2(5, 0), testOnly: true) == null) { GDArea.Bullets.Emit(this, 1); }
    }
}
