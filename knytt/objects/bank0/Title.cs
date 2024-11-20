using Godot;

public class Title : GDKnyttBaseObject
{
    public override void _Ready()
    {
        string title = GDArea.Area.getExtraData("Title");
        string subtitle = GDArea.Area.getExtraData("Subtitle");

        var title_node = GDArea.GDWorld.Game.GetNode<Control>("%Title");
        var title_label = title_node.GetNode<Label>("TitleLabel");
        var subtitle_label = title_node.GetNode<Label>("SubtitleLabel");
        var anim_player = title_node.GetNode<AnimationPlayer>("AnimationPlayer");

        if (title_label.GetFont("font")    is BitmapFont) { title    = title?.Replace(' ', '\x100'); }
        if (subtitle_label.GetFont("font") is BitmapFont) { subtitle = subtitle?.Replace(' ', '\x100'); }

        if (title != null) { adjustLabel(title, title_label); }
        if (subtitle != null) { adjustLabel(subtitle, subtitle_label); }

        if (title != null || subtitle != null)
        {
            title_label.Modulate = subtitle_label.Modulate = new Color(1, 1, 1, 0);
            anim_player.Stop();
            anim_player.Play(title != null && subtitle != null ? "title_subtitle" : title != null ? "title" : "subtitle");
        }
    }

    private void adjustLabel(string text, Label label)
    {
        label.Text = ""; // complex workaround to update label size
        label.Hide();
        label.Show();
        label.RectSize = Vector2.Zero;
        label.Text = text;
        label.Hide();
        label.Show();
        
        label.RectPosition = new Vector2(
            Coords.x < 8 ?  -300 + Position.x :                            // left align
            Coords.x > 16 ? -300 + Position.x + 24 - label.RectSize.x :    // right align
                            -300 + Position.x + 12 - label.RectSize.x / 2, // center align
            Position.y - 120
        );
    }
}
