using Godot;

public static class ScrollBarStyle
{
    private const int Width = 11;
    private const int TrackThickness = 1;

    private static StyleBoxFlat _track;
    private static StyleBoxFlat _grabber;
    private static StyleBoxFlat _grabberHL;
    private static StyleBoxFlat _grabberPressed;

    private static void Init()
    {
        if (_track != null) return;

        // Track: black background, narrowed by borders to create a centered 1px line.
        // Border color is fully transparent so it works on any background.
        int borderSide = (Width - TrackThickness) / 2;
        _track = new StyleBoxFlat();
        _track.BgColor = new Color(0, 0, 0, 1);
        _track.BorderColor = new Color(0, 0, 0, 0);
        _track.BorderWidthLeft = borderSide;
        _track.BorderWidthRight = borderSide;
        _track.BorderWidthTop = 4;
        _track.BorderWidthBottom = 4;

        _grabber = new StyleBoxFlat();
        _grabber.BgColor = new Color(0.3f, 0.3f, 0.3f, 1);
        _grabber.ContentMarginTop = 6;
        _grabber.ContentMarginBottom = 6;

        _grabberHL = new StyleBoxFlat();
        _grabberHL.BgColor = new Color(0.2f, 0.2f, 0.2f, 1);
        _grabberHL.ContentMarginTop = 6;
        _grabberHL.ContentMarginBottom = 6;

        _grabberPressed = new StyleBoxFlat();
        _grabberPressed.BgColor = new Color(0.1f, 0.1f, 0.1f, 1);
        _grabberPressed.ContentMarginTop = 6;
        _grabberPressed.ContentMarginBottom = 6;
    }

    public static void Apply(ScrollContainer container)
    {
        Init();

        var v = container.GetVScrollBar();
        v.CustomMinimumSize = new Vector2(Width, 0);
        v.AddThemeStyleboxOverride("scroll", _track);
        v.AddThemeStyleboxOverride("grabber", _grabber);
        v.AddThemeStyleboxOverride("grabber_highlight", _grabberHL);
        v.AddThemeStyleboxOverride("grabber_pressed", _grabberPressed);

        var h = container.GetHScrollBar();
        h.CustomMinimumSize = new Vector2(0, Width);
        h.AddThemeStyleboxOverride("scroll", _track);
        h.AddThemeStyleboxOverride("grabber", _grabber);
        h.AddThemeStyleboxOverride("grabber_highlight", _grabberHL);
        h.AddThemeStyleboxOverride("grabber_pressed", _grabberPressed);
    }
}
