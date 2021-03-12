using Godot;
using YUtil.Random;

public class LiquidPool : GDKnyttBaseObject
{
    protected AnimatedSprite player;
    bool ping = true;

    public override void _Ready()
    {
        base._Ready();
        GetNodeOrNull<AnimatedSprite>("Halo")?.Play();
        
        player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.SpeedScale = random.NextFloat(.6f, 1f);
        player.Play();
        player.Frame = random.Next(player.Frames.GetFrameCount(player.Animation));
    }

    public void _on_AnimatedSprite_animation_finished()
    {
        player.Stop();
        player.Play(backwards: ping);
        ping = !ping;
    }
}
