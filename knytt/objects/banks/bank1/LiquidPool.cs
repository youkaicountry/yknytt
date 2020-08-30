using Godot;

public class LiquidPool : GDKnyttBaseObject
{
    bool ping = true;

    public override void _Ready()
    {
        this._impl_initialize();
    }

    public void _on_AnimatedSprite_animation_finished()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.Stop();
        if (ping) { player.Play("Pool1", true); }
        else { player.Play("Pool1"); }
        ping = !ping;
    }

    protected override void _impl_initialize()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = ((float)GDKnyttDataStore.random.NextDouble()) * .4f + .8f;
        //player.frame (set the frame)
        player.Play("Pool1");
        
    }

    protected override void _impl_process(float delta)
    {
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        ((Juni)(body)).die();
    }
}
