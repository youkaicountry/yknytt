using Godot;

public class SmallSpider : Muff
{
    private void _on_Area2D_body_entered(object body)
    {
        if (!(body is Juni juni)) { return; }
        juniDie(juni);
        GetNode<Timer>("SpeedTimer").Stop();
        GetNode<Timer>("DirectionTimer").Stop();
        changeSpeed(0);
        sprite.Play("open");
    }
}
