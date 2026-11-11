using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using YKnyttLib;
using static YKnyttLib.KnyttSwitch;

// The whole level at once: pan, zoom, colour screens by what they use, see where warps and shifts lead
public class WorldView : Control
{
    [Signal] public delegate void ScreenChosen(int x, int y);
    [Signal] public delegate void ScreenMenu(int x, int y, int cell_x, int cell_y);
    [Signal] public delegate void ThingChosen(int x, int y, int bank, int obj, string key);

    [Export] public Texture JuniTexture { get; set; }

    private enum ColourBy { None, Music, Ambiance, Tilesets, MapColour }

    private static readonly Vector2 SIZE = AreaPainter.SIZE;
    private static readonly Vector2 TILE = AreaPainter.TILE;
    private const float MAX_ZOOM = 2;
    private const float ZOOM_STEP = 1.25f;
    private const float DETAIL_ZOOM = 0.5f;     // from here screens are drawn tile by tile
    private const float SNAP_SCREEN = 60;       // screens drawn smaller than this link centre to centre
    private const float FAN_TILE = 8;           // tiles drawn this big or bigger show every shift of a screen, not one for all
    private const int THUMB_DIVISOR = 4;        // further out they're quarter size pictures
    private const int THUMB_BUDGET_MS = 6;
    private const float SPECK = 6;              // screens narrower than this are their picture's average colour
    private const float DRAG_START = 4;
    private const float SCROLL_SPEED = 700;     // view pixels a second, three times that with Shift
    private const float LABEL_WIDTH = 44;
    private const float ICON_SIZE = 12;
    private const int TOUCH = -1;
    private const ulong LONG_PRESS_MS = 500;

    private static readonly Color BACKGROUND = Color.Color8(58, 102, 135);
    private static readonly Color GRID = Color.Color8(48, 85, 112);
    private static readonly Color CURRENT = Colors.White;
    private static readonly Color HOVER = new Color(1, 1, 1, 0.6f);
    private static readonly Color START = Color.Color8(247, 195, 79);
    private static readonly Color SHADOW = new Color(0, 0, 0, 0.6f);
    private static readonly Color HIDDEN = new Color(0.1f, 0.1f, 0.12f, 0.75f);
    private static readonly Color PANEL = new Color(1, 1, 1, 0.95f);
    private static readonly Color PANEL_EDGE = new Color(0.6f, 0.6f, 0.6f);
    private static readonly Color INK = Colors.Black;
    private static readonly Color WARP = Color.Color8(90, 220, 255);
    private static readonly Color SHIFT = Color.Color8(247, 195, 79);
    private static readonly Color FLAG_WARP = Color.Color8(200, 140, 255);
    private static readonly Color BROKEN = Color.Color8(255, 80, 80);
    private static readonly Color WASH = new Color(0.35f, 0.35f, 0.38f, 0.75f);
    private static readonly Color ROW_LIT = new Color(0.737f, 0.737f, 0.96f);
    private const float FADED = 0.2f;
    private static readonly Color OFF_TEXT = new Color(0, 0, 0, 0.4f);

    // What the marks show, by object
    private class Kind
    {
        public string name;
        public Color color;
        public string group;
        public int bank, first, last;
        public Kind(string name, Color color, string group, int bank, int first, int last)
        {
            this.name = name; this.color = color; this.group = group; this.bank = bank; this.first = first; this.last = last;
        }
    }

    // The checkboxes, and the kinds of lines under the first
    private const string WARPS = "Warps", PICKUPS = "Pickups", SAVES = "Saves", SIGNS = "Signs";
    private static readonly string[] GROUPS = { WARPS, PICKUPS, SAVES, SIGNS };
    private static readonly (string name, Color color)[] LINK_KINDS = { ("Warp", WARP), ("Shift", SHIFT), ("Flag warp", FLAG_WARP) };

    private static readonly Kind[] KINDS = {
        new Kind("Power", Color.Color8(255, 150, 60), PICKUPS, 0, 3, 10),
        new Kind("Power", Color.Color8(255, 150, 60), PICKUPS, 0, 35, 35),
        new Kind("Red key", Color.Color8(235, 70, 70), PICKUPS, 0, 21, 21),
        new Kind("Yellow key", Color.Color8(240, 220, 60), PICKUPS, 0, 22, 22),
        new Kind("Blue key", Color.Color8(80, 140, 255), PICKUPS, 0, 23, 23),
        new Kind("Purple key", Color.Color8(180, 100, 230), PICKUPS, 0, 24, 24),
        new Kind("Creature", Color.Color8(130, 230, 130), PICKUPS, 19, 1, 50),
        new Kind("Coin", Color.Color8(255, 225, 90), PICKUPS, 19, 51, 150),
        new Kind("Artifact", Color.Color8(120, 220, 255), PICKUPS, 19, 151, 199),
        new Kind("Save", Color.Color8(240, 240, 240), SAVES, 0, 1, 1),
        new Kind("Win", Color.Color8(255, 110, 200), SIGNS, 0, 2, 2),
        new Kind("Sign", Color.Color8(160, 200, 255), SIGNS, 0, 17, 19),
        new Kind("Sign", Color.Color8(160, 200, 255), SIGNS, 0, 29, 31),
        new Kind("Trigger", Color.Color8(255, 140, 110), SIGNS, 0, 32, 34),
        new Kind("Trigger", Color.Color8(255, 140, 110), SIGNS, 0, 42, 47),
        new Kind("Custom object", Color.Color8(235, 130, 215), SIGNS, 254, 1, 255),
        new Kind("Custom object", Color.Color8(235, 130, 215), SIGNS, 255, 1, 255),
    };

    // Every bank and object's kind, looked up straight away: a level can have hundreds of thousands of objects
    private static readonly Kind[,] KIND_OF = kindTable();

    private static Kind[,] kindTable()
    {
        var table = new Kind[256, 256];
        foreach (var kind in KINDS) { for (int obj = kind.first; obj <= kind.last; obj++) { table[kind.bank, obj] = kind; } }
        return table;
    }

    private class Link
    {
        public string kind;
        public KnyttPoint source, target;
        public KnyttPoint? id;      // the object it starts from, or the World.ini key it's made of
        public KnyttPoint cell;
        public List<KnyttPoint> cells;  // every shift it stands for, when there are more than one
        public bool fan;                // they all land on one spot (absolute), else each as far from the first as it is
        public string key = "";
        public Vector2 from, to;
        public Color color;
        public bool dashed;
        public string text;
        public Rect2 bounds;
        public int pass;
    }

    private class Mark
    {
        public KnyttPoint id, area, cell;
        public Vector2 at;
        public Kind kind;
    }

    // A line of the key, and what it lights up on the map when hovered
    private class Row
    {
        public Color color;
        public string text;
        public bool line;
        public Func<KnyttArea, bool> screens;
        public Func<Link, bool> links;
        public Func<Mark, bool> marks;
        public string kind;         // the kind of line or mark it turns on and off when clicked
        public bool Lights => screens != null || links != null || marks != null;
    }

    private KnyttWorld world;
    private EditorAssets assets;
    private ObjectCatalog catalog;
    private AreaPainter painter;
    // A screen's small picture, its average colour, and a stamp of the screen it was made from
    private class Thumb
    {
        public Texture texture;
        public Color average;
        public int stamp;
    }

    private readonly Dictionary<KnyttPoint, Thumb> thumbs = new Dictionary<KnyttPoint, Thumb>();
    private readonly List<Link> links = new List<Link>();
    private readonly List<Mark> marks = new List<Mark>();
    private readonly Dictionary<KnyttPoint, List<Mark>> marks_by_screen = new Dictionary<KnyttPoint, List<Mark>>();

    // Things already drawn on a spot this pass: zoomed out, thousands of the same dot or line land on the same pixels
    private readonly HashSet<(int, int, int, int, int)> drawn = new HashSet<(int, int, int, int, int)>();
    private Rect2 link_view;

