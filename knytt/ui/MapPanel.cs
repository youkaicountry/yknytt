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
            int? map_y = int.TryParse(area.ExtraData["MapY"], out     i) ? i : null as int?;
            if (map_x != null || map_y != null)
            {
                spoofing[coord] = new KnyttPoint(map_x ?? coord.x, map_y ?? coord.y);
            }

            if (area.ExtraData["MapVisible"] == "True")  { visible[coord] = true;  }
            if (area.ExtraData["MapVisible"] == "False") { visible[coord] = false; }

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

        float xsize = XSIZE * RectScale.x;
        float ysize = YSIZE * RectScale.y;

        foreach (var area in world.Map)
        {
            if (area == null) { continue; }
            var coord = area.Position;

            if (spoofing.ContainsKey(coord) && !(visible.ContainsKey(coord) && visible[coord]))
            {
                coord = spoofing[coord];
            }

            bool is_visited = juni.Powers.VisitedAreas.Contains(coord);
            if (!juni.Powers.getPower(JuniValues.PowerNames.Map) && !is_visited) { continue; }

            if (visible.ContainsKey(coord) && !visible[coord]) { continue; }

            Rect2 r = new Rect2((coord.x - world.MinBounds.x) * xsize, (coord.y - world.MinBounds.y) * ysize, xsize - 1, ysize - 1);
            DrawRect(r, is_visited ? (colors.ContainsKey(coord) ? colors[coord] : VISITED_COLOR) :
                                        (colors.ContainsKey(coord) ? makeGrey(colors[coord]) : NOT_VISITED_COLOR));

            if (coord.Equals(pos)) { DrawRect(r, CURRENT_BORDER, filled: false, width: 2); }
        }

        RectPosition = new Vector2(
            (world.MinBounds.x - pos.x) * xsize * RectScale.x + (GetParentAreaSize().x - xsize) / 2,
            (world.MinBounds.y - pos.y) * ysize * RectScale.y + (GetParentAreaSize().y - ysize) / 2);
    }

    private Color makeGrey(Color c)
    {
        return new Color(0.5f + (c.r - 0.5f) / 5, 0.5f + (c.g - 0.5f) / 5, 0.5f + (c.b - 0.5f) / 5);
    }

    public void ShowMap(bool show)
    {
        if (show) { RectScale = new Vector2(1, 1); Update(); }
        GetParent<Panel>().Visible = show;
        GetTree().Paused = show;
        SetProcess(show);
        SetProcessInput(show);
        Cutscene.releaseAll();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenDrag drag_event) { drag(drag_event.Relative); }

        if (@event is InputEventMouseButton mouse_event && mouse_event.IsPressed())
        {
            if (mouse_event.ButtonIndex == (int)ButtonList.WheelDown) { scale(0.9f);  }
            if (mouse_event.ButtonIndex == (int)ButtonList.WheelUp)   { scale(1.11f); }
        }
    }

    private void scale(float k)
    {
        // TODO: wrong positioning sometimes
        // TODO: scale with current center of the map, not current area of the game
        RectScale *= k;
        RectSize *= k;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("map") || Input.IsActionJustPressed("pause")) { ShowMap(false); }
        
        var new_offset = Vector2.Zero;
        if (Input.IsActionPressed("up"))    { new_offset += new Vector2(0, 1)  * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("down"))  { new_offset += new Vector2(0, -1) * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("left"))  { new_offset += new Vector2(1, 0)  * SCROLL_SPEED * delta; }
        if (Input.IsActionPressed("right")) { new_offset += new Vector2(-1, 0) * SCROLL_SPEED * delta; }
        drag(new_offset);
    }

    private void drag(Vector2 diff)
    {
        // This is very buggy
        //var candidate = RectPosition + diff;
        //if (diff.x > 0 && candidate.x > BORDER) { diff = new Vector2(0, diff.y); }
        //if (diff.x < 0 && candidate.x < -RectSize.x * RectScale.x + GetParentAreaSize().x - BORDER)  { diff = new Vector2(0, diff.y); }
        //if (diff.y > 0 && candidate.y > BORDER) { diff = new Vector2(diff.x, 0); }
        //if (diff.y < 0 && candidate.y < -RectSize.y * RectScale.y + GetParentAreaSize().y - BORDER)  { diff = new Vector2(diff.x, 0); }
        if (diff != Vector2.Zero) { RectPosition += diff; }
    }
}
