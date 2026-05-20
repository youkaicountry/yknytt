using Godot;

public class Title : GDKnyttBaseObject
{
    private Label title_label;
    private Label subtitle_label;
    
    public override void _Ready()
    {
        string title = GDArea.Area.getExtraData("Title");
        string subtitle = GDArea.Area.getExtraData("Subtitle");

        var title_node = GDArea.GDWorld.Game.GetNode<Control>("%Title");
        title_label = title_node.GetNode<Label>("TitleLabel");
        subtitle_label = title_node.GetNode<Label>("SubtitleLabel");
        var anim_player = title_node.GetNode<AnimationPlayer>("AnimationPlayer");

        if (title_label.GetFont("font")    is BitmapFont) { title    = title?.Replace(' ', '\x100'); }
        if (subtitle_label.GetFont("font") is BitmapFont) { subtitle = subtitle?.Replace(' ', '\x100'); }

        if (title_label.Text == title) { title = null; }
        if (subtitle_label.Text == subtitle) { subtitle = null; }

        if (title != null) { adjustLabel(title, title_label, initial: true); }
        if (subtitle != null) { adjustLabel(subtitle, subtitle_label, initial: true); }

        if (title != null || subtitle != null)
        {
            title_label.Modulate = subtitle_label.Modulate = new Color(1, 1, 1, 0);
            anim_player.Stop();
            anim_player.Play(title != null && subtitle != null ? "title_subtitle" : title != null ? "title" : "subtitle");
        }
    }

    private void adjustLabel(string text, Label label, bool initial)
    {
        if (initial)
        {
            label.Text = ""; // complex workaround to update label size
            label.Hide();
            label.Show();
            label.RectSize = Vector2.Zero;
            label.Text = text;
            label.Hide();
            label.Show();
        }
        
        var object_left_range = -300 + Position.x;
        var object_right_range = -300 + Position.x + 24;
        var object_center = -300 + Position.x + 12;
        
        if (GDKnyttSettings.SideScroll && !initial)
        {
            float x_viewport = GetViewport().GetVisibleRect().Size.x * TouchSettings.ViewportNow;
            object_left_range = Mathf.Max(object_left_range, -x_viewport / 2);
            object_right_range = Mathf.Min(object_right_range, x_viewport / 2);
            if (label.RectSize.x < x_viewport)
            {
                if (object_center - label.RectSize.x / 2 < -x_viewport / 2) { object_center = -x_viewport / 2 + label.RectSize.x / 2; }
                if (object_center + label.RectSize.x / 2 >  x_viewport / 2) { object_center =  x_viewport / 2 - label.RectSize.x / 2; }
            }
        }

        label.RectPosition = new Vector2(
            Coords.x < 8 ?  object_left_range :                      // left align
            Coords.x > 16 ? object_right_range - label.RectSize.x :  // right align
                            object_center - label.RectSize.x / 2,    // center align
            Position.y - 120
        );
    }

    public override void _PhysicsProcess(float delta)
    {
        if (GDKnyttSettings.SideScroll)
        {
            adjustLabel(title_label.Text, title_label, initial: false);
            adjustLabel(subtitle_label.Text, subtitle_label, initial: false);
        }
    }
}