    // Lines by the 8x8 screen squares they cross, so a view only looks at its own. Lines across much of the level
    // are always looked at
    private const int LINK_CELL = 8;
    private const int LONG_LINK_CELLS = 64;
    private readonly Dictionary<(int, int), List<Link>> link_cells = new Dictionary<(int, int), List<Link>>();
    private readonly List<Link> long_links = new List<Link>();
    private readonly Dictionary<KnyttPoint, List<Link>> links_by_screen = new Dictionary<KnyttPoint, List<Link>>();
    private readonly Dictionary<KnyttPoint, List<Link>> links_to_screen = new Dictionary<KnyttPoint, List<Link>>();
    private int link_pass;
    private List<Vector2> lit_points = new List<Vector2>();
    private List<KnyttArea> lit_screens = new List<KnyttArea>();
    private List<Mark> lit_marks = new List<Mark>();
    private List<Link> lit_links = new List<Link>();
    private (string, int) lit_points_for;

    // With thousands lit, their glow is made once for the view and only its brightness pulses
    private const int MANY_LIT = 1500;
    private const int BUSY_LINKS = 300;         // more lines than this in view are drawn faint, the hovered screen's in full
    private const float BUSY_FADE = 0.3f;
    private readonly ArrayMesh lit_mesh = new ArrayMesh();
    private (string, Vector2, float, int) lit_mesh_for, arrows_for;
    private Dictionary<(int, int), (Vector2 at, Vector2 way, int count)> arrows = new Dictionary<(int, int), (Vector2, Vector2, int)>();
    private int version;            // counts up when the level or what's shown changes, so the caches above are made again

    // Read once when the map opens: the screens, how the in-game map shows them, what kinds there are
    private List<KnyttArea> areas = new List<KnyttArea>();
    private readonly Dictionary<KnyttPoint, Color> map_colors = new Dictionary<KnyttPoint, Color>();
    private readonly HashSet<KnyttPoint> map_hidden = new HashSet<KnyttPoint>();
    private readonly HashSet<string> kinds = new HashSet<string>();

    // The map is drawn when it changes. What moves or follows the pointer is drawn on the overlay above it, every frame
    // while something pulses, so the screens aren't drawn again for that
    private Control overlay;
    private CanvasItem ink;
    private List<KnyttArea> in_view = new List<KnyttArea>();
    private List<Row> colour_rows = new List<Row>(), key_rows = new List<Row>();
    private Row lit;

    // Lines, arrow heads, dots and rings go into one mesh per layer, drawn in one go: Godot draws its own
    // antialiased lines and circles one by one, which got slow with thousands of them
    private readonly List<Vector2> triangles = new List<Vector2>();
    private readonly List<Color> triangle_colors = new List<Color>();
    private readonly ArrayMesh map_mesh = new ArrayMesh(), overlay_mesh = new ArrayMesh(), arrow_mesh = new ArrayMesh();
    private ulong thumbs_shown_at;
    private bool thumbs_waiting;
    private List<KnyttArea> todo = new List<KnyttArea>();
    private Rect2 todo_view;

    private Vector2 center;
    private float zoom;
    private bool placed;
    private KnyttPoint current, start_area, start_position;
    private KnyttPoint? hovered;
    private Vector2? press_at;
    private int press_button;
    private ulong press_time;
    private bool dragging;
    private readonly Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();

    private ColourBy colour_by;
    private string highlight;
    private Mark hovered_mark;
    private Link hovered_link;
    private Vector2 mouse_at;
    private readonly List<(Rect2 rect, Row row)> legend_hits = new List<(Rect2, Row)>();

    // Kinds turned off, by a checkbox or a click in the key. Saves are everywhere, so they start off
    private readonly HashSet<string> hidden = new HashSet<string> { "Save" };

    private bool Shown(string kind) => !hidden.Contains(kind);
    private bool Shown(Mark mark) => Shown(mark.kind.name);
    private bool Shown(Link link) => Shown(link.kind);

    private static IEnumerable<string> kindsIn(string group) =>
        group == WARPS ? LINK_KINDS.Select(kind => kind.name) : KINDS.Where(kind => kind.group == group).Select(kind => kind.name).Distinct();

    public override void _Ready()
    {
        overlay = GetNode<Control>("Overlay");
    }

    public void init(KnyttWorld world, EditorAssets assets, ObjectCatalog catalog)
    {
        this.world = world;
        this.assets = assets;
        this.catalog = catalog;
        painter = new AreaPainter(assets);
    }

    public void open(KnyttPoint current, KnyttPoint start_area, KnyttPoint start_position)
    {
        this.current = current;
        this.start_area = start_area;
        this.start_position = start_position;
        GetFocusOwner()?.ReleaseFocus();
        touches.Clear();
        todo_view = new Rect2();
        forgetChanged();
        readWorld();
        Show();
        if (!placed) { fit(); placed = true; }
        redraw();
    }

    public void close()
    {
        Hide();
        touches.Clear();
        press_at = null;
    }

    // Camera: world pixels to view pixels and back
    private Vector2 toView(Vector2 at) => (at - center) * zoom + RectSize / 2;
    private Vector2 toWorld(Vector2 at) => (at - RectSize / 2) / zoom + center;
    private static KnyttPoint screenAt(Vector2 at) => new KnyttPoint(Mathf.FloorToInt(at.x / SIZE.x), Mathf.FloorToInt(at.y / SIZE.y));
    private static Vector2 origin(KnyttPoint screen) => new Vector2(screen.x, screen.y) * SIZE;

    private void readWorld()
    {
        readScreens();
        findLinksAndMarks();
        indexLinks();
        version++;
        kinds.Clear();
        kinds.UnionWith(links.Select(link => link.kind));
        kinds.UnionWith(marks.Select(mark => mark.kind.name));
    }

    private void readScreens()
    {
        areas = world.Map.Values.Where(area => area != null && !area.Empty).ToList();
        map_colors.Clear();
        map_hidden.Clear();
        foreach (var area in areas)
        {
            map_colors[area.Position] = MapPanel.areaColor(area);
            if (MapPanel.areaVisible(area) == false) { map_hidden.Add(area.Position); }
        }
    }

    private void redraw()
    {
        Update();
        overlay.Update();
    }

    private float MinZoom => Mathf.Min(fitZoom() / 2, 0.05f);

    private Rect2 WorldBounds
    {
        get
        {
            var bounds = new Rect2(origin(current), SIZE);
            foreach (var area in areas) { bounds = bounds.Merge(new Rect2(origin(area.Position), SIZE)); }
            return bounds;
        }
    }

    // The room between the bars
    private Rect2 MapRect => new Rect2(0, GetNode<Control>("Bar").RectSize.y, RectSize.x, RectSize.y - GetNode<Control>("Bar").RectSize.y - GetNode<Control>("InfoBar").RectSize.y);

    private float fitZoom()
    {
        var bounds = WorldBounds.Grow(SIZE.y);
        return Mathf.Min(MAX_ZOOM, Mathf.Min(MapRect.Size.x / bounds.Size.x, MapRect.Size.y / bounds.Size.y));
    }

    private void fit()
    {
        zoom = fitZoom();
        var middle = MapRect.Position + MapRect.Size / 2;
        center = WorldBounds.Position + WorldBounds.Size / 2 - (middle - RectSize / 2) / zoom;
        redraw();
    }

    private void zoomAt(Vector2 at, float factor)
    {
        var before = toWorld(at);
        zoom = Mathf.Clamp(zoom * factor, MinZoom, MAX_ZOOM);
        center += before - toWorld(at);
        redraw();
    }

    private Rect2 View => new Rect2(toWorld(Vector2.Zero), RectSize / zoom);

    // The lit key line in this view
    private (string, Vector2, float, int) LitKey => (lit.text, center, zoom, version);

    // Slowly between 0 and 1, for what a hovered key line lights up
    private static float Pulse => 0.5f + 0.5f * Mathf.Sin(OS.GetTicksMsec() / 1000f * 6);

    private ulong map_draw_ms;

