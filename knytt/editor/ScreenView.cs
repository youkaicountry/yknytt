using Godot;
using YKnyttLib;

public class ScreenView : Control
{
    [Signal] public delegate void StrokeStarted();
    [Signal] public delegate void CellPainted(int x, int y);
    [Signal] public delegate void StrokeEnded();
    [Signal] public delegate void CellPicked(int x, int y);

    [Export] public Texture JuniTexture { get; set; }

    public static readonly Vector2 Offset = new Vector2(25, 24);

    private static readonly Vector2 TILE = new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
    private static readonly Color EDGE_COLOR = Color.Color8(102, 102, 102);
    private static readonly Color START_COLOR = new Color(1, 1, 0);

    private KnyttWorld world;
    private EditorAssets assets;
    private KnyttPoint coords;
    private int background;
    private KnyttPoint start_area;
    private KnyttPoint start_position;
    private TextureRect cursor;
    private Vector2? last_cell;

    public override void _Ready()
    {
        cursor = GetNode<TextureRect>("Cursor");
    }

    public void init(KnyttWorld world, EditorAssets assets, KnyttPoint start_area, KnyttPoint start_position)
    {
        this.world = world;
        this.assets = assets;
        this.start_area = start_area;
        this.start_position = start_position;
    }

    public void showScreen(KnyttPoint coords, int background)
    {
        this.coords = coords;
        this.background = background;
        Update();
    }

    public override void _Draw()
    {
        if (world == null) { return; }

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) { continue; }
                drawArea(world.getArea(coords + new KnyttPoint(x, y)), Offset + new Vector2(x, y) * GDKnyttArea.Size);
            }
        }

        var area = world.getArea(coords);
        if (area == null || area.Empty) { drawGradient(background, Offset); }
        else { drawArea(area, Offset); }

        drawFrame(new Rect2(Offset - Vector2.One, GDKnyttArea.Size + 2 * Vector2.One), EDGE_COLOR);

        if (coords.Equals(start_area))
        {
            var position = Offset + new Vector2(start_position.x, start_position.y) * TILE;
            DrawTextureRectRegion(JuniTexture, new Rect2(position, TILE), new Rect2(Vector2.Zero, TILE));
            drawFrame(new Rect2(position, TILE), START_COLOR);
        }
    }

    private void drawArea(KnyttArea area, Vector2 origin)
    {
        if (area == null || area.Empty) { return; }
        drawGradient(area.Background, origin);
        drawTiles(area, origin);
        drawObjects(area, origin);
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

    private void drawTiles(KnyttArea area, Vector2 origin)
    {
        // KnyttArea.TilesetB holds tiles 0-127, which the editor calls tileset A
        var tileset_a = assets.getTileset(area.TilesetB);
        var tileset_b = assets.getTileset(area.TilesetA);

        foreach (var layer in area.TileLayers)
        {
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    int tile = layer.getTile(x, y);
                    var texture = tile < 128 ? tileset_a : tileset_b;
                    if (tile % 128 == 0 || texture == null) { continue; }

                    var source = new Vector2(tile % 128 % GDKnyttAssetManager.TILESET_WIDTH, tile % 128 / GDKnyttAssetManager.TILESET_WIDTH);
                    DrawTextureRectRegion(texture, new Rect2(origin + new Vector2(x, y) * TILE, TILE), new Rect2(source * TILE, TILE));
                }
            }
        }
    }

    private void drawObjects(KnyttArea area, Vector2 origin)
    {
        foreach (var layer in area.ObjectLayers)
        {
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var id = layer.getObjectID(x, y);
                    if (id.y == 0) { continue; }

                    var texture = assets.getObject(id);
                    if (texture != null) { DrawTexture(texture, origin + new Vector2(x, y) * TILE); }
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

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            var cell = cellAt(button.Position);
            if (button.ButtonIndex == (int)ButtonList.Left && button.Pressed && cell != null)
            {
                EmitSignal(nameof(StrokeStarted));
                paint(cell.Value);
            }
            else if (button.ButtonIndex == (int)ButtonList.Left && !button.Pressed && last_cell != null)
            {
                last_cell = null;
                EmitSignal(nameof(StrokeEnded));
            }
            else if (button.ButtonIndex == (int)ButtonList.Right && button.Pressed && cell != null)
            {
                EmitSignal(nameof(CellPicked), (int)cell.Value.x, (int)cell.Value.y);
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var cell = cellAt(motion.Position);
            cursor.Visible = cell != null;
            if (cell == null) { return; }
            cursor.RectPosition = Offset + cell.Value * TILE;
            if (last_cell != null && cell != last_cell) { paint(cell.Value); }
        }
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

    private void _on_ScreenView_mouse_exited()
    {
        cursor.Visible = false;
    }
}
