using Godot;

public partial class Title : GDKnyttBaseObject
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
        label.Position = label.Position; // Workaround to update label size
        if (Coords.x < 8)
        {
            ;
        }
        else if (Coords.x > 16)
        {
            label.Position = new Vector2(24 - label.Size.X, label.Position.Y); // right align
        }
        else
        {
            label.Position = new Vector2(-Position.X + (600 - label.Size.X) / 2, label.Position.Y); // center align
        }
    }
}
