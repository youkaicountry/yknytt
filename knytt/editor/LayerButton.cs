using Godot;

public class LayerButton : Control
{
    [Signal] public delegate void Chosen(int layer);
    [Signal] public delegate void VisibilityToggled(int layer);

    [Export] public int Layer { get; set; }
    [Export] public Color Tint { get; set; } = new Color(1, 1, 1);

    private const int TOUCH = -1;
    private const ulong LONG_PRESS_MS = 500;
    private static readonly Color SELECTED_TEXT = Color.Color8(250, 200, 60);

    private bool selected;
    private bool shown = true;
    private ulong touch_time;

    public bool Selected { get { return selected; } set { selected = value; Update(); } }
    public bool Shown { get { return shown; } set { shown = value; Update(); } }

    private Vector2[] Corners => new Vector2[] {
        new Vector2(0, RectSize.y / 2), new Vector2(RectSize.x / 2, 0),
        new Vector2(RectSize.x, RectSize.y / 2), new Vector2(RectSize.x / 2, RectSize.y) };

    public override void _Draw()
    {
        var color = new Color(Tint.r, Tint.g, Tint.b, shown ? 1 : 0.3f);
        var corners = Corners;
        if (selected) { DrawColoredPolygon(corners, new Color(color.r, color.g, color.b, color.a * 0.45f)); }
        DrawPolyline(new Vector2[] { corners[0], corners[1], corners[2], corners[3], corners[0] }, color);

        var font = GetFont("font");
        string text = Layer.ToString();
        var position = (RectSize - font.GetStringSize(text)) / 2 + new Vector2(0, font.GetAscent());
        DrawString(font, position.Floor(), text, selected ? SELECTED_TEXT : color);
    }

    public override bool HasPoint(Vector2 point)
    {
        var half = RectSize / 2;
        return Mathf.Abs(point.x - half.x) / half.x + Mathf.Abs(point.y - half.y) / half.y <= 1;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton button)) { return; }

        if (button.Device == TOUCH && button.ButtonIndex == (int)ButtonList.Left)
        {
            if (button.Pressed) { touch_time = OS.GetTicksMsec(); }
            else if (OS.GetTicksMsec() - touch_time >= LONG_PRESS_MS) { EmitSignal(nameof(VisibilityToggled), Layer); }
            else { EmitSignal(nameof(Chosen), Layer); }
        }
        else if (button.Pressed && button.ButtonIndex == (int)ButtonList.Left) { EmitSignal(nameof(Chosen), Layer); }
        else if (button.Pressed && button.ButtonIndex == (int)ButtonList.Right) { EmitSignal(nameof(VisibilityToggled), Layer); }
        else { return; }

        AcceptEvent();
    }
}
