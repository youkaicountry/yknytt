using Godot;
using YKnyttLib;

public class ScreenView : Control
{
    [Signal] public delegate void StrokeStarted();
    [Signal] public delegate void CellPainted(int x, int y);
    [Signal] public delegate void StrokeEnded();
    [Signal] public delegate void CellMenu(int x, int y);
    [Signal] public delegate void CellHovered(int x, int y);
    [Signal] public delegate void CellPlaced(int x, int y);
    [Signal] public delegate void CellFilled(int x, int y);
    [Signal] public delegate void CellReplaced(int x, int y);
    [Signal] public delegate void AreaCopied(int x, int y, int width, int height);

    [Export] public Texture JuniTexture { get; set; }

    public static readonly Vector2 Offset = new Vector2(25, 24);

    public const int LAYERS = KnyttArea.AREA_TILE_LAYERS + KnyttArea.AREA_SPRITE_LAYERS;

    private static readonly Vector2 TILE = new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
    private static readonly Color NEIGHBOUR_SHADE = new Color(0, 0, 0, 0.45f);
    private static readonly Color START_COLOR = new Color(1, 1, 0);
    private static readonly Color STAMP_COLOR = new Color(1, 1, 1, 0.6f);
    private const float FADED_LAYER = 0.35f;
    private const int TOUCH = -1;
    private const ulong LONG_PRESS_MS = 500;

    public int CurrentLayer { get; set; }
    public bool FadeOthers { get; set; } = true;
    public bool Painting { get; set; } = true;
    public bool Peeking { get; set; }
    public bool Placing { get; set; }
    public bool[] LayerShown { get; } = new bool[] { true, true, true, true, true, true, true, true };

    private KnyttWorld world;
    private AreaPainter painter;
    private KnyttPoint coords;
    private int background;
    private KnyttPoint start_area;
    private KnyttPoint start_position;
    private NinePatchRect cursor;
    private TextureRect brush;
    private Vector2? last_cell;
    private Vector2? hovered_cell;
    private Vector2? touch_cell;
    private Vector2? copy_start;
    private Vector2 copy_end;
    private KnyttStamp stamp;
    private bool stamp_objects;
    private Texture stamp_tileset_a;
    private Texture stamp_tileset_b;
    private ulong touch_time;

    public override void _Ready()
    {
        cursor = GetNode<NinePatchRect>("Cursor");
        brush = GetNode<TextureRect>("Cursor/Brush");
    }

    public void init(KnyttWorld world, EditorAssets assets)
    {
        this.world = world;
        painter = new AreaPainter(assets);
    }

    public void setStart(KnyttPoint area, KnyttPoint position)
    {
        start_area = area;
        start_position = position;
        Update();
    }

    // Juni as the brush, for placing where a test or the level starts
    public void showJuni()
    {
        setBrush(JuniTexture, new Rect2(Vector2.Zero, TILE));
    }

    public void showScreen(KnyttPoint coords, int background)
    {
        this.coords = coords;
        this.background = background;
        Update();
    }

    public void setBrush(Texture texture, Rect2 region)
    {
        var atlas = (AtlasTexture)brush.Texture;
        atlas.Atlas = texture;
        atlas.Region = region;
        brush.Visible = texture != null;
        brush.MinimumSizeChanged();
        brush.Update();
    }

    // A brush of many cells, or null for the one drawn by setBrush
    public void setStamp(KnyttStamp stamp, bool objects, Texture tileset_a, Texture tileset_b)
    {
        this.stamp = stamp;
        stamp_objects = objects;
        stamp_tileset_a = tileset_a;
        stamp_tileset_b = tileset_b;
        if (hovered_cell != null) { placeCursor(hovered_cell.Value); }
        Update();
    }

    private float layerAlpha(int layer, bool neighbour)
    {
        if (!LayerShown[layer]) { return 0; }
        return neighbour || !FadeOthers || Peeking || CurrentLayer < 0 || layer == CurrentLayer ? 1 : FADED_LAYER;
    }

