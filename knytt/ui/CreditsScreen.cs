using Godot;

public partial class CreditsScreen : BasicScreen
{
    private bool testers_open = true;
    private bool tilesets_open = true;
    private bool music_open = true;

    public override void _Ready()
    {
        base._Ready();
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TestersPanel/AnimationPlayer").Play("RESET");
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TilesetsPanel/AnimationPlayer").Play("RESET");
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/MusicPanel/AnimationPlayer").Play("RESET");

        // Apply scrollbar styles via code (workaround for Godot 4 Theme bug)
        var scrollContainer = GetNode<ScrollContainer>("ScrollContainer");
        var vScrollBar = scrollContainer.GetVScrollBar();
        var hScrollBar = scrollContainer.GetHScrollBar();

        // Set scrollbar to grabber width
        vScrollBar.CustomMinimumSize = new Vector2(11, 0);
        hScrollBar.CustomMinimumSize = new Vector2(0, 11);

        // Track: use bg for the 1px center line, borders to mask the sides with background color
        var trackStyle = new StyleBoxFlat();
        trackStyle.BgColor = new Color(0.6f, 0.6f, 0.6f, 1f);  // The visible track line
        trackStyle.BorderColor = new Color(1f, 1f, 1f, 1f);     // Match credits background (white)
        trackStyle.BorderWidthLeft = 5;
        trackStyle.BorderWidthRight = 5;
        trackStyle.BorderWidthTop = 0;
        trackStyle.BorderWidthBottom = 0;

        // Grabber: solid rectangular box (full width, no expand needed)
        var grabberStyle = new StyleBoxFlat();
        grabberStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        var grabberHoverStyle = new StyleBoxFlat();
        grabberHoverStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        var grabberPressedStyle = new StyleBoxFlat();
        grabberPressedStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        vScrollBar.AddThemeStyleboxOverride("scroll", trackStyle);
        vScrollBar.AddThemeStyleboxOverride("grabber", grabberStyle);
        vScrollBar.AddThemeStyleboxOverride("grabber_highlight", grabberHoverStyle);
        vScrollBar.AddThemeStyleboxOverride("grabber_pressed", grabberPressedStyle);

        hScrollBar.AddThemeStyleboxOverride("scroll", trackStyle);
        hScrollBar.AddThemeStyleboxOverride("grabber", grabberStyle);
        hScrollBar.AddThemeStyleboxOverride("grabber_highlight", grabberHoverStyle);
        hScrollBar.AddThemeStyleboxOverride("grabber_pressed", grabberPressedStyle);

        initFocus();
    }
    
    public override void initFocus()
    {
        GetNode<Button>("BackButton").GrabFocus();
    }

    private void _on_Testers_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TestersPanel/AnimationPlayer").Play(
            testers_open ? "collapse" : "expand");
        testers_open = !testers_open;
    }

    private void _on_Tilesets_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TilesetsPanel/AnimationPlayer").Play(
            tilesets_open ? "collapse" : "expand");
        tilesets_open = !tilesets_open;
    }

    private void _on_Music_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/MusicPanel/AnimationPlayer").Play(
            music_open ? "collapse" : "expand");
        music_open = !music_open;
    }
}