    public override void _Draw()
    {
        if (world == null) { return; }
        ulong started = OS.GetTicksMsec();
        ink = this;
        ink.DrawRect(new Rect2(Vector2.Zero, RectSize), BACKGROUND);
        drawGrid();

        var view = View;
        in_view = areas.Where(area => view.Intersects(new Rect2(origin(area.Position), SIZE))).ToList();
        (colour_rows, key_rows) = legendRows();
        lit = colour_rows.Concat(key_rows).FirstOrDefault(row => row.Lights && row.text == highlight);
        drawn.Clear();
        foreach (var area in in_view)
        {
            var shown = new Rect2(toView(origin(area.Position)), SIZE * zoom);
            // Smaller than a pixel: one per pixel is enough
            if (shown.Size.x < 1 && !drawn.Add(((int)shown.Position.x, (int)shown.Position.y, 0, 0, 0))) { continue; }
            drawScreen(area, shown);
            if (lit != null && lit.screens?.Invoke(area) != true) { ink.DrawRect(shown, WASH); }
        }

        drawStart();
        link_view = view.Grow(20 / zoom);
        drawn.Clear();
        foreach (var mark in marksIn(in_view)) { if (Shown(mark) && view.HasPoint(mark.at)) { drawMark(mark, 1, fade(lit?.marks?.Invoke(mark) == true)); } }
        var shown_links = linksIn(link_view).Where(Shown).ToList();
        // Only from afar: close up they're few enough screens to read, and grey there just looked wrong
        float busy = TILE.x * zoom < FAN_TILE && shown_links.Count > BUSY_LINKS ? BUSY_FADE : 1;
        foreach (var link in shown_links) { drawLink(link, 2, busy * fade(lit?.links?.Invoke(link) == true)); }
        drawTriangles(map_mesh);
        drawFrame(new Rect2(toView(origin(current)), SIZE * zoom), CURRENT, 2);
        map_draw_ms = OS.GetTicksMsec() - started;
    }

    // Lit things pulse over the map, the key, the tooltip and the hovered screen
    private void _on_Overlay_draw()
    {
        if (world == null) { return; }
        ink = overlay;
        float pulse = Pulse;
        link_view = View.Grow(20 / zoom);
        drawn.Clear();
        if (lit != null && litPoints().Count > MANY_LIT)
        {
            if (lit_mesh_for != LitKey)
            {
                lit_mesh_for = LitKey;
                drawLit(0.5f);
                makeMesh(lit_mesh);
            }
            ink.DrawMesh(lit_mesh, null, null, null, new Color(1, 1, 1, 0.55f + 0.45f * pulse));
        }
        else if (lit != null) { drawLit(pulse); }
        else if (hovered != null) { drawFrame(new Rect2(toView(origin(hovered.Value)), SIZE * zoom), HOVER, 1); }
        if (hovered_mark != null) { drawMark(hovered_mark, 1.3f + 0.4f * pulse, 1); }
        if (lit == null && hovered != null && hovered_link == null && hovered_mark == null) { drawScreenLinks(hovered.Value); }
        if (hovered_link?.cells != null) { drawCells(hovered_link); }
        if (hovered_link != null) { drawLink(hovered_link, 3 + 2 * pulse, 1); }
        drawTriangles(overlay_mesh);

        legend_hits.Clear();
        float x = drawLegendBox(colour_rows, 6, lit);
        drawLegendBox(key_rows, x, lit);
        if (lit != null) { drawEdgeArrows(); }
        drawTooltip();
    }

    // Where the hovered screen sends Juni and where she comes from
    private void drawScreenLinks(KnyttPoint screen)
    {
        var from_here = links_by_screen.TryGetValue(screen, out var a) ? a : new List<Link>();
        var to_here = links_to_screen.TryGetValue(screen, out var b) ? b : new List<Link>();
        foreach (var link in from_here.Concat(to_here)) { if (Shown(link)) { drawLink(link, 2, 1); } }
    }

    // Where the shifts of a hovered arrow are
    private void drawCells(Link link)
    {
        var fill = new Color(link.color, 0.3f);
        foreach (var cell in link.cells)
        {
            var at = toView(origin(link.source) + new Vector2(cell.x, cell.y) * TILE);
            ink.DrawRect(new Rect2(at, TILE * zoom), fill);
        }
    }

    // What the lit key line matches, in view, as big as the pulse makes it
    private void drawLit(float pulse)
    {
        litPoints();
        var view = View;
        foreach (var area in lit_screens)
        {
            if (!view.Intersects(new Rect2(origin(area.Position), SIZE))) { continue; }
            var shown = new Rect2(toView(origin(area.Position)), SIZE * zoom);
            var fill = new Color(1, 1, 1, 0.1f + 0.25f * pulse);
            quad(shown.Position, new Vector2(shown.End.x, shown.Position.y), shown.End, new Vector2(shown.Position.x, shown.End.y), fill, fill);
            drawFrame(shown, Colors.White, 2);
        }
        foreach (var mark in lit_marks) { if (view.HasPoint(mark.at)) { drawMark(mark, 1.3f + 0.4f * pulse, 1); } }
        foreach (var link in lit_links) { drawLink(link, 3 + 2 * pulse, 1); }
    }

    // Faded when something else is lit
    private float fade(bool matches) => lit != null && !matches ? FADED : 1;

    // Lines between screens once they're big enough to tell apart
    private void drawGrid()
    {
        if (SIZE.x * zoom < 12) { return; }
        var first = screenAt(toWorld(Vector2.Zero));
        var last = screenAt(toWorld(RectSize));
        for (int x = first.x; x <= last.x + 1; x++)
        {
            float at = toView(origin(new KnyttPoint(x, 0))).x;
            ink.DrawRect(new Rect2(at, 0, 1, RectSize.y), GRID);
        }
        for (int y = first.y; y <= last.y + 1; y++)
        {
            float at = toView(origin(new KnyttPoint(0, y))).y;
            ink.DrawRect(new Rect2(0, at, RectSize.x, 1), GRID);
        }
    }

    // Tile by tile up close, small pictures further out, the map colour until a picture is made
    private void drawScreen(KnyttArea area, Rect2 rect)
    {
        bool pictured = true;
        if (zoom >= DETAIL_ZOOM)
        {
            ink.DrawSetTransform(rect.Position, 0, new Vector2(zoom, zoom));
            painter.drawArea(ink, area, Vector2.Zero, layer => 1);
            ink.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        }
        // A picture that small is its average colour anyway, and plain rects are drawn together, pictures one by one
        else if (thumbs.TryGetValue(area.Position, out var thumb) && rect.Size.x < SPECK) { ink.DrawRect(rect, thumb.average); }
        else if (thumb != null) { ink.DrawTextureRect(thumb.texture, rect, false); }
        else
        {
            ink.DrawRect(rect, map_colors[area.Position]);
            pictured = false;
        }
        drawColouring(area, rect, pictured);
    }

    private void drawColouring(KnyttArea area, Rect2 rect, bool pictured)
    {
        float alpha = pictured ? 0.55f : 1;
        var top = new Rect2(rect.Position, new Vector2(rect.Size.x, rect.Size.y / 2));
        var bottom = new Rect2(rect.Position + new Vector2(0, rect.Size.y / 2), top.Size);
        string text = null;
        switch (colour_by)
        {
            case ColourBy.Music:
                int music = ScreenValues.get(area, ScreenValue.Music);
                ink.DrawRect(rect, numberColor(music, alpha));
                text = music.ToString();
                break;
            case ColourBy.Ambiance:
            case ColourBy.Tilesets:
                var (a, b) = colour_by == ColourBy.Ambiance ? (ScreenValue.AmbianceA, ScreenValue.AmbianceB) : (ScreenValue.TilesetA, ScreenValue.TilesetB);
                ink.DrawRect(top, numberColor(ScreenValues.get(area, a), alpha));
                ink.DrawRect(bottom, numberColor(ScreenValues.get(area, b), alpha));
                text = $"{ScreenValues.get(area, a)} / {ScreenValues.get(area, b)}";
                break;
            case ColourBy.MapColour:
                ink.DrawRect(rect, map_colors[area.Position]);
                if (map_hidden.Contains(area.Position))
                {
                    ink.DrawRect(rect, HIDDEN);
                    ink.DrawLine(rect.Position, rect.End, SHADOW);
                    ink.DrawLine(new Vector2(rect.End.x, rect.Position.y), new Vector2(rect.Position.x, rect.End.y), SHADOW);
                }
                break;
        }
        if (text != null && rect.Size.x >= LABEL_WIDTH) { drawText(text, rect.Position + rect.Size / 2, Colors.White, centered: true); }
    }

    private static Color numberColor(int number, float alpha) =>
        number == 0 ? new Color(0.27f, 0.27f, 0.3f, alpha) : Color.FromHsv(number * 0.618034f % 1, 0.55f, 0.95f, alpha);

