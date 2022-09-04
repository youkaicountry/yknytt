using Godot;

public class DropCrawler : Crawler
{
    private void _on_DistanceMod_EnterEvent()
    {
        GDArea.Selector.Register(this);
    }

    private void _on_DistanceMod_ExitEvent()
    {
        GDArea.Selector.Unregister(this);
    }

    private void _on_ShotTimer_timeout_ext()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        if (moveAndCollide(new Vector2(-5, 0), testOnly: true) == null) { GDArea.Bullets.Emit(this, -1); }
        GDArea.Bullets.Emit(this, 0);
        if (moveAndCollide(new Vector2(5, 0), testOnly: true) == null) { GDArea.Bullets.Emit(this, 1); }
    }
}
