using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using YUtil.Random;

public class GesturesObject : GDKnyttBaseObject
{
    [Export] float minTime = 1;
    [Export] float maxTime = 2;
    [Export] bool bidirectional = false;

    protected AnimatedSprite gesturesSprite;
    protected Timer timer;
    private List<string> animations;
    private bool leftDirection = true;
    
    public override void _Ready()
    {
        gesturesSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        timer = GetNode<Timer>("IdleTimer");
        animations = gesturesSprite.Frames.GetAnimationNames().Where(s => !s.StartsWith("_")).ToList();

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
            leftDirection = GDKnyttDataStore.random.Next(2) == 0;
        }
        string anim_name = animations[GDKnyttDataStore.random.Next(animations.Count)];

        // Play animation
        if (bidirectional) { gesturesSprite.FlipH = !leftDirection; }
        gesturesSprite.Frame = 0;
        gesturesSprite.Play(anim_name);

        // Idle
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(minTime, maxTime);
        timer.Start();
    }
}
