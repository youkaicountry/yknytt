using Godot;

public class SmallSpider : Muff
{
    private void _on_Area2D_body_entered(object body)
    {
        if (!(body is Juni juni)) { return; }
        juni.die();
        GetNode<Timer>("SpeedTimer").Stop();
        GetNode<Timer>("DirectionTimer").Stop();
        changeSpeed(0);
        sprite.Play("open");
    }
}
