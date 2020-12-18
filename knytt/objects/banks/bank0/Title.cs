using Godot;
using System;

public class Title : GDKnyttBaseObject
{
    public override void _Ready()
    {
        string title = GDArea.Area.getExtraData("Title");
        string subtitle = GDArea.Area.getExtraData("Subtitle");

        if (title != null) { GetNode<Label>("TitleLabel").Text = title; adjustLabel(GetNode<Label>("TitleLabel")); }
        if (subtitle != null) { GetNode<Label>("SubtitleLabel").Text = subtitle; adjustLabel(GetNode<Label>("SubtitleLabel")); }

        if (title != null || subtitle != null)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play(
                title != null && subtitle != null ? "title_subtitle" :
                title != null ? "title" : "subtitle");
        }
    }

    private void adjustLabel(Label label)
    {
        label.RectPosition = label.RectPosition; // Workaround to update label size
        if (Coords.x < 8)
        {
            ;
        }
        else if (Coords.x > 16)
        {
            label.RectPosition = new Vector2(24 - label.RectSize.x, label.RectPosition.y); // right align
        }
        else
        {
            label.RectPosition = new Vector2(-Position.x + (600 - label.RectSize.x) / 2, label.RectPosition.y); // center align
        }
    }
}
