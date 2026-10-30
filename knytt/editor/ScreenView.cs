using Godot;
using YKnyttLib;

public class ScreenView : Control
{
    [Signal] public delegate void StrokeStarted();
    [Signal] public delegate void CellPainted(int x, int y);
    [Signal] public delegate void StrokeEnded();
    [Signal] public delegate void CellPicked(int x, int y);
    [Signal] public delegate void CellHovered(int x, int y);
    [Signal] public delegate void CellPlaced(int x, int y);

    [Export] public Texture JuniTexture { get; set; }

    public static readonly Vector2 Offset = new Vector2(25, 24);

    public const int LAYERS = KnyttArea.AREA_TILE_LAYERS + KnyttArea.AREA_SPRITE_LAYERS;

    private static readonly Vector2 TILE = new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
    private static readonly Color NEIGHBOUR_SHADE = new Color(0, 0, 0, 0.45f);
    private static readonly Color START_COLOR = new Color(1, 1, 0);
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
    private EditorAssets assets;
    private KnyttPoint coords;
    private int background;
    private KnyttPoint start_area;
    private KnyttPoint start_position;
    private TextureRect cursor;
    private TextureRect brush;
    private Vector2? last_cell;
    private Vector2? hovered_cell;
    private Vector2? touch_cell;
    private ulong touch_time;

    public override void _Ready()
    {
        cursor = GetNode<TextureRect>("Cursor");
        brush = GetNode<TextureRect>("Cursor/Brush");
    }

    public void init(KnyttWorld world, EditorAssets assets)
    {
        this.world = world;
        this.assets = assets;
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
                drawArea(world.getArea(coords + new KnyttPoint(x, y)), Offset + new Vector2(x, y) * GDKnyttArea.Size, neighbour: true);
            }
        }

        var area = world.getArea(coords);
        if (area == null || area.Empty) { drawGradient(background, Offset); }
        else { drawArea(area, Offset, neighbour: false); }

        var end = Offset + GDKnyttArea.Size;
        DrawRect(new Rect2(0, 0, RectSize.x, Offset.y), NEIGHBOUR_SHADE);
        DrawRect(new Rect2(0, end.y, RectSize.x, RectSize.y - end.y), NEIGHBOUR_SHADE);
        DrawRect(new Rect2(0, Offset.y, Offset.x, GDKnyttArea.Height), NEIGHBOUR_SHADE);
        DrawRect(new Rect2(end.x, Offset.y, RectSize.x - end.x, GDKnyttArea.Height), NEIGHBOUR_SHADE);

        if (coords.Equals(start_area))
        {
            var position = Offset + new Vector2(start_position.x, start_position.y) * TILE;
            DrawTextureRectRegion(JuniTexture, new Rect2(position, TILE), new Rect2(Vector2.Zero, TILE));
            drawFrame(new Rect2(position, TILE), START_COLOR);
        }
    }

    private void drawArea(KnyttArea area, Vector2 origin, bool neighbour)
    {
        if (area == null || area.Empty) { return; }
        drawGradient(area.Background, origin);
        drawTiles(area, origin, neighbour);
        drawObjects(area, origin, neighbour);
    }

    private void drawGradient(int num, Vector2 origin)
    {
        var texture = assets.getGradient(num);
        if (texture == null) { return; }

        int width = texture.GetWidth();
        for (int x = 0; x < GDKnyttArea.Width; x += width)
        {
            var size = new Vector2(Mathf.Min(width, GDKnyttArea.Width - x), GDKnyttArea.Height);
            DrawTextureRectRegion(texture, new Rect2(origin + new Vector2(x, 0), size), new Rect2(Vector2.Zero, size));
        }
    }

    private void drawTiles(KnyttArea area, Vector2 origin, bool neighbour)
    {
        // KnyttArea.TilesetB holds tiles 0-127, which the editor calls tileset A
        var tileset_a = assets.getTileset(area.TilesetB);
        var tileset_b = assets.getTileset(area.TilesetA);

        for (int layer = 0; layer < KnyttArea.AREA_TILE_LAYERS; layer++)
        {
            var color = new Color(1, 1, 1, layerAlpha(layer, neighbour));
            if (color.a == 0) { continue; }

            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    int tile = area.TileLayers[layer].getTile(x, y);
                    var texture = tile < 128 ? tileset_a : tileset_b;
                    if (tile % 128 == 0 || texture == null) { continue; }

                    var source = new Vector2(tile % 128 % GDKnyttAssetManager.TILESET_WIDTH, tile % 128 / GDKnyttAssetManager.TILESET_WIDTH);
                    DrawTextureRectRegion(texture, new Rect2(origin + new Vector2(x, y) * TILE, TILE), new Rect2(source * TILE, TILE), color);
                }
            }
        }
    }

    private void drawObjects(KnyttArea area, Vector2 origin, bool neighbour)
    {
        for (int layer = 0; layer < KnyttArea.AREA_SPRITE_LAYERS; layer++)
        {
            var color = new Color(1, 1, 1, layerAlpha(KnyttArea.AREA_TILE_LAYERS + layer, neighbour));
            if (color.a == 0) { continue; }

            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var id = area.ObjectLayers[layer].getObjectID(x, y);
                    if (id.y == 0) { continue; }

                    var texture = assets.getObject(id);
                    if (texture != null) { DrawTexture(texture, origin + new Vector2(x, y) * TILE, color); }
                }
            }
        }
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

    // Touch has no right button: a tap paints, a drag paints a stroke, a long press picks
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
                if (button.Device == TOUCH)
                {
                    touch_cell = cell;
                    touch_time = OS.GetTicksMsec();
                    return;
                }
                startStroke(cell.Value);
            }
            else if (button.ButtonIndex == (int)ButtonList.Left && !button.Pressed)
            {
                if (touch_cell != null)
                {
                    var start = touch_cell.Value;
                    touch_cell = null;
                    if (OS.GetTicksMsec() - touch_time >= LONG_PRESS_MS) { pick(start); return; }
                    startStroke(start);
                }
                if (last_cell != null)
                {
                    last_cell = null;
                    EmitSignal(nameof(StrokeEnded));
                }
            }
            else if (button.ButtonIndex == (int)ButtonList.Right && button.Pressed && cell != null)
            {
                pick(cell.Value);
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var cell = cellAt(motion.Position);
            cursor.Visible = cell != null && (Painting || Placing);
            hover(cell);
            if (cell == null) { return; }
            cursor.RectPosition = Offset + cell.Value * TILE;

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

    private void pick(Vector2 cell)
    {
        EmitSignal(nameof(CellPicked), (int)cell.x, (int)cell.y);
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
        EmitSignal(nameof(CellHovered), (int)(cell?.x ?? -1), (int)(cell?.y ?? -1));
    }

    private void _on_ScreenView_mouse_exited()
    {
        cursor.Visible = false;
        hover(null);
    }
}
