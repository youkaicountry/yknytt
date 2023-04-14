using Godot;
using YUtil.Random;

public partial class LiquidPool : GDKnyttBaseObject
{
    protected AnimatedSprite2D player;
    float ping = 1;

    public override void _Ready()
    {
        base._Ready();
        GetNodeOrNull<AnimatedSprite2D>("Halo")?.Play();
        
        player = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        player.SpeedScale = random.NextFloat(.6f, 1f);
        player.Play();
        player.Frame = random.Next(player.SpriteFrames.GetFrameCount(player.Animation));
    }

    public void _on_AnimatedSprite_animation_finished()
    {
        ping = -ping;
        player.Play(customSpeed: ping, fromEnd: ping == -1);
    }
}
