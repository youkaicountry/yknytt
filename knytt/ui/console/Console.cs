using Godot;
using System;

public class Console : CanvasLayer
{
    bool showing = false;
    bool sliding_out = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var consoleContainer = GetNode<Control>("ConsoleContainer");
        consoleContainer.MarginTop = -240;
        consoleContainer.MarginBottom = -240;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("debug_console"))
        {
            toggleConsole();
        }      
    }

    private void toggleConsole()
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");

        if (!anim.IsPlaying())
        {
            anim.PlaybackSpeed = 6f;
            if (showing) { anim.PlayBackwards("SlideOut"); sliding_out = false; }
            else { anim.Play("SlideOut"); sliding_out = true; }
        }
        else
        {
            anim.PlaybackSpeed *= -1f;
            sliding_out = !sliding_out;
        }
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        showing = sliding_out;
    }
}
