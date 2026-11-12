using Godot;

// The 64 colours MapColor can give a screen on the map: red + green×4 + blue×16, each 0-3
public class MapColorGrid : Control
{
    [Signal] public delegate void Chosen(int color);

    private const int CELL = 16, GAP = 2;

    // The colour now, -1 none
    public int Current { get; set; } = -1;
    private int hovered = -1;

    public static Color colorOf(int color) => new Color((color % 4) / 3f, (color / 4 % 4) / 3f, (color / 16 % 4) / 3f);

    // Blue changes by block, green down, red across: two blocks of 4×4 side by side, twice
    private static Vector2 cellAt(int color)
    {
        int blue = color / 16;
        return new Vector2(color % 4 + blue % 2 * 4, color / 4 % 4 + blue / 2 * 4) * (CELL + GAP);
    }

    private int colorAt(Vector2 position)
    {
        for (int color = 0; color < 64; color++)
        {
            if (new Rect2(cellAt(color), CELL, CELL).HasPoint(position)) { return color; }
        }
        return -1;
    }

    public override Vector2 _GetMinimumSize() => new Vector2(8, 8) * (CELL + GAP) - new Vector2(GAP, GAP);

    public override void _Draw()
    {
        for (int color = 0; color < 64; color++)
        {
            var rect = new Rect2(cellAt(color), CELL, CELL);
            DrawRect(rect, colorOf(color));
            if (color == Current || color == hovered) { DrawRect(rect.Grow(1), color == Current ? Colors.Black : new Color(0.5f, 0.5f, 0.86f), false); }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            int color = colorAt(motion.Position);
            if (color != hovered) { hovered = color; HintTooltip = color < 0 ? "" : $"MapColor={color}"; Update(); }
        }
        if (@event is InputEventMouseButton click && click.Pressed && click.ButtonIndex == (int)ButtonList.Left)
        {
            int color = colorAt(click.Position);
            if (color >= 0) { AcceptEvent(); EmitSignal(nameof(Chosen), color); }
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationMouseExit && hovered >= 0) { hovered = -1; Update(); }
    }
}
