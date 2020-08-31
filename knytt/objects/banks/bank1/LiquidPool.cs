using Godot;
using YUtil.Random;

public class LiquidPool : GDKnyttBaseObject
{
    bool ping = true;

    public void _on_AnimatedSprite_animation_finished()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.Stop();
        player.Play("Pool1", ping);
        ping = !ping;
    }

    protected override void _impl_initialize()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = (float)GDKnyttDataStore.random.NextDouble(.6f, 1f);
        
        string anim = "Pool1";
        player.Play(anim);
        player.Frame = GDKnyttDataStore.random.Next(player.Frames.GetFrameCount(anim));
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
