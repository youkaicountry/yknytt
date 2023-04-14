using Godot;
using System.Collections.Generic;
using System.Linq;
using YUtil.Random;

public partial class GesturesObject : GDKnyttBaseObject
{
    [Export] float minTime = 1;
    [Export] float maxTime = 2;
    [Export] bool bidirectional = false;

    protected AnimatedSprite2D sprite;
    protected Timer timer;
    private List<string> animations;
    private bool leftDirection = true;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        timer = GetNode<Timer>("IdleTimer");
        animations = sprite.SpriteFrames.GetAnimationNames().Where(s => !s.StartsWith("_")).ToList();

        nextAnimation();
    }

    private void _on_IdleTimer_timeout()
    {
        nextAnimation();
    }

    protected virtual void nextAnimation()
    {
        if (bidirectional)
        {
            leftDirection = random.NextBoolean();
        }
        string anim_name = random.NextElement(animations);

        // Play animation
        if (bidirectional) { sprite.FlipH = !leftDirection; }
        sprite.Frame = 0;
        sprite.Play(anim_name);

        idle();
    }

    protected void idle()
    {
        timer.Start(random.NextFloat(minTime, maxTime));
    }
}
