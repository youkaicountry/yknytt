using Godot;
using System;
using YUtil.Random;

public class CircleBird : GDKnyttBaseObject
{
    private string direction;
    
    private Vector2 IDLE_RANGE = new Vector2(1f, 3f);
    private string[] animations = {"Chirp", "Hop", "HopMulti", "Wink"};
    
    public override void _Ready()
    {
        direction = "Left";
        nextAnimation();
    }
    
    private void nextAnimation()
    {
        direction = GDKnyttDataStore.random.Next(4) != 0 ? direction :
            (direction == "Left" ? "Right" : "Left");
        string anim_name = animations[GDKnyttDataStore.random.Next(4)];

        // Play animation
        var anim = GetNode<AnimatedSprite>("AnimatedSprite");
        anim.Stop();
        anim.Play($"{direction}{anim_name}");

        // Idle
        var timer = GetNode<Timer>("IdleTimer");
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(IDLE_RANGE.x, IDLE_RANGE.y);
        timer.Start();
    }
    
    private void _on_IdleTimer_timeout()
    {
        nextAnimation();
    }
}