    private void drawStart()
    {
        var rect = new Rect2(toView(origin(start_area) + new Vector2(start_position.x, start_position.y) * TILE), TILE * zoom);
        if (rect.Size.x < ICON_SIZE) { rect = new Rect2(rect.Position + rect.Size / 2 - new Vector2(ICON_SIZE, ICON_SIZE) / 2, ICON_SIZE, ICON_SIZE); }
        ink.DrawTextureRectRegion(JuniTexture, rect, new Rect2(Vector2.Zero, TILE));
        drawFrame(rect, START, 1);
    }

    // A ring round the object when it's big enough to see, a dot when it isn't
    private void drawMark(Mark mark, float grow, float alpha)
    {
        float size = TILE.x * zoom;
        var color = new Color(mark.kind.color, alpha);
        var shadow = new Color(SHADOW, SHADOW.a * alpha);
        var at = toView(mark.at);
        if (!drawn.Add(((int)at.x / 2, (int)at.y / 2, -1, (int)(grow * 10), color.ToRgba32()))) { return; }
        if (size >= ICON_SIZE)
        {
            ring(at, size * 0.75f * grow, 4, shadow);
            ring(at, size * 0.75f * grow, 2, color);
        }
        else
        {
            dot(at, 4 * grow, shadow);
            dot(at, 3 * grow, color);
        }
    }

    // A circle line with soft edges, like the lines
    private void ring(Vector2 at, float radius, float width, Color color)
    {
        const int SIDES = 24;
        float inside = radius - width / 2, outside = radius + width / 2;
        var clear = new Color(color, 0);
        for (int i = 0; i < SIDES; i++)
        {
            var a = Vector2.Right.Rotated(Mathf.Tau * i / SIDES);
            var b = Vector2.Right.Rotated(Mathf.Tau * (i + 1) / SIDES);
            quad(at + a * inside, at + b * inside, at + b * outside, at + a * outside, color, color);
            quad(at + a * outside, at + b * outside, at + b * (outside + 1), at + a * (outside + 1), color, clear);
            quad(at + a * inside, at + b * inside, at + b * (inside - 1), at + a * (inside - 1), color, clear);
        }
    }

    // Only the part in view is drawn, so a long line far out of view costs nothing
    private void drawLink(Link link, float width, float alpha)
    {
        if (!link_view.Intersects(link.bounds)) { return; }
        if (fanned(link))
        {
            // Thinner: each is one of many
            foreach (var (from, to) in fanOf(link)) { drawArrow(link, toView(from), toView(to), width, alpha, 0.5f); }
            return;
        }
        var (start, end) = ends(link);
        drawArrow(link, start, end, width, alpha);
    }

    private void drawArrow(Link link, Vector2 from, Vector2 to, float width, float alpha, float scale = 1)
    {
        if ((to - from).Length() < 2) { return; }
        // Smaller from afar, or a level full of them is all heads
        scale *= Mathf.Clamp(SIZE.x * zoom / 200, 0.5f, 1);
        width *= scale;
        if (!drawn.Add(((int)from.x / 2, (int)from.y / 2, (int)to.x / 2, (int)to.y / 2, link.color.ToRgba32() ^ (int)(alpha * 255) ^ (int)width))) { return; }
        var color = new Color(link.color, alpha);
        var shadow = new Color(SHADOW, SHADOW.a * alpha);

        // The shaft stops where the head starts: past it, it showed beside the tip and through the head
        float length = (to - from).Length();
        var way = (to - from) / length;
        var side = new Vector2(-way.y, way.x);
        float head = Mathf.Min((8 + width) * scale, length / 2);
        var neck = to - way * head;
        drawLine(from, neck, shadow, width + 2 * scale, link.dashed);
        // The head's shadow is as much wider all round as the shaft's: its sides are at 26.6 degrees
        triangle(to + way * 2.24f * scale, neck + side * (head / 2 + 1.12f * scale), neck - side * (head / 2 + 1.12f * scale), shadow, shadow, shadow);
        drawLine(from, neck, color, width, link.dashed);
        dot(from, 3 * scale, color);
        triangle(to, neck + side * head / 2, neck - side * head / 2, color, color, color);
    }

    // Where a link is drawn, in view pixels. Far out where it starts and lands on a screen can't be told apart, and
    // screen to screen they're one line (both heads when it goes both ways), not a carpet of them
    private (Vector2 from, Vector2 to) ends(Link link)
    {
        if (SIZE.x * zoom >= SNAP_SCREEN) { return (toView(link.from), toView(link.to)); }
        return (toView(origin(link.source) + SIZE / 2), toView(origin(link.target) + SIZE / 2));
    }

    private void triangle(Vector2 a, Vector2 b, Vector2 c, Color color_a, Color color_b, Color color_c)
    {
        triangles.Add(a); triangles.Add(b); triangles.Add(c);
        triangle_colors.Add(color_a); triangle_colors.Add(color_b); triangle_colors.Add(color_c);
    }

