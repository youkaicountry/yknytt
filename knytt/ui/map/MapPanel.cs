using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class MapPanel : Panel
{
    private KnyttWorld world;
    private Juni juni;

    public static int SCALE { get; set; } = 12;
    public static int XSIZE => 600 / SCALE;
    public static int YSIZE => 240 / SCALE;

    private static readonly Color VISITED_COLOR = new Color(0.5f, 0.5f, 1);
    private static readonly Color NOT_VISITED_COLOR = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color CURRENT_BORDER = new Color(1, 0, 0);
    
    private static Color markColor(char c)
    {
        return  c == 'm' ?              new Color(0, 0, 1) : 
                c == 'r' || c == 'R' ?  new Color(1, 0, 0) :
                c == 'y' || c == 'Y' ?  new Color(0.75f, 0.75f, 0) :
                c == 'b' || c == 'B' ?  new Color(0, 0, 1) :
                c == 'u' || c == 'U' ?  new Color(0.5f, 0, 1) :
                c == 'k' || c == 'd' ?  new Color(0, 0.65f, 0) :
                                        new Color(1, 0, 0);
    }

    private static string markChar(char c)
    {
        return ("rybu".IndexOf(c) != -1 ? 'k' : "RYBU".IndexOf(c) != -1 ? 'd' : c).ToString().Capitalize();
    }

    private static readonly float SCROLL_SPEED = 600;
    private static readonly float BORDER = 20;
    private static readonly int AREA_PRELOAD_LIMIT = 1000;
    private static readonly float DETAILED_MIN_SCALE = 0.5f;

    private Font mark_font;

    //knytt/data/Gradients$ for i in {0..255}; do convert Gradient$i.png -format "0x%[hex:u.p{3,237}]FF\n" info: ; done
    private static Color[] GRADIENTS = new Color[] {
        new Color(0x6E3DABFF), new Color(0x283C4CFF), new Color(0xD4BBFFFF), new Color(0x474646FF), new Color(0x060021FF), new Color(0x006363FF), new Color(0x61707EFF), new Color(0xF3F3A8FF), 
        new Color(0x11508BFF), new Color(0x755038FF), new Color(0x61707EFF), new Color(0xEBCBC3FF), new Color(0x742C5CFF), new Color(0x535985FF), new Color(0x010101FF), new Color(0xDFF1F6FF), 
        new Color(0x3D205AFF), new Color(0xCDB0EEFF), new Color(0xFFD4C7FF), new Color(0x156B61FF), new Color(0x9BA6A8FF), new Color(0x333334FF), new Color(0xD8B13DFF), new Color(0x12465DFF), 
        new Color(0x1D1D1DFF), new Color(0xC0AA71FF), new Color(0xF64239FF), new Color(0xFA0008FF), new Color(0x2E2E2EFF), new Color(0x082927FF), new Color(0xFEFFFFFF), new Color(0x4C9233FF), 
        new Color(0xC08E8CFF), new Color(0xFEFFFFFF), new Color(0x4A4E55FF), new Color(0x5EC39EFF), new Color(0x00002AFF), new Color(0x4F5076FF), new Color(0x272733FF), new Color(0xAACDA3FF), 
        new Color(0x658399FF), new Color(0x728181FF), new Color(0xA5AABAFF), new Color(0x181C24FF), new Color(0x000000FF), new Color(0x6B0052FF), new Color(0x005EDBFF), new Color(0xFFFFFFFF), 
        new Color(0x302522FF), new Color(0xB2BDD5FF), new Color(0x00005AFF), new Color(0x3F2C27FF), new Color(0x928F9EFF), new Color(0xF7FEBAFF), new Color(0x8F8B54FF), new Color(0x414165FF), 
        new Color(0x3A0065FF), new Color(0x171063FF), new Color(0x3C3C3CFF), new Color(0x514E47FF), new Color(0x008040FF), new Color(0x0A018EFF), new Color(0x1F2C4BFF), new Color(0x00137FFF), 
        new Color(0xDFFDDBFF), new Color(0x7499CAFF), new Color(0x1F1F1FFF), new Color(0x402E34FF), new Color(0x1E040DFF), new Color(0xE1FBF2FF), new Color(0x718155FF), new Color(0x000000FF), 
        new Color(0x3E487EFF), new Color(0xFFFFFEFF), new Color(0x07122FFF), new Color(0x767416FF), new Color(0x191414FF), new Color(0xC4D0D0FF), new Color(0x0A0F0FFF), new Color(0x36145CFF), 
        new Color(0x000000FF), new Color(0x419F41FF), new Color(0x04170DFF), new Color(0x033D2BFF), new Color(0x518C73FF), new Color(0x1F2436FF), new Color(0xFFFFFFFF), new Color(0x221D29FF), 
        new Color(0x9B0364FF), new Color(0x4C4664FF), new Color(0x464B6BFF), new Color(0xEF3251FF), new Color(0xFEFEFEFF), new Color(0xFFFFFFFF), new Color(0x000000FF), new Color(0xC8A872FF), 
        new Color(0xD9D7CFFF), new Color(0xF8F8FCFF), new Color(0x242424FF), new Color(0xFEFEFFFF), new Color(0x562A2AFF), new Color(0xCACCCBFF), new Color(0xB364A9FF), new Color(0x0166B2FF), 
        new Color(0xFDFDFDFF), new Color(0xD3D3D3FF), new Color(0xDDDDDDFF), new Color(0xFFFFFFFF), new Color(0x1A717BFF), new Color(0xFFFFFFFF), new Color(0xE7E7E7FF), new Color(0x172479FF), 
        new Color(0x01042BFF), new Color(0x033D2BFF), new Color(0x0080E3FF), new Color(0x232054FF), new Color(0x005787FF), new Color(0xE6A9FFFF), new Color(0x145414FF), new Color(0x272755FF), 
        new Color(0xFFFFFFFF), new Color(0xD8D7DAFF), new Color(0x002D5CFF), new Color(0xD89B85FF), new Color(0xFEFEFEFF), new Color(0xC4C4C4FF), new Color(0xF0F0CDFF), new Color(0xE2D4B2FF), 
        new Color(0x6C192DFF), new Color(0xBCB8A4FF), new Color(0x4E3AE7FF), new Color(0x3492A8FF), new Color(0xCCDEE7FF), new Color(0xAF6829FF), new Color(0x00137FFF), new Color(0x5357A0FF), 
        new Color(0x47A197FF), new Color(0xFFFC00FF), new Color(0x000089FF), new Color(0x000850FF), new Color(0x000000FF)/*changed*/, new Color(0x000000FF), new Color(0x61707EFF), new Color(0xFFFFFFFF), 
        new Color(0x480121FF), new Color(0x010000FF), new Color(0x9B0394FF), new Color(0xBEFDFFFF), new Color(0xB035F0FF), new Color(0x09006AFF), new Color(0x2F4D92FF), new Color(0x4B4B4BFF), 
        new Color(0x4D156AFF), new Color(0x7FB6ABFF), new Color(0x000000FF), new Color(0x4F4951FF), new Color(0x460000FF), new Color(0x004646FF), new Color(0xFFAD00FF), new Color(0x7187C0FF), 
        new Color(0x3D64D8FF), new Color(0x73252CFF), new Color(0x343658FF), new Color(0x5D5D5DFF), new Color(0xFFEC21FF), new Color(0x86AD91FF), new Color(0x0F168AFF), new Color(0x3F567CFF), 
        new Color(0x566393FF), new Color(0xD83D54FF), new Color(0xDCDBDCFF), new Color(0x102347FF), new Color(0x272A2FFF), new Color(0x285769FF), new Color(0x324B72FF), new Color(0x466B61FF), 
        new Color(0x598E63FF), new Color(0xBCBCCEFF), new Color(0xBEBE9AFF), new Color(0x3B3DA8FF), new Color(0x61707EFF), new Color(0x465C7AFF), new Color(0x4E7DB6FF), new Color(0x191919FF), 
        new Color(0x50005DFF), new Color(0x000100FF), new Color(0x17274AFF), new Color(0x808080FF), new Color(0xDCDCDCFF), new Color(0x0D0A25FF), new Color(0xA1A1A1FF), new Color(0x888686FF), 
        new Color(0x848484FF), new Color(0xE45E15FF), new Color(0x102410FF), new Color(0x353535FF), new Color(0x182462FF), new Color(0x32001EFF), new Color(0x111111FF), new Color(0x637EB1FF), 
        new Color(0xA04141FF), new Color(0x3B5429FF), new Color(0xC64E4EFF), new Color(0x000000FF), new Color(0xFF3429FF), new Color(0x143450FF), new Color(0x101010FF), new Color(0x0D3624FF), 
        new Color(0x413931FF), new Color(0xF6F6F6FF), new Color(0x4B98ABFF), new Color(0x11234BFF), new Color(0x84B694FF), new Color(0x4E5D96FF), new Color(0x405A64FF), new Color(0xFFFFFFFF), 
        new Color(0x094545FF), new Color(0xC06999FF), new Color(0x000533FF), new Color(0x669967FF), new Color(0x000000FF)/*changed*/, new Color(0x5A0000FF), new Color(0x4D3A24FF), new Color(0x237596FF), 
        new Color(0x06582CFF), new Color(0x000000FF), new Color(0x142B55FF), new Color(0x040F13FF), new Color(0x0C2444FF), new Color(0x5E7D8EFF), new Color(0x234069FF), new Color(0x9C1D1DFF), 
        new Color(0xFB6700FF), new Color(0x8090A6FF), new Color(0x442863FF), new Color(0x003300FF), new Color(0x000000FF), new Color(0x00084EFF), new Color(0x57455FFF), new Color(0xA8FFFFFF), 
        new Color(0xA8FFFFFF), new Color(0x00FFFFFF), new Color(0x232941FF), new Color(0xFCFFF1FF), new Color(0x4D526CFF), new Color(0x9CAD4DFF), new Color(0x040AFFFF), new Color(0x765F4DFF), 
        new Color(0x361C1CFF), new Color(0x997FA6FF), new Color(0x008080FF), new Color(0x0B1035FF), new Color(0x6E583EFF), new Color(0x5FB67EFF), new Color(0x490249FF), new Color(0x030663FF),         
    };

    private Dictionary<KnyttPoint, Color> colors = new Dictionary<KnyttPoint, Color>();
    private Dictionary<KnyttPoint, bool> visible = new Dictionary<KnyttPoint, bool>();

    public static void initScale() => SCALE = GDKnyttSettings.DetailedMap ? 12 : 16;
    
    public void initSize()
    {
        this.RectSize = new Vector2(
            (world.MaxBounds.x - world.MinBounds.x + 1) * XSIZE,
            (world.MaxBounds.y - world.MinBounds.y + 1) * YSIZE);
    }

    public void init(KnyttWorld world, Juni juni)
    {
        SetProcessInput(false);
        SetProcess(false);
        SetPhysicsProcess(false);

        mark_font = ResourceLoader.Load<DynamicFont>("res://knytt/ui/map/MarkFont.tres");
        
        if (world == null || juni == null) { return; }
        this.world = world;
        this.juni = juni;
        initScale();
        initSize();

        foreach (var area in world.Map.Values)
        {
            if (area == null) { continue; }
            var coord = area.Position;
            colors[coord] = makeBright(GRADIENTS[area.Background]);
            if (area.ExtraData == null) { continue; }

            int? map_x = int.TryParse(area.ExtraData["MapX"], out var i) ? i : null as int?;
            int? map_y = int.TryParse(area.ExtraData["MapY"], out i) ? i : null as int?;
            if (map_x != null || map_y != null)
            {
                area.Spoofing = new KnyttPoint(map_x ?? coord.x, map_y ?? coord.y);
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
        var map_viewports = juni.Game.GetNode<MapViewports>("%MapViewports");
        KnyttPoint screen_min_bounds = new KnyttPoint(
            world.MinBounds.x - (int)(GetGlobalRect().Position.x / RectScale.x) / XSIZE,
            world.MinBounds.y - (int)(GetGlobalRect().Position.y / RectScale.y) / YSIZE);
        KnyttPoint screen_max_bounds = screen_min_bounds + new KnyttPoint(
            (int)(GetViewportRect().Size.x / RectScale.x) / XSIZE + 1,
            (int)(GetViewportRect().Size.y / RectScale.y) / YSIZE + 1); // not sure about it
        bool draw_detailed = GDKnyttSettings.DetailedMap && RectScale.x > DETAILED_MIN_SCALE;

        foreach (var area in world.Map.Values)
        {
            var coord = area.MapPosition;

            if (coord.x < screen_min_bounds.x || coord.y < screen_min_bounds.y || 
                coord.x > screen_max_bounds.x || coord.y > screen_max_bounds.y) { continue; }

            bool is_visited = juni.Powers.isVisited(area);
            if (!juni.Powers.getPower(JuniValues.PowerNames.Map) && !is_visited) { continue; }

            if (visible.ContainsKey(coord) && !visible[coord]) { continue; }

            Rect2 r = new Rect2((coord.x - world.MinBounds.x) * XSIZE + 1, (coord.y - world.MinBounds.y) * YSIZE + 1, XSIZE - 2, YSIZE - 2);
            DrawRect(r, is_visited ? (colors.ContainsKey(coord) ? colors[coord] : VISITED_COLOR) : NOT_VISITED_COLOR);

            if (draw_detailed && is_visited)
            {
                Rect2 r2 = new Rect2((coord.x - world.MinBounds.x) * XSIZE, (coord.y - world.MinBounds.y) * YSIZE, XSIZE, YSIZE);
                (Rect2 src, Texture tex) = map_viewports.getArea(coord);
                if (tex != null) { DrawTextureRectRegion(tex, r2, src); }
            }

            if (is_visited && juni.Powers.Marked?.ContainsKey(coord) == true)
            {
                Vector2 p = new Vector2((coord.x - world.MinBounds.x) * XSIZE + 4, (coord.y - world.MinBounds.y) * YSIZE + 12);
                string m = juni.Powers.Marked[coord];
                DrawChar(mark_font, p, markChar(m[0]), "", markColor(m[0]));
                if (m.Length == 2) { DrawChar(mark_font, p + new Vector2(11, 0), markChar(m[1]), "", markColor(m[1])); }
                else if (m.Length > 2) { DrawChar(mark_font, p + new Vector2(11, 0), "+", "", markColor('+')); }
            }
        }

        KnyttPoint cur_pos = juni.GDArea.Area.MapPosition - world.MinBounds;
        DrawRect(new Rect2(cur_pos.x * XSIZE, cur_pos.y * YSIZE, XSIZE, YSIZE), CURRENT_BORDER, filled: false, width: 2);
    }

    private Color makeBright(Color c) => new Color(0.5f + signedSqrt(c.r - 0.5f), 0.5f + signedSqrt(c.g - 0.5f), 0.5f + signedSqrt(c.b - 0.5f));

    private float signedSqrt(float f) => f > 0 ? Mathf.Sqrt(f) : -Mathf.Sqrt(-f);

    public void ShowMap(bool show)
    {
        if (show)
        {
            KnyttPoint pos = juni.GDArea.Area.MapPosition;

            RectScale = Vector2.One;
            RectPosition = new Vector2(
                (world.MinBounds.x - pos.x) * XSIZE + (GetParentAreaSize().x - XSIZE) / 2,
                (world.MinBounds.y - pos.y) * YSIZE + (GetParentAreaSize().y - YSIZE) / 2);
            RectPivotOffset = -RectPosition + GetParentAreaSize() / 2;

            if (world.Size.Area < AREA_PRELOAD_LIMIT) { juni.Game.GetNode<MapViewports>("%MapViewports").loadAll(); }
            juni.GDArea?.Objects?.checkCollectables(juni.Powers);
            setMarkButtonText(juni.Powers.hasMark(juni.GDArea.Area.Position, JuniValues.Collectable.User));
            last_drag_distance = 0;
            Update();
        }
        GetParent<Panel>().Visible = show;
        GetTree().Paused = show;
        SetProcess(show);
        SetProcessInput(show);
        SetPhysicsProcess(show);
        Cutscene.releaseAll();
    }

    private Vector2 drag_pos0, drag_pos1;
    private float last_drag_distance;

    public override void _Input(InputEvent @event)
    {
        GetNode<GDKnyttKeys>("/root/GdKnyttKeys")._Input(@event);

        if (@event is InputEventScreenDrag drag_event)
        {
            if (drag_event.Index == 0)
            {
                drag(drag_event.Relative / RectScale);
                drag_pos0 = drag_event.Position;
            }
            else if (drag_event.Index == 1)
            {
                drag_pos1 = drag_event.Position;
                if (last_drag_distance == 0) { last_drag_distance = drag_pos1.DistanceTo(drag_pos0); }
            }

            if (last_drag_distance > 0)
            {
                float drag_distance = drag_pos1.DistanceTo(drag_pos0);
                float scale_k = drag_distance / last_drag_distance;
                if (scale_k < 1.5f && scale_k > 1 / 1.5f)
                {
                    scale(scale_k);
                }
                last_drag_distance = drag_distance;
            }
        }

        if (@event is InputEventScreenTouch touch_event) { last_drag_distance = 0; }

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
        if (Input.IsActionPressed("up")) { new_offset += new Vector2(0, 1) * SCROLL_SPEED * delta / RectScale; }
        if (Input.IsActionPressed("down")) { new_offset += new Vector2(0, -1) * SCROLL_SPEED * delta / RectScale; }
        if (Input.IsActionPressed("left")) { new_offset += new Vector2(1, 0) * SCROLL_SPEED * delta / RectScale; }
        if (Input.IsActionPressed("right")) { new_offset += new Vector2(-1, 0) * SCROLL_SPEED * delta / RectScale; }
        if (Input.IsActionJustPressed("walk")) { scale(0.9f); }
        if (Input.IsActionJustPressed("umbrella")) { scale(10 / 9f); }
        if (Input.IsActionJustPressed("jump")) { mark(); }
        if (new_offset != Vector2.Zero) { drag(new_offset); }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("map") || Input.IsActionJustPressed("pause")) { ShowMap(false); }
    }

    private void drag(Vector2 diff)
    {
        var candidate = RectPosition + diff;
        Vector2 up_left = new Vector2(BORDER, BORDER) + RectPivotOffset * (RectScale - Vector2.One);
        Vector2 bottom_right = -(new Vector2(BORDER, BORDER) + RectSize - GetParentAreaSize()) - (RectSize - RectPivotOffset) * (RectScale - Vector2.One);
        if (diff.x > 0 && candidate.x > up_left.x) { diff = new Vector2(0, diff.y); }
        if (diff.y > 0 && candidate.y > up_left.y) { diff = new Vector2(diff.x, 0); }
        if (diff.x < 0 && candidate.x < bottom_right.x)  { diff = new Vector2(0, diff.y); }
        if (diff.y < 0 && candidate.y < bottom_right.y)  { diff = new Vector2(diff.x, 0); }
        if (diff != Vector2.Zero) { RectPosition += diff; }
        RectPivotOffset = -RectPosition + GetParentAreaSize() / 2;
    }

    private void setMarkButtonText(bool has_mark) => GetNode<Button>("../MarkButton").Text = has_mark ? "M–" : "M+";

    private void mark()
    {
        bool marked = juni.Powers.hasMark(juni.GDArea.Area.MapPosition, JuniValues.Collectable.User);
        var base_powers = juni.Game.GDWorld.KWorld.CurrentSave.SourcePowers;
        if (marked)
        {
            juni.Powers.unsetMark(juni.GDArea.Area.MapPosition, JuniValues.Collectable.User);
            base_powers.unsetMark(juni.GDArea.Area.MapPosition, JuniValues.Collectable.User);
        }
        else
        {
            juni.Powers.setMark(juni.GDArea.Area.MapPosition, JuniValues.Collectable.User);
            base_powers.setMark(juni.GDArea.Area.MapPosition, JuniValues.Collectable.User);
        }
        setMarkButtonText(!marked);
        Update();
    }
}
