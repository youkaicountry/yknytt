using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class MapPanel : Panel
{
    private KnyttWorld world;
    private Juni juni;

    private int XSIZE = 50;
    private int YSIZE = 15;

    private Color VISITED_COLOR = new Color(0.5f, 0.5f, 1);
    private Color NOT_VISITED_COLOR = new Color(0.5f, 0.5f, 0.5f);
    private Color CURRENT_BORDER = new Color(1, 0, 0);

    private int SCROLL_SPEED = 10;

    private Vector2 offset;

    private Dictionary<KnyttPoint, KnyttPoint> spoofing = new Dictionary<KnyttPoint, KnyttPoint>();
    private Dictionary<KnyttPoint, Color> colors = new Dictionary<KnyttPoint, Color>();
    private Dictionary<KnyttPoint, bool> visible = new Dictionary<KnyttPoint, bool>();

    public void init(KnyttWorld world, Juni juni)
    {
        SetProcessInput(false);
        SetProcess(false);

        if (world == null || juni == null) { return; }
        this.world = world;
        this.juni = juni;
        
        this.RectSize = new Vector2(
            (world.MaxBounds.x - world.MinBounds.x + 1) * XSIZE,
            (world.MaxBounds.y - world.MinBounds.y + 1) * YSIZE);

        for (int x = world.MinBounds.x; x <= world.MaxBounds.x; x++)
        {
            for (int y = world.MinBounds.y; y <= world.MaxBounds.y; y++)
            {
                var coord = new KnyttPoint(x, y);
                var area = new KnyttArea(coord, world);
                if (area.ExtraData == null) { continue; }
                
                int? map_x = int.TryParse(area.ExtraData["MapX"], out var i) ? i : null as int?;
                int? map_y = int.TryParse(area.ExtraData["MapY"], out     i) ? i : null as int?;
                if (map_x != null || map_y != null)
                {
                    spoofing[coord] = new KnyttPoint(map_x ?? x, map_y ?? y);
                    //GD.Print($"{x} {y} -> {map_x ?? x} {map_y ?? y}");
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
    }
 
    public override void _Draw()
    {
        KnyttPoint pos = juni.GDArea.Area.Position;
        if (spoofing.ContainsKey(pos)) { pos = spoofing[pos]; }

        for (int x = world.MinBounds.x; x <= world.MaxBounds.x; x++)
        {
            for (int y = world.MinBounds.y; y <= world.MaxBounds.y; y++)
            {
                var coord = new KnyttPoint(x, y);
                if (world.Map[world.getMapIndex(coord)] == null) { continue; }

                if (spoofing.ContainsKey(coord) && !(visible.ContainsKey(coord) && visible[coord]))
                {
                    coord = spoofing[coord];
                }

                if (visible.ContainsKey(coord) && !visible[coord]) { continue; }

                bool is_visited = juni.Powers.VisitedAreas.Contains(coord);
                if (!juni.Powers.getPower(JuniValues.PowerNames.Map) && !is_visited) { continue; }

                Rect2 r = new Rect2((coord.x - world.MinBounds.x) * XSIZE, (coord.y - world.MinBounds.y) * YSIZE, XSIZE - 1, YSIZE - 1);
                // TODO: color based on gradient
                DrawRect(r, colors.ContainsKey(coord) ? colors[coord] :
                            is_visited ? VISITED_COLOR : NOT_VISITED_COLOR);

                if (coord.Equals(pos)) { DrawRect(r, CURRENT_BORDER, filled: false, width: 2); }
            }
        }
        offset = new Vector2(
            (world.MinBounds.x - pos.x) * XSIZE + (GetParentAreaSize().x - XSIZE) / 2,
            (world.MinBounds.y - pos.y) * YSIZE + (GetParentAreaSize().y - YSIZE) / 2);
        SetPosition(offset);
    }

    public void ShowMap(bool show)
    {
        if (show) { Update(); }
        GetParent<Panel>().Visible = show;
        GetTree().Paused = show;
        SetProcess(show);
        SetProcessInput(show);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenDrag drag)
        {
            offset += drag.Relative;
            SetPosition(offset);
        }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("map") || Input.IsActionJustPressed("pause")) { ShowMap(false); }
        
        var new_offset = Vector2.Zero;
        if (Input.IsActionPressed("up"))    { new_offset += new Vector2(0, 1)  * SCROLL_SPEED; }
        if (Input.IsActionPressed("down"))  { new_offset += new Vector2(0, -1) * SCROLL_SPEED; }
        if (Input.IsActionPressed("left"))  { new_offset += new Vector2(1, 0)  * SCROLL_SPEED; }
        if (Input.IsActionPressed("right")) { new_offset += new Vector2(-1, 0) * SCROLL_SPEED; }
        if (new_offset != Vector2.Zero) { offset += new_offset; SetPosition(offset); }
    }

    private void _on_CloseButton_pressed()
    {
        ShowMap(false);
    }
}
