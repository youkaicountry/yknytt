using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class MapPanel : Panel
{
    private KnyttWorld world;
    private Juni juni;

    private float XSIZE = 35;
    private float YSIZE = 10;

    private Color VISITED_COLOR = new Color(0.5f, 0.5f, 1);
    private Color NOT_VISITED_COLOR = new Color(0.5f, 0.5f, 0.5f);
    private Color CURRENT_BORDER = new Color(1, 0, 0);

    private float SCROLL_SPEED = 600;
    private float BORDER = 20;

    private Dictionary<KnyttPoint, KnyttPoint> spoofing = new Dictionary<KnyttPoint, KnyttPoint>();
    private Dictionary<KnyttPoint, Color> colors = new Dictionary<KnyttPoint, Color>();
    private Dictionary<KnyttPoint, bool> visible = new Dictionary<KnyttPoint, bool>();

    public void init(KnyttWorld world, Juni juni)
    {
        SetProcessInput(false);
        SetProcess(false);
        SetPhysicsProcess(false);

        if (world == null || juni == null) { return; }
        this.world = world;
        this.juni = juni;

        this.RectSize = new Vector2(
            (world.MaxBounds.x - world.MinBounds.x + 1) * XSIZE,
            (world.MaxBounds.y - world.MinBounds.y + 1) * YSIZE);

        foreach (var area in world.Map)
        {
            if (area?.ExtraData == null) { continue; }
            var coord = area.Position;

            int? map_x = int.TryParse(area.ExtraData["MapX"], out var i) ? i : null as int?;
            int? map_y = int.TryParse(area.ExtraData["MapY"], out i) ? i : null as int?;
            if (map_x != null || map_y != null)
            {
                spoofing[coord] = new KnyttPoint(map_x ?? coord.x, map_y ?? coord.y);
            }

            if (area.ExtraData["MapVisible"]?.ToLower() == "true") { visible[coord] = true; }
            if (area.ExtraData["MapVisible"]?.ToLower() == "false") { visible[coord] = false; }

            int? color = int.TryParse(area.ExtraData["MapColor"], out i) ? i : null as int?;
            if (color != null)
            {
                if (color == 64)
                {
                    visible[coord] = false;
                }
                else
                {
                    colors[coord] = new Color((color.Value % 4) / 3f, ((color.Value / 4) % 4) / 3f, ((color.Value / 16) % 4) / 3f);
                }
            }
        }
    }

    public override void _Draw()
    {
        KnyttPoint pos = juni.GDArea.Area.Position;
        if (spoofing.ContainsKey(pos)) { pos = spoofing[pos]; }

        foreach (var area in world.Map)
        {
            if (area == null) { continue; }
            var coord = area.Position;

            if (spoofing.ContainsKey(coord) && !(visible.ContainsKey(coord) && visible[coord]))
            {
                coord = spoofing[coord];
            }

            bool is_visited = juni.Powers.isVisited(area);
            if (!juni.Powers.getPower(JuniValues.PowerNames.Map) && !is_visited) { continue; }

            if (visible.ContainsKey(coord) && !visible[coord]) { continue; }

            Rect2 r = new Rect2((coord.x - world.MinBounds.x) * XSIZE, (coord.y - world.MinBounds.y) * YSIZE, XSIZE - 1, YSIZE - 1);
            DrawRect(r, is_visited ? (colors.ContainsKey(coord) ? colors[coord] : VISITED_COLOR) :
                                        (colors.ContainsKey(coord) ? makeGrey(colors[coord]) : NOT_VISITED_COLOR));

            if (coord.Equals(pos)) { DrawRect(r, CURRENT_BORDER, filled: false, width: 2); }
        }
    }

    private Color makeGrey(Color c)
    {
        return new Color(0.5f + (c.r - 0.5f) * 0.1f, 0.5f + (c.g - 0.5f) * 0.1f, 0.5f + (c.b - 0.5f) * 0.1f);
    }

    public void ShowMap(bool show)
    {
        if (show)
        {
            KnyttPoint pos = juni.GDArea.Area.Position;
            if (spoofing.ContainsKey(pos)) { pos = spoofing[pos]; }

            RectScale = new Vector2(1, 1);
            RectPosition = new Vector2(
                (world.MinBounds.x - pos.x) * XSIZE + (GetParentAreaSize().x - XSIZE) / 2,
                (world.MinBounds.y - pos.y) * YSIZE + (GetParentAreaSize().y - YSIZE) / 2);
            RectPivotOffset = -RectPosition + GetParentAreaSize() / 2;
            Update();
        }
        GetParent<Panel>().Visible = show;
        GetTree().Paused = show;
        SetProcess(show);
        SetProcessInput(show);
        SetPhysicsProcess(show);
        Cutscene.releaseAll();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenDrag drag_event) { drag(drag_event.Relative / RectScale); }

        if (@event is InputEventMouseButton mouse_event && mouse_event.IsPressed())
        {
            if (mouse_event.ButtonIndex == (int)ButtonList.WheelDown) { scale(0.9f); }
            if (mouse_event.ButtonIndex == (int)ButtonList.WheelUp) { scale(10 / 9f); }
        }
    }

    private void scale(float k)
    {
        RectScale *= k;
    }

    public override void _PhysicsProcess(float delta)
    {
        var new_offset = Vector2.Zero;
        if (Input.IsActionPressed("up")) { new_offset += new Vector2(0, 1) * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("down")) { new_offset += new Vector2(0, -1) * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("left")) { new_offset += new Vector2(1, 0) * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("right")) { new_offset += new Vector2(-1, 0) * SCROLL_SPEED * delta; }
        if (new_offset != Vector2.Zero) { drag(new_offset); }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("map") || Input.IsActionJustPressed("pause")) { ShowMap(false); }
    }

    private void drag(Vector2 diff)
    {
        var candidate = RectPosition + diff;
        Vector2 up_left = new Vector2(20, 20) + RectPivotOffset * (RectScale - new Vector2(1, 1));
        Vector2 bottom_right = -(new Vector2(20, 20) + RectSize - GetParentAreaSize()) - (RectSize - RectPivotOffset) * (RectScale - new Vector2(1, 1));
        if (diff.x > 0 && candidate.x > up_left.x) { diff = new Vector2(0, diff.y); }
        if (diff.y > 0 && candidate.y > up_left.y) { diff = new Vector2(diff.x, 0); }
        if (diff.x < 0 && candidate.x < bottom_right.x)  { diff = new Vector2(0, diff.y); }
        if (diff.y < 0 && candidate.y < bottom_right.y)  { diff = new Vector2(diff.x, 0); }
        if (diff != Vector2.Zero) { RectPosition += diff; }
        RectPivotOffset = -RectPosition + GetParentAreaSize() / 2;
    }
}
