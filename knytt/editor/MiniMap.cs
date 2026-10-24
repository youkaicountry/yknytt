using Godot;
using YKnyttLib;

public class MiniMap : Control
{
    [Signal] public delegate void ScreenSelected(int x, int y);

    private const int COLUMNS = 16;
    private const int ROWS = 11;
    private const int CELL = 12;
    private static readonly KnyttPoint CENTER = new KnyttPoint(COLUMNS / 2, ROWS / 2);

    private static readonly Color GRID_COLOR = Color.Color8(48, 85, 112);
    private static readonly Color EMPTY_COLOR = Color.Color8(58, 102, 135);
    private static readonly Color SCREEN_COLOR = Color.Color8(171, 199, 243);
    private static readonly Color SCREEN_EDGE_COLOR = Color.Color8(127, 167, 239);
    private static readonly Color CURRENT_EDGE_COLOR = Color.Color8(219, 219, 219);

    private KnyttWorld world;
    private KnyttPoint coords;

    public void showScreen(KnyttWorld world, KnyttPoint coords)
    {
        this.world = world;
        this.coords = coords;
        Update();
    }

    private KnyttPoint screenAt(int column, int row) => coords + new KnyttPoint(column, row) - CENTER;

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, RectSize), GRID_COLOR);
        if (world == null) { return; }

        for (int row = 0; row < ROWS; row++)
        {
            for (int column = 0; column < COLUMNS; column++)
            {
                var rect = new Rect2(column * CELL + 1, row * CELL + 1, CELL - 1, CELL - 1);
                var screen = screenAt(column, row);
                if (world.Map.ContainsKey(screen))
                {
                    DrawRect(rect, SCREEN_EDGE_COLOR);
                    DrawRect(rect.Grow(-1), SCREEN_COLOR);
                }
                else
                {
                    DrawRect(rect, EMPTY_COLOR);
                }
                if (screen.Equals(coords)) { DrawRect(rect, CURRENT_EDGE_COLOR, filled: false); }
            }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button && button.Pressed && button.ButtonIndex == (int)ButtonList.Left)
        {
            var screen = screenAt((int)button.Position.x / CELL, (int)button.Position.y / CELL);
            EmitSignal(nameof(ScreenSelected), screen.x, screen.y);
        }
    }
}