    private void quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color_ab, Color color_cd)
    {
        triangle(a, b, c, color_ab, color_ab, color_cd);
        triangle(a, c, d, color_ab, color_cd, color_cd);
    }

    // Sides by size: a few pixels across, six look as round as more
    private void dot(Vector2 at, float radius, Color color)
    {
        int sides = Mathf.Clamp(Mathf.RoundToInt(radius * 2), 6, 24);
        for (int i = 0; i < sides; i++)
        {
            var a = at + Vector2.Right.Rotated(Mathf.Tau * i / sides) * radius;
            var b = at + Vector2.Right.Rotated(Mathf.Tau * (i + 1) / sides) * radius;
            triangle(at, a, b, color, color, color);
        }
    }

    private void drawTriangles(ArrayMesh mesh)
    {
        if (makeMesh(mesh)) { ink.DrawMesh(mesh, null); }
    }

    private bool makeMesh(ArrayMesh mesh)
    {
        mesh.ClearSurfaces();
        if (triangles.Count == 0) { return false; }
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = triangles.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Color] = triangle_colors.ToArray();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        triangles.Clear();
        triangle_colors.Clear();
        return true;
    }

    // The visible stretch of a line, as how far along it starts and ends (0 to 1), or null when none is
    private (float, float)? clip(Vector2 from, Vector2 to)
    {
        var box = new Rect2(Vector2.Zero, RectSize).Grow(20);
        float start = 0, end = 1;
        var d = to - from;
        foreach (var (p, q) in new[] { (-d.x, from.x - box.Position.x), (d.x, box.End.x - from.x), (-d.y, from.y - box.Position.y), (d.y, box.End.y - from.y) })
        {
            if (p == 0) { if (q < 0) { return null; } continue; }
            float t = q / p;
            if (p < 0) { start = Mathf.Max(start, t); } else { end = Mathf.Min(end, t); }
            if (start > end) { return null; }
        }
        return (start, end);
    }

    // Dashes are counted from the start of the whole line, so they stay put while the view moves
    private void drawLine(Vector2 from, Vector2 to, Color color, float width, bool dashed)
    {
        var visible = clip(from, to);
        if (visible == null) { return; }
        var (start, end) = visible.Value;
        float length = (to - from).Length();
        var step = (to - from) / length;
        if (!dashed) { drawSmooth(from + step * length * start, from + step * length * end, color, width); return; }
        for (float at = Mathf.Floor(start * length / 10) * 10; at < end * length; at += 10)
        {
            drawSmooth(from + step * at, from + step * Mathf.Min(at + 6, length), color, width);
        }
    }

    // An antialiased line made of triangles: a solid middle and edges fading out. Godot draws its own antialiased
    // lines one by one, which got slow with many arrows
    private void drawSmooth(Vector2 from, Vector2 to, Color color, float width)
    {
        var along = to - from;
        if (along.LengthSquared() < 0.01f) { return; }
        var across = new Vector2(-along.y, along.x).Normalized();
        var solid = across * width / 2;
        var edge = across * (width / 2 + 1);
        var clear = new Color(color, 0);
        quad(from + solid, to + solid, to - solid, from - solid, color, color);
        quad(from + solid, to + solid, to + edge, from + edge, color, clear);
        quad(from - solid, to - solid, to - edge, from - edge, color, clear);
    }

    // Four strips: Godot's own outline is lines inset by half their width, which vanish on a screen a few pixels big
    private void drawFrame(Rect2 rect, Color color, float width)
    {
        width = Mathf.Min(width, Mathf.Min(rect.Size.x, rect.Size.y) / 2);
        ink.DrawRect(new Rect2(rect.Position, rect.Size.x, width), color);
        ink.DrawRect(new Rect2(rect.Position.x, rect.End.y - width, rect.Size.x, width), color);
        ink.DrawRect(new Rect2(rect.Position.x, rect.Position.y + width, width, rect.Size.y - width * 2), color);
        ink.DrawRect(new Rect2(rect.End.x - width, rect.Position.y + width, width, rect.Size.y - width * 2), color);
    }

    // On the map words get a shadow; on the white panels they don't
    private void drawText(string text, Vector2 at, Color color, bool centered = false, bool shadow = true)
    {
        var font = GetFont("font");
        if (centered) { at -= new Vector2(font.GetStringSize(text).x / 2, -font.GetAscent() / 2 + 2); }
        if (shadow) { ink.DrawString(font, at + Vector2.One, text, SHADOW); }
        ink.DrawString(font, at, text, color);
    }

    // What a lit key line matches out of view, as arrows on the edge pointing at it. Close ones share an arrow
    private void drawEdgeArrows()
    {
        if (arrows_for != LitKey)
        {
            arrows_for = LitKey;
            placeArrows();
        }
        foreach (var (at, way, count) in arrows.Values)
        {
            var tip = at + way * (2 + 3 * Pulse);
            var side = new Vector2(-way.y, way.x);
            triangle(tip + way * 2, tip - way * 11 + side * 8, tip - way * 11 - side * 8, SHADOW, SHADOW, SHADOW);
            var color = new Color(lit.color, 1);
            triangle(tip, tip - way * 9 + side * 6, tip - way * 9 - side * 6, color, color, color);
        }
        drawTriangles(arrow_mesh);
        foreach (var (at, way, count) in arrows.Values)
        {
            if (count > 1) { drawText(count.ToString(), at + way * (2 + 3 * Pulse) - way * 22, Colors.White, centered: true); }
        }
    }

    private void placeArrows()
    {
        var inner = MapRect.Grow(-14);
        var middle = inner.Position + inner.Size / 2;
        var key = legend_hits.Select(hit => hit.rect).Aggregate((Rect2?)null, (all, rect) => all?.Merge(rect) ?? rect)?.Grow(8);
        arrows = new Dictionary<(int, int), (Vector2 at, Vector2 way, int count)>();
        foreach (var point in litPoints())
        {
            var at = toView(point);
            if (float.IsNaN(at.x) || float.IsNaN(at.y) || MapRect.HasPoint(at)) { continue; }
            var away = at - middle;
            float reach = Mathf.Min(away.x == 0 ? float.MaxValue : inner.Size.x / 2 / Mathf.Abs(away.x), away.y == 0 ? float.MaxValue : inner.Size.y / 2 / Mathf.Abs(away.y));
            var edge = middle + away * reach;
            // Not over the key: above it on the left edge, past it on the bottom
            if (key?.HasPoint(edge) == true)
            {
                if (Mathf.Abs(edge.x - inner.Position.x) < 1) { edge.y = key.Value.Position.y; } else { edge.x = key.Value.End.x; }
            }
            var spot = (Mathf.RoundToInt(edge.x / 20), Mathf.RoundToInt(edge.y / 20));
            arrows[spot] = arrows.TryGetValue(spot, out var arrow) ? (arrow.at, arrow.way, arrow.count + 1) : (edge, away.Normalized(), 1);
        }
    }

    // Everything the lit line matches, found again only when another line lights up
    private List<Vector2> litPoints()
    {
        if (lit_points_for == (lit.text, version)) { return lit_points; }
        lit_points_for = (lit.text, version);
        lit_screens = lit.screens == null ? new List<KnyttArea>() : areas.Where(lit.screens).ToList();
        lit_links = lit.links == null ? new List<Link>() : links.Where(link => Shown(link) && lit.links(link)).ToList();
        lit_marks = lit.marks == null ? new List<Mark>() : marks.Where(mark => Shown(mark) && lit.marks(mark)).ToList();
        lit_points = lit_screens.Select(area => origin(area.Position) + SIZE / 2)
            .Concat(lit_links.Select(link => link.from)).Concat(lit_marks.Select(mark => mark.at)).ToList();
        return lit_points;
    }

    // What's under the pointer, right by it
    private void drawTooltip()
    {
        var lines = new List<string>();
        if (hovered_mark != null)
        {
            lines.Add(catalog.title(hovered_mark.id));
            lines.Add($"{hovered_mark.kind.name}, x{hovered_mark.area.x}y{hovered_mark.area.y} at {hovered_mark.cell.x}, {hovered_mark.cell.y}");
        }
        else if (hovered_link != null) { lines.Add($"x{hovered_link.source.x}y{hovered_link.source.y}: {hovered_link.text}"); }
        if (lines.Count == 0) { return; }

        const float ROW = 15;
        var font = GetFont("font");
        var size = new Vector2(lines.Max(line => font.GetStringSize(line).x) + 12, lines.Count * ROW + 6);
        var at = mouse_at + new Vector2(16, 16);
        at = new Vector2(Mathf.Min(at.x, RectSize.x - size.x - 4), Mathf.Min(at.y, MapRect.End.y - size.y - 4));
        ink.DrawRect(new Rect2(at, size), PANEL);
        drawFrame(new Rect2(at, size), PANEL_EDGE, 1);
        for (int i = 0; i < lines.Count; i++) { drawText(lines[i], at + new Vector2(6, 15 + i * ROW), i == 0 ? INK : OFF_TEXT, shadow: false); }
    }

    // The key: what the colours mean for the screens in view, and what the lines and marks are
    private (List<Row> colours, List<Row> keys) legendRows()
    {
        var colours = new List<Row>();
        if (colour_by == ColourBy.Music || colour_by == ColourBy.Ambiance || colour_by == ColourBy.Tilesets)
        {
            var values = colour_by == ColourBy.Music ? new[] { ScreenValue.Music } :
                colour_by == ColourBy.Ambiance ? new[] { ScreenValue.AmbianceA, ScreenValue.AmbianceB } : new[] { ScreenValue.TilesetA, ScreenValue.TilesetB };
            var used = in_view.SelectMany(area => values.Select(value => ScreenValues.get(area, value))).Distinct().OrderBy(n => n).ToList();
            string what = colour_by == ColourBy.Music ? "Music" : colour_by == ColourBy.Ambiance ? "Ambiance" : "Tileset";
            if (values.Length == 2) { colours.Add(new Row { text = "A on top, B below" }); }
            foreach (int n in used.Take(12))
            {
                colours.Add(new Row { color = numberColor(n, 1), text = n == 0 ? "None" : $"{what} {n}", screens = area => values.Any(value => ScreenValues.get(area, value) == n) });
            }
            if (used.Count > 12) { colours.Add(new Row { text = $"and {used.Count - 12} more" }); }
        }
        if (colour_by == ColourBy.MapColour && in_view.Any(area => map_hidden.Contains(area.Position)))
        {
            colours.Add(new Row { color = HIDDEN, text = "Hidden on the map", screens = area => map_hidden.Contains(area.Position) });
        }

        // Every kind the level has, off ones too so a click brings them back
        var keys = new List<Row>();
        foreach (var (name, color) in LINK_KINDS.Where(kind => kinds.Contains(kind.name)))
        {
            keys.Add(new Row { color = color, text = name, line = true, kind = name, links = Shown(name) ? link => link.kind == name : (Func<Link, bool>)null });
        }
        if (links.Any(link => link.color == BROKEN && Shown(link)))
        {
            keys.Add(new Row { color = BROKEN, text = "Leads nowhere", line = true, links = link => link.color == BROKEN });
        }
        foreach (var kind in KINDS.Where(k => kinds.Contains(k.name)).GroupBy(k => k.name).Select(g => g.First()))
        {
            keys.Add(new Row { color = kind.color, text = kind.name, kind = kind.name, marks = Shown(kind.name) ? mark => mark.kind.name == kind.name : (Func<Mark, bool>)null });
        }
        return (colours, keys);
    }

    // Bottom left. The line for what's lit or under the pointer stands out
    private float drawLegendBox(List<Row> rows, float x, Row lit)
    {
        if (rows.Count == 0) { return x; }
        const float ROW = 14;
        var font = GetFont("font");
        float width = rows.Max(row => font.GetStringSize(row.text).x) + 30;
        var box = new Rect2(x, MapRect.End.y - 6 - rows.Count * ROW - 6, width, rows.Count * ROW + 6);
        ink.DrawRect(box, PANEL);
        drawFrame(box, PANEL_EDGE, 1);
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var at = box.Position + new Vector2(6, 4 + i * ROW);
            var line_rect = new Rect2(box.Position.x, at.y - 1, width, ROW);
            legend_hits.Add((line_rect, row));
            bool under = row == lit || (hovered_mark != null && row.marks?.Invoke(hovered_mark) == true) || (hovered_link != null && row.links?.Invoke(hovered_link) == true);
            bool off = row.kind != null && !Shown(row.kind);
            if (under) { ink.DrawRect(line_rect, ROW_LIT); }
            // Off: a hollow swatch and grey words
            var swatch = new Rect2(at + new Vector2(1, 1), 10, 10);
            if (row.line && off) { ink.DrawRect(swatch, row.color, false); }
            else if (row.line) { ink.DrawLine(at + new Vector2(0, 6), at + new Vector2(12, 6), row.color, 2); }
            else if (row.color.a > 0)
            {
                ink.DrawRect(swatch, row.color, !off);
                if (!off) { ink.DrawRect(swatch, OFF_TEXT, false); }
            }
            drawText(row.text, at + new Vector2(18, 11), off && !under ? OFF_TEXT : INK, shadow: false);
        }
        return box.End.x + 6;
    }

    // Warps, shifts and flag warps as arrows, collectables and other spots as marks
    private void findLinksAndMarks()
    {
        links.Clear();
        marks.Clear();
        marks_by_screen.Clear();
        var shifts = new List<KnyttPoint>[3];
        foreach (var area in areas)
        {
            KnyttPoint? warp = null;
            for (int id = 0; id < 3; id++) { shifts[id]?.Clear(); }
            foreach (var layer in area.ObjectLayers)
            {
                for (int i = 0; i < layer.hi.Length; i++)
                {
                    int bank = layer.lo[i], obj = layer.hi[i];
                    if (obj == 0) { continue; }
                    var cell = new KnyttPoint(i % KnyttArea.AREA_WIDTH, i / KnyttArea.AREA_WIDTH);
                    if (bank == 0 && obj == 20) { warp = cell; }
                    if (bank == 0 && obj >= 14 && obj <= 16) { (shifts[obj - 14] = shifts[obj - 14] ?? new List<KnyttPoint>()).Add(cell); }
                    var kind = KIND_OF[bank, obj];
                    if (kind == null) { continue; }
                    var at = origin(area.Position) + new Vector2(cell.x, cell.y) * TILE + TILE / 2;
                    var mark = new Mark { id = new KnyttPoint(bank, obj), area = area.Position, cell = cell, at = at, kind = kind };
                    marks.Add(mark);
                    if (!marks_by_screen.TryGetValue(area.Position, out var here)) { marks_by_screen[area.Position] = here = new List<Mark>(); }
                    here.Add(mark);
                }
            }
            if (warp != null) { addWarps(area, warp.Value); }
            for (int id = 0; id < 3; id++) { if (shifts[id]?.Count > 0) { addShift(area, shifts[id], (SwitchID)id); } }
            addFlagWarps(area);
        }
    }

    private (int, int) linkCell(Vector2 at) => (Mathf.FloorToInt(at.x / (SIZE.x * LINK_CELL)), Mathf.FloorToInt(at.y / (SIZE.y * LINK_CELL)));

    private void indexLinks()
    {
        link_cells.Clear();
        long_links.Clear();
        links_by_screen.Clear();
        links_to_screen.Clear();
        foreach (var link in links)
        {
            if (!links_by_screen.TryGetValue(link.source, out var from_here)) { links_by_screen[link.source] = from_here = new List<Link>(); }
            from_here.Add(link);
            if (!links_to_screen.TryGetValue(link.target, out var to_here)) { links_to_screen[link.target] = to_here = new List<Link>(); }
            to_here.Add(link);
            var (x0, y0) = linkCell(link.bounds.Position);
            var (x1, y1) = linkCell(link.bounds.End);
            if ((x1 - x0 + 1) * (y1 - y0 + 1) > LONG_LINK_CELLS) { long_links.Add(link); continue; }
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    if (!link_cells.TryGetValue((x, y), out var cell)) { link_cells[(x, y)] = cell = new List<Link>(); }
                    cell.Add(link);
                }
            }
        }
    }

    // Each line once, the ones that may cross the rect
    private IEnumerable<Link> linksIn(Rect2 rect)
    {
        var (x0, y0) = linkCell(rect.Position);
        var (x1, y1) = linkCell(rect.End);
        if ((long)(x1 - x0 + 1) * (y1 - y0 + 1) > link_cells.Count) { foreach (var link in links) { yield return link; } yield break; }
        link_pass++;
        foreach (var link in long_links) { link.pass = link_pass; yield return link; }
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                if (!link_cells.TryGetValue((x, y), out var cell)) { continue; }
                foreach (var link in cell)
                {
                    if (link.pass == link_pass) { continue; }
                    link.pass = link_pass;
                    yield return link;
                }
            }
        }
    }

    // The marks on screens in view
    private IEnumerable<Mark> marksIn(List<KnyttArea> screens)
    {
        foreach (var area in screens)
        {
            if (!marks_by_screen.TryGetValue(area.Position, out var here)) { continue; }
            foreach (var mark in here) { yield return mark; }
        }
    }

    private static readonly (string name, KnyttPoint step, Vector2 edge)[] SIDES = {
        ("up", new KnyttPoint(0, -1), new Vector2(0.5f, 0)), ("down", new KnyttPoint(0, 1), new Vector2(0.5f, 1)),
        ("left", new KnyttPoint(-1, 0), new Vector2(0, 0.5f)), ("right", new KnyttPoint(1, 0), new Vector2(1, 0.5f)) };

    // Leaving through an edge lands on the far edge of the warped screen
    private void addWarps(KnyttArea area, KnyttPoint cell)
    {
        if (area.ExtraData == null) { return; }
        var warp = new KnyttWarp(area);
        warp.loadWarpData();
        foreach (var (name, step, edge) in SIDES)
        {
            var offset = warp.getWarpCoords(area.Position + step, area.Position);
            if (offset.isZero()) { continue; }
            var target = area.Position + step + offset;
            var link = addLink("Warp", area.Position, origin(area.Position) + edge * SIZE, origin(target) + (Vector2.One - edge) * SIZE,
                target, WARP, false, $"warp {name} to x{target.x}y{target.y}");
            link.id = new KnyttPoint(0, 20);
            link.cell = cell;
        }
    }

    // All of a screen's shifts of one letter share their settings, so they're one arrow from the one nearest their middle
    // (levels fill whole screens with them: The Manor has 56629)
    private void addShift(KnyttArea area, List<KnyttPoint> cells, SwitchID id)
    {
        if (area.ExtraData == null) { return; }
        var middle = cells.Aggregate(Vector2.Zero, (sum, c) => sum + new Vector2(c.x, c.y)) / cells.Count;
        var cell = cells.OrderBy(c => (new Vector2(c.x, c.y) - middle).LengthSquared()).First();
        var shift = new KnyttShift(area, cell, id);
        var target = shift.AbsoluteArea;
        var at = new Vector2(shift.AbsolutePosition.x, shift.AbsolutePosition.y) * TILE + TILE / 2;
        string many = cells.Count > 1 ? $" ({cells.Count} of them)" : "";
        var link = addLink("Shift", area.Position, origin(area.Position) + new Vector2(cell.x, cell.y) * TILE + TILE / 2, origin(target) + at,
            target, SHIFT, false, $"shift {id}{many} to x{target.x}y{target.y}");
        link.id = new KnyttPoint(0, 14 + (int)id);
        link.cell = cell;
        if (cells.Count == 1) { return; }
        link.cells = new List<KnyttPoint>(cells);
        link.fan = shift.AbsoluteTarget;
        foreach (var (from, to) in fanOf(link)) { link.bounds = link.bounds.Expand(from).Expand(to); }
    }

    private bool fanned(Link link) => link.cells != null && TILE.x * zoom >= FAN_TILE;

    // Each of a link's shifts, start and landing in level pixels
    private IEnumerable<(Vector2 from, Vector2 to)> fanOf(Link link)
    {
        foreach (var cell in link.cells)
        {
            var step = new Vector2(cell.x - link.cell.x, cell.y - link.cell.y) * TILE;
            yield return (link.from + step, link.fan ? link.to : link.to + step);
        }
    }

    // Flag warps send Juni on as she comes in, when she has the flag
    private void addFlagWarps(KnyttArea area)
    {
        foreach (KnyttArea.FlagWarpID id in Enum.GetValues(typeof(KnyttArea.FlagWarpID)))
        {
            var flag_warp = area.FlagWarps?[(int)id];
            if (flag_warp == null || flag_warp.xArtifactMode || flag_warp.yArtifactMode) { continue; }
            var target = area.Position + new KnyttPoint(flag_warp.x, flag_warp.y);
            if (target.Equals(area.Position)) { continue; }
            var link = addLink("Flag warp", area.Position, origin(area.Position) + SIZE / 2, origin(target) + SIZE / 2,
                target, FLAG_WARP, true, $"flag warp {id} ({area.getExtraData($"Flag({id})")}) to x{target.x}y{target.y}");
            link.key = $"FlagWarpX({id})";
        }
    }

    private Link addLink(string kind, KnyttPoint source, Vector2 from, Vector2 to, KnyttPoint target, Color color, bool dashed, string text)
    {
        bool lands = world.getArea(target) is KnyttArea area && !area.Empty;
        var bounds = new Rect2(from, Vector2.Zero).Expand(to).Expand(origin(source) + SIZE / 2).Expand(origin(target) + SIZE / 2).Grow(1);
        var link = new Link { bounds = bounds, kind = kind, source = source, target = target, from = from, to = to, color = lands ? color : BROKEN, dashed = dashed, text = lands ? text : text + " (nothing there)" };
        links.Add(link);
        return link;
    }

    public override void _Process(float delta)
    {
        if (!Visible) { return; }
        scroll(delta);
        if (lit != null || hovered_mark != null || hovered_link != null) { overlay.Update(); }
        // Under a pixel a screen is its map colour, a picture wouldn't show
        if (zoom < DETAIL_ZOOM && SIZE.x * zoom >= 1) { makeThumbs(); }
    }

    // WASD and the arrows glide while held
    private void scroll(float delta)
    {
        var direction = Vector2.Zero;
        if (Input.IsKeyPressed((int)KeyList.Left) || Input.IsKeyPressed((int)KeyList.A)) { direction.x -= 1; }
        if (Input.IsKeyPressed((int)KeyList.Right) || Input.IsKeyPressed((int)KeyList.D)) { direction.x += 1; }
        if (Input.IsKeyPressed((int)KeyList.Up) || Input.IsKeyPressed((int)KeyList.W)) { direction.y -= 1; }
        if (Input.IsKeyPressed((int)KeyList.Down) || Input.IsKeyPressed((int)KeyList.S)) { direction.y += 1; }
        if (direction == Vector2.Zero) { return; }

        float speed = SCROLL_SPEED * (Input.IsKeyPressed((int)KeyList.Shift) ? 3 : 1);
        center += direction.Normalized() * speed * delta / zoom;
        redraw();
    }

    // Pictures are made a few at a time, nearest to the middle first
    private void makeThumbs()
    {
        // What's left to make in view, nearest the middle last so it's taken first. Sorted again only when the view moves
        var view = View;
        if (view != todo_view)
        {
            todo_view = view;
            var middle = toWorld(RectSize / 2);
            todo = areas.Where(area => !thumbs.ContainsKey(area.Position) && view.Intersects(new Rect2(origin(area.Position), SIZE)))
                .OrderByDescending(area => (origin(area.Position) + SIZE / 2 - middle).LengthSquared()).ToList();
        }
        if (todo.Count == 0)
        {
            if (thumbs_waiting) { Update(); }
            thumbs_waiting = false;
            return;
        }

        // Specks only need their colour, so they take less of each frame
        ulong until = OS.GetTicksMsec() + (ulong)(SIZE.x * zoom < SPECK ? THUMB_BUDGET_MS / 2 : THUMB_BUDGET_MS);
        while (todo.Count > 0 && OS.GetTicksMsec() <= until)
        {
            var area = todo[todo.Count - 1];
            todo.RemoveAt(todo.Count - 1);
            if (!thumbs.ContainsKey(area.Position)) { makeThumb(area); }
        }
        // Shown about ten times a second while they come in: drawing every screen each frame slowed making them
        thumbs_waiting = true;
        if (OS.GetTicksMsec() - thumbs_shown_at > Math.Max(100, map_draw_ms * 8))
        {
            thumbs_shown_at = OS.GetTicksMsec();
            thumbs_waiting = false;
            Update();
        }
    }

    private void makeThumb(KnyttArea area)
    {
        var image = painter.makeImage(area);
        image.Resize((int)GDKnyttArea.Width / 2, (int)GDKnyttArea.Height / 2);
        image.Resize((int)GDKnyttArea.Width / THUMB_DIVISOR, (int)GDKnyttArea.Height / THUMB_DIVISOR);
        var average = (Image)image.Duplicate();
        while (average.GetWidth() > 1 || average.GetHeight() > 1)
        {
            average.Resize(Mathf.Max(average.GetWidth() / 2, 1), Mathf.Max(average.GetHeight() / 2, 1));
        }
        average.Lock();
        var texture = new ImageTexture();
        texture.CreateFromImage(image, (uint)(Texture.FlagsEnum.Filter | Texture.FlagsEnum.Mipmaps));
        thumbs[area.Position] = new Thumb { texture = texture, average = average.GetPixel(0, 0), stamp = stampOf(area) };
        average.Unlock();
    }

    // Pictures of screens edited since are made again
    private void forgetChanged()
    {
        painter.forgetImages();
        foreach (var position in thumbs.Keys.ToList())
        {
            var area = world.getArea(position);
            if (area == null || area.Empty || thumbs[position].stamp != stampOf(area)) { thumbs.Remove(position); }
        }
    }

    private static int stampOf(KnyttArea area)
    {
        unchecked
        {
            int stamp = area.Background * 31 + area.TilesetA * 961 + area.TilesetB * 29791;
            foreach (var layer in area.TileLayers) { foreach (byte tile in layer.tiles) { stamp = stamp * 31 + tile; } }
            foreach (var layer in area.ObjectLayers)
            {
                foreach (byte b in layer.hi) { stamp = stamp * 31 + b; }
                foreach (byte b in layer.lo) { stamp = stamp * 31 + b; }
            }
            return stamp;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            if (button.Pressed && button.ButtonIndex == (int)ButtonList.WheelUp) { zoomAt(button.Position, ZOOM_STEP); }
            else if (button.Pressed && button.ButtonIndex == (int)ButtonList.WheelDown) { zoomAt(button.Position, 1 / ZOOM_STEP); }
            else if (button.Pressed && button.ButtonIndex == (int)ButtonList.Left && keyRowAt(button.Position)?.kind is string kind)
            {
                toggle(kind);
            }
            else if (button.Pressed)
            {
                press_at = button.Position;
                press_button = button.ButtonIndex;
                press_time = OS.GetTicksMsec();
                dragging = false;
            }
            else if (press_at != null && button.ButtonIndex == press_button)
            {
                if (!dragging) { clicked(button); }
                press_at = null;
            }
            AcceptEvent();
        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (press_at != null && touches.Count < 2)
            {
                dragging |= (motion.Position - press_at.Value).Length() > DRAG_START;
                if (dragging) { center -= motion.Relative / zoom; redraw(); }
            }
            hover(motion.Position);
            AcceptEvent();
        }
        // Trackpads. Android sends one for a pinch too, which the fingers already did
        else if (@event is InputEventMagnifyGesture magnify && touches.Count < 2)
        {
            zoomAt(magnify.Position, magnify.Factor);
            AcceptEvent();
        }
    }

    // One finger comes as the mouse. Fingers are followed here, where every touch and lift arrives, for two finger
    // pinches: zoom by how far they spread, pan with the point between them
    public override void _Input(InputEvent @event)
    {
        if (!Visible) { return; }
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed) { touches[touch.Index] = ((InputEventScreenTouch)MakeInputLocal(touch)).Position; }
            else { touches.Remove(touch.Index); }
            if (touches.Count > 1) { dragging = true; }
        }
        else if (@event is InputEventScreenDrag drag && touches.ContainsKey(drag.Index))
        {
            var at = ((InputEventScreenDrag)MakeInputLocal(drag)).Position;
            if (touches.Count > 1)
            {
                var other = touches.First(t => t.Key != drag.Index).Value;
                var before = touches[drag.Index];
                center -= ((at + other) / 2 - (before + other) / 2) / zoom;
                float spread = (before - other).Length();
                if (spread > 0) { zoomAt((at + other) / 2, (at - other).Length() / spread); }
                redraw();
            }
            touches[drag.Index] = at;
        }
    }

    // Left: go there, and to a mark's or line's form when it has one. Right or a long touch: the cell menu
    private void clicked(InputEventMouseButton button)
    {
        var at = toWorld(button.Position);
        var screen = screenAt(at);
        var spot = ((at - origin(screen)) / TILE).Floor();
        var cell = new KnyttPoint((int)spot.x, (int)spot.y);
        var mark = markAt(button.Position);
        var link = mark == null ? linkAt(button.Position) : null;
        if (mark != null) { (screen, cell) = (mark.area, mark.cell); }
        else if (link?.id != null) { (screen, cell) = (link.source, link.cell); }
        else if (link != null) { screen = link.source; }

        bool long_touch = button.Device == TOUCH && OS.GetTicksMsec() - press_time >= LONG_PRESS_MS;
        if (button.ButtonIndex == (int)ButtonList.Right || long_touch) { EmitSignal(nameof(ScreenMenu), screen.x, screen.y, cell.x, cell.y); }
        else if (button.ButtonIndex != (int)ButtonList.Left) { return; }
        else if (mark != null) { EmitSignal(nameof(ThingChosen), screen.x, screen.y, mark.id.x, mark.id.y, ""); }
        else if (link != null) { EmitSignal(nameof(ThingChosen), screen.x, screen.y, link.id?.x ?? -1, link.id?.y ?? -1, link.key); }
        else { EmitSignal(nameof(ScreenChosen), screen.x, screen.y); }
    }

    // The key first, then marks and lines under the pointer, then the screen
    private void hover(Vector2 position)
    {
        mouse_at = position;
        var row = keyRowAt(position);
        MouseDefaultCursorShape = row?.kind != null ? CursorShape.PointingHand : CursorShape.Arrow;
        string was = highlight;
        highlight = row != null && row.Lights ? row.text : null;
        hovered_mark = row == null ? markAt(position) : null;
        hovered_link = row == null && hovered_mark == null ? linkAt(position) : null;

        var screen = screenAt(toWorld(position));
        if (hovered == null || !hovered.Value.Equals(screen))
        {
            hovered = screen;
            GetNode<Label>("%Info").Text = describe(screen);
        }
        // Lighting a key line fades the rest of the map, the rest only needs the overlay
        if (highlight != was) { redraw(); } else { overlay.Update(); }
    }

    // Only the screens within reach of the pointer are looked at
    private Mark markAt(Vector2 position)
    {
        float reach = Mathf.Max(TILE.x * zoom * 0.75f, 5) + 3;
        var near = new Rect2(toWorld(position), Vector2.Zero).Grow(reach / zoom);
        var first = screenAt(near.Position);
        var last = screenAt(near.End);
        Mark best = null;
        float best_distance = reach;
        for (int x = first.x; x <= last.x; x++)
        {
            for (int y = first.y; y <= last.y; y++)
            {
                if (!marks_by_screen.TryGetValue(new KnyttPoint(x, y), out var here)) { continue; }
                foreach (var mark in here)
                {
                    float d = (toView(mark.at) - position).Length();
                    if (Shown(mark) && d <= best_distance) { best = mark; best_distance = d; }
                }
            }
        }
        return best;
    }

    private Link linkAt(Vector2 position)
    {
        var at = toWorld(position);
        float reach = 5 / zoom;
        Link best = null;
        float best_distance = 5;
        foreach (var link in linksIn(new Rect2(at, Vector2.Zero).Grow(reach)))
        {
            if (!Shown(link) || !link.bounds.Grow(reach).HasPoint(at)) { continue; }
            float d = fanned(link)
                ? fanOf(link).Min(pair => distance(position, toView(pair.from), toView(pair.to)))
                : distance(position, ends(link).from, ends(link).to);
            if (d <= best_distance) { best = link; best_distance = d; }
        }
        return best;
    }

    private static float distance(Vector2 point, Vector2 from, Vector2 to)
    {
        var line = to - from;
        float along = line.LengthSquared() == 0 ? 0 : Mathf.Clamp((point - from).Dot(line) / line.LengthSquared(), 0, 1);
        return (from + line * along - point).Length();
    }

    private void _on_WorldView_mouse_exited()
    {
        hovered = null;
        highlight = null;
        hovered_mark = null;
        hovered_link = null;
        GetNode<Label>("%Info").Text = "";
        redraw();
    }

    private string describe(KnyttPoint screen)
    {
        var area = world.getArea(screen);
        string name = $"x{screen.x}y{screen.y}";
        if (area == null || area.Empty) { return $"{name}   Empty. Click to go there"; }

        var parts = new List<string> {
            name,
            $"Music {area.Song}",
            $"Ambiance {ScreenValues.get(area, ScreenValue.AmbianceA)}/{ScreenValues.get(area, ScreenValue.AmbianceB)}",
            $"Background {area.Background}",
            $"Tilesets {ScreenValues.get(area, ScreenValue.TilesetA)}/{ScreenValues.get(area, ScreenValue.TilesetB)}" };
        var on_screen = marks_by_screen.TryGetValue(screen, out var here) ? here : new List<Mark>();
        var counts = on_screen.GroupBy(mark => mark.kind.name).Select(g => g.Count() == 1 ? g.Key : $"{g.Count()} {g.Key.ToLower()}s");
        if (counts.Any()) { parts.Add(string.Join(", ", counts)); }
        if (links_by_screen.TryGetValue(screen, out var from_here)) { parts.AddRange(from_here.Select(link => link.text)); }
        if (MapPanel.areaVisible(area) == false) { parts.Add("hidden on the map"); }
        return string.Join("   ", parts);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !(@event is InputEventKey key)) { return; }
        GetTree().SetInputAsHandled();
        if (!key.Pressed) { return; }

        var middle = MapRect.Position + MapRect.Size / 2;
        switch ((KeyList)key.Scancode)
        {
            case KeyList.Escape:
            case KeyList.M: close(); break;
            case KeyList.Plus:
            case KeyList.Equal:
            case KeyList.KpAdd: zoomAt(middle, ZOOM_STEP); break;
            case KeyList.Minus:
            case KeyList.KpSubtract: zoomAt(middle, 1 / ZOOM_STEP); break;
            case KeyList.Home: fit(); break;
        }
    }

    private void _on_ColourBy_item_selected(int index)
    {
        colour_by = (ColourBy)index;
        redraw();
    }

    private Row keyRowAt(Vector2 position) => legend_hits.FirstOrDefault(hit => hit.rect.HasPoint(position)).row;

    private void toggle(string kind)
    {
        if (!hidden.Remove(kind)) { hidden.Add(kind); }
        version++;
        showGroups();
        redraw();
    }

    // A checkbox is on while any of its kinds is
    private void showGroups()
    {
        foreach (var group in GROUPS) { GetNode<BaseButton>($"%{group}").SetPressedNoSignal(kindsIn(group).Any(Shown)); }
    }

    private void _on_Group_toggled(bool pressed, string group)
    {
        foreach (var kind in kindsIn(group))
        {
            if (pressed) { hidden.Remove(kind); } else { hidden.Add(kind); }
        }
        version++;
        redraw();
    }

    private void _on_Fit_pressed()
    {
        fit();
    }

    private void _on_Close_pressed()
    {
        close();
    }
}
