using Godot;
using System;

public class HoldButton : Password // inheritance for destroyWalls() function
{
    private float timer = 0;
    private bool entered = false;
    private AnimatedSprite animatedSprite;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (entered) { timer += delta; }

        int frame = Math.Min((int)(timer + 1.9f), 7);
        string state = entered ? "inc" : "stop";
        string anim = $"{frame}_{state}";

        if (animatedSprite.Animation != anim) { animatedSprite.Play(anim); }
        if (frame == 7) { destroyWalls(); }
    }

    private void _on_Area2D_body_entered(object body)
    {
        entered = true;
    }

    private void _on_Area2D_body_exited(object body)
    {
        entered = false;
    }
}
