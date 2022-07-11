using Godot;
using System;
using System.Text;
using System.Collections.Generic;

public class Console : CanvasLayer
{
    bool showing = false;
    bool sliding_out = false;
    HashSet<char> disallowed = new HashSet<char>(new char[] {'`'});
    LineEdit lineEdit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var consoleContainer = GetNode<Control>("ConsoleContainer");
        consoleContainer.MarginTop = -240;
        consoleContainer.MarginBottom = -240;
        lineEdit = GetNode<LineEdit>("ConsoleContainer/Panel/LineEdit");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("debug_console"))
        {
            lineEdit.ReleaseFocus();
            toggleConsole();
        }
    }

    private void toggleConsole()
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");

        if (!anim.IsPlaying())
        {
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
        lineEdit.GrabFocus();
    }

    public void _on_LineEdit_text_changed(string newText)
    {
        var caretPosition = lineEdit.CaretPosition;

        StringBuilder sb = new StringBuilder();
        foreach (var c in newText)
        {
            if (!disallowed.Contains(c)) { sb.Append(c); }
        }

        lineEdit.Text = sb.ToString();
        lineEdit.CaretPosition = caretPosition;
    }
}
