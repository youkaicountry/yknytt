using Godot;

public class LayerButton : Control
{
    [Signal] public delegate void Chosen(int layer);
    [Signal] public delegate void VisibilityToggled(int layer);

    [Export] public int Layer { get; set; }
    [Export] public Color Tint { get; set; } = new Color(1, 1, 1);
    [Export] public Color TextColor { get; set; } = new Color(1, 1, 1);
    [Export] public Texture Line { get; set; }
    [Export] public Texture Face { get; set; }
    [Export] public Font Font { get; set; }

    private const int TOUCH = -1;
    private const ulong LONG_PRESS_MS = 500;
    private const float HIDDEN_ALPHA = 0.37f;

    // KS+'s diamond: the faces hide what's behind them, a selected one takes the layer's colour
    private static readonly Color FACE_COLOR = Color.Color8(102, 102, 102, 194);
    private static readonly Color HIDDEN_FACE_COLOR = Color.Color8(102, 102, 102, 155);
    private static readonly Color SELECTED_TEXT = Color.Color8(247, 195, 79);
    private static readonly Vector2[] CORNERS = { new Vector2(20, 0), new Vector2(55, 21), new Vector2(35, 32), new Vector2(0, 12) };

    private bool selected;
    private bool shown = true;
    private ulong touch_time;

    public bool Selected { get { return selected; } set { selected = value; Update(); } }
    public bool Shown { get { return shown; } set { shown = value; Update(); } }

    public override void _Draw()
    {
        float alpha = shown ? 1 : HIDDEN_ALPHA;
        DrawTexture(Face, Vector2.Zero, selected ? new Color(Tint.r, Tint.g, Tint.b, 0.49f) : shown ? FACE_COLOR : HIDDEN_FACE_COLOR);
        DrawTexture(Line, Vector2.Zero, new Color(Tint.r, Tint.g, Tint.b, alpha));

        string text = Layer.ToString();
        var color = selected ? SELECTED_TEXT : TextColor;
        float width = Font.GetStringSize(text).x - 2;
        var position = new Vector2(CORNERS[0].x - Mathf.Floor((width - 1) / 2), 6 + Font.GetAscent());
        DrawString(Font, position, text, new Color(color.r, color.g, color.b, alpha));
    }

    public override bool HasPoint(Vector2 point)
    {
        return Geometry.IsPointInPolygon(point, CORNERS);
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