    public override void _Draw()
    {
        if (world == null) { return; }

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) { continue; }
                // Neighbours are shaded, where there are none the editor shows through like KS+
                var side = world.getArea(coords + new KnyttPoint(x, y));
                var origin = Offset + new Vector2(x, y) * GDKnyttArea.Size;
                painter.drawArea(this, side, origin, layer => layerAlpha(layer, neighbour: true));
                if (side != null && !side.Empty) { DrawRect(new Rect2(origin, GDKnyttArea.Size), NEIGHBOUR_SHADE); }
            }
        }

        var area = world.getArea(coords);
        if (area == null || area.Empty) { painter.drawGradient(this, background, Offset); }
        else { painter.drawArea(this, area, Offset, layer => layerAlpha(layer, neighbour: false)); }

        if (coords.Equals(start_area))
        {
            var position = Offset + new Vector2(start_position.x, start_position.y) * TILE;
            DrawTextureRectRegion(JuniTexture, new Rect2(position, TILE), new Rect2(Vector2.Zero, TILE));
            drawFrame(new Rect2(position, TILE), START_COLOR);
        }

        if (stamp != null && hovered_cell != null && copy_start == null && Painting && !Placing)
        {
            var cell = hovered_cell.Value;
            painter.drawStamp(this, stamp, stamp_objects, stamp_tileset_a, stamp_tileset_b, new KnyttPoint((int)cell.x, (int)cell.y), Offset + cell * TILE, STAMP_COLOR);
        }
    }

    // The frame shows the brush's size, or the area being copied
    private void placeCursor(Vector2 cell)
    {
        var size = stamp == null || Placing ? Vector2.One : new Vector2(stamp.Width, stamp.Height);
        var room = new Vector2(KnyttArea.AREA_WIDTH, KnyttArea.AREA_HEIGHT) - cell;
        cursor.RectPosition = Offset + cell * TILE;
        cursor.RectSize = new Vector2(Mathf.Min(size.x, room.x), Mathf.Min(size.y, room.y)) * TILE;
    }

    private Rect2 CopyArea
    {
        get
        {
            var from = new Vector2(Mathf.Min(copy_start.Value.x, copy_end.x), Mathf.Min(copy_start.Value.y, copy_end.y));
            var to = new Vector2(Mathf.Max(copy_start.Value.x, copy_end.x), Mathf.Max(copy_start.Value.y, copy_end.y));
            return new Rect2(from, to - from + Vector2.One);
        }
    }

    private void showCopyArea()
    {
        cursor.RectPosition = Offset + CopyArea.Position * TILE;
        cursor.RectSize = CopyArea.Size * TILE;
        Update();
    }

    private void drawFrame(Rect2 rect, Color color)
    {
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.x, 1)), color);
        DrawRect(new Rect2(rect.Position.x, rect.End.y - 1, rect.Size.x, 1), color);
        DrawRect(new Rect2(rect.Position, new Vector2(1, rect.Size.y)), color);
        DrawRect(new Rect2(rect.End.x - 1, rect.Position.y, 1, rect.Size.y), color);
    }

    private static Vector2? cellAt(Vector2 position)
    {
        var cell = ((position - Offset) / TILE).Floor();
        bool inside = cell.x >= 0 && cell.y >= 0 && cell.x < KnyttArea.AREA_WIDTH && cell.y < KnyttArea.AREA_HEIGHT;
        return inside ? cell : (Vector2?)null;
    }

    // Touch has no right button: a tap paints, a drag paints a stroke, a long press opens the menu.
    // Shift + click fills, Shift + right-click replaces, Ctrl + drag copies an area, like KS+
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            var cell = cellAt(button.Position);
            if (button.ButtonIndex == (int)ButtonList.Left && button.Pressed && cell != null)
            {
                if (Placing)
                {
                    EmitSignal(nameof(CellPlaced), (int)cell.Value.x, (int)cell.Value.y);
                    return;
                }
                if (button.Control && Painting)
                {
                    copy_start = cell;
                    copy_end = cell.Value;
                    showCopyArea();
                    return;
                }
                if (button.Shift && Painting)
                {
                    EmitSignal(nameof(CellFilled), (int)cell.Value.x, (int)cell.Value.y);
                    return;
                }
                if (button.Device == TOUCH)
                {
                    touch_cell = cell;
                    touch_time = OS.GetTicksMsec();
                    return;
                }
                startStroke(cell.Value);
            }
            else if (button.ButtonIndex == (int)ButtonList.Left && !button.Pressed && copy_start != null)
            {
                var area = CopyArea;
                copy_start = null;
                EmitSignal(nameof(AreaCopied), (int)area.Position.x, (int)area.Position.y, (int)area.Size.x, (int)area.Size.y);
                placeCursor(copy_end);
                Update();
            }
            else if (button.ButtonIndex == (int)ButtonList.Left && !button.Pressed)
            {
                if (touch_cell != null)
                {
                    var start = touch_cell.Value;
                    touch_cell = null;
                    if (OS.GetTicksMsec() - touch_time >= LONG_PRESS_MS) { menu(start); return; }
                    startStroke(start);
                }
                if (last_cell != null)
                {
                    last_cell = null;
                    EmitSignal(nameof(StrokeEnded));
                }
            }
            else if (button.ButtonIndex == (int)ButtonList.Right && button.Pressed && cell != null && button.Shift && Painting && !Placing)
            {
                EmitSignal(nameof(CellReplaced), (int)cell.Value.x, (int)cell.Value.y);
            }
            else if (button.ButtonIndex == (int)ButtonList.Right && button.Pressed && cell != null)
            {
                menu(cell.Value);
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var cell = cellAt(motion.Position);
            if (copy_start != null)
            {
                hover(cell);
                if (cell != null && cell != copy_end)
                {
                    copy_end = cell.Value;
                    showCopyArea();
                }
                return;
            }
            cursor.Visible = cell != null && (Painting || Placing);
            hover(cell);
            if (cell == null) { return; }
            placeCursor(cell.Value);

            if (touch_cell != null && cell != touch_cell)
            {
                startStroke(touch_cell.Value);
                touch_cell = null;
            }
            if (last_cell != null && cell != last_cell) { paint(cell.Value); }
        }
    }

    private void startStroke(Vector2 cell)
    {
        EmitSignal(nameof(StrokeStarted));
        paint(cell);
    }

    private void menu(Vector2 cell)
    {
        EmitSignal(nameof(CellMenu), (int)cell.x, (int)cell.y);
    }

    private void paint(Vector2 cell)
    {
        var from = last_cell ?? cell;
        int steps = (int)Mathf.Max(Mathf.Abs(cell.x - from.x), Mathf.Abs(cell.y - from.y));
        for (int i = 1; i <= steps; i++)
        {
            var step = from.LinearInterpolate(cell, (float)i / steps).Round();
            EmitSignal(nameof(CellPainted), (int)step.x, (int)step.y);
        }
        if (steps == 0) { EmitSignal(nameof(CellPainted), (int)cell.x, (int)cell.y); }
        last_cell = cell;
    }

    private void hover(Vector2? cell)
    {
        if (cell == hovered_cell) { return; }
        hovered_cell = cell;
        if (stamp != null) { Update(); }
        EmitSignal(nameof(CellHovered), (int)(cell?.x ?? -1), (int)(cell?.y ?? -1));
    }

    private void _on_ScreenView_mouse_exited()
    {
        cursor.Visible = false;
        hover(null);
    }
}
