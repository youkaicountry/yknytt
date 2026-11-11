using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YKnyttLib;

public class LevelEditor : Control
{
    private static readonly Vector2 SCREEN_SIZE = new Vector2(700, 610);
    private static readonly Vector2 SCRIPT_SIZE = new Vector2(1010, 610);
    private static readonly KnyttPoint DEFAULT_SCREEN = new KnyttPoint(1000, 1000);
    private static readonly ScreenValue[] SOUNDS = { ScreenValue.Music, ScreenValue.AmbianceA, ScreenValue.AmbianceB };

    private enum FileMenu { Save, SaveAs, Quit }
    private enum EditMenu { Undo, Redo, LevelStart }
    private enum OptionsMenu { FadeLayers, FollowScript, Guides, Neighbours }
    private enum GoToMenu { Start, Default, Coordinates, WorldMap }
    private enum TestMenu { FromStart, FromHere, Profiles, UseAsStart, Profile = 100 }
    private enum Placing { None, Start, Test, Profile }
    private enum CellMenu { Pick, Fill, Replace, Test, Start }

    private static readonly HashSet<string> overwrite_allowed = new HashSet<string>();
    private static LevelEditor tested;
    private static readonly Color SAVED_COLOR = Color.Color8(87, 135, 231);
    private static readonly Color CHANGED_COLOR = Color.Color8(247, 195, 79);
    private static readonly Color INACTIVE_COLOR = new Color(1, 1, 1, 0.3f);

    // KS+'s colours for the line round the screen and the checks behind the tileset, the default last.
    // The buttons show a colour of their own for some
    private static readonly Color[] BORDER_COLORS = {
        Color.Color8(56, 88, 23), Color.Color8(91, 44, 83), Color.Color8(255, 255, 255), Color.Color8(0, 0, 0),
        Color.Color8(127, 177, 185), Color.Color8(181, 205, 138), Color.Color8(210, 163, 202), Color.Color8(102, 102, 102) };
    private static readonly Color[] BUTTON_COLORS = {
        Color.Color8(56, 88, 23), Color.Color8(91, 44, 83), Color.Color8(255, 255, 254), Color.Color8(1, 1, 1),
        Color.Color8(100, 190, 213), Color.Color8(181, 205, 138), Color.Color8(210, 163, 202), Color.Color8(76, 76, 76) };
    private static readonly Color[,] CHECKS = {
        { Color.Color8(56, 88, 23), Color.Color8(55, 82, 26) }, { Color.Color8(91, 44, 83), Color.Color8(85, 44, 78) },
        { Color.Color8(255, 255, 255), Color.Color8(226, 226, 226) }, { Color.Color8(0, 0, 0), Color.Color8(7, 7, 7) },
        { Color.Color8(127, 177, 185), Color.Color8(116, 159, 166) }, { Color.Color8(181, 205, 138), Color.Color8(162, 183, 125) },
        { Color.Color8(210, 163, 202), Color.Color8(187, 147, 180) }, { Color.Color8(23, 74, 89), Color.Color8(26, 70, 83) } };
    private static readonly Color TILESET_BUTTON_DEFAULT = Color.Color8(23, 74, 89);
    private const int CHECK_SIZE = 6;
    private static readonly Vector2 TILE = new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
    private const int TOUCH = -1;
    private const ulong LONG_PRESS_MS = 500;
    private const int BIG_STEP = 10;

    private Vector2 game_window_size;

    private GDKnyttWorldImpl world;
    private EditorAssets assets;
    private ObjectCatalog catalog;
    private EditorHistory history = new EditorHistory();
    private KnyttPoint start_area;
    private KnyttPoint start_position;

    private KnyttPoint coords;
    private readonly int[] values = new int[ScreenValues.ALL.Length];
    private readonly int?[] playing = new int?[SOUNDS.Length];

    private int layer = -1;
    private int selected_tile;
    private KnyttPoint tile_block = new KnyttPoint(1, 1);
    private KnyttPoint? tile_drag;
    private KnyttStamp copied;
    private bool copied_objects;
    private KnyttPoint? stroke_origin;
    private int selected_bank;
    private int selected_object;
    private ulong press_time;
    private KnyttPoint menu_cell;
    private readonly List<ObjectKey> menu_keys = new List<ObjectKey>();

    // Objects with World.ini data: what the cell menu says, and the key its form opens at. No section: the screen's
    private class ObjectKey
    {
        public string label, key, section;
        public ObjectKey(string label, string key, string section = null)
        {
            this.label = label; this.key = key; this.section = section;
        }
    }

    private static readonly Dictionary<KnyttPoint, ObjectKey> OBJECT_KEYS = objectKeys();

    private static Dictionary<KnyttPoint, ObjectKey> objectKeys()
    {
        var keys = new Dictionary<KnyttPoint, ObjectKey>()
        {
            [new KnyttPoint(0, 2)] = new ObjectKey("Edit win scene", "Ending"),
            [new KnyttPoint(0, 20)] = new ObjectKey("Edit warp", "WarpX(U)"),
            [new KnyttPoint(0, 36)] = new ObjectKey("Edit title", "Title"),
        };
        foreach (char letter in "ABC")
        {
            int i = letter - 'A';
            var sign = new ObjectKey($"Edit sign {letter}", $"Sign({letter})");
            var shift = new ObjectKey($"Edit shift {letter}", $"ShiftXMap({letter})");
            var trigger = new ObjectKey($"Edit trigger {letter}", $"TrigBank({letter})");
            // Signs and sign areas, shifts, then triggers with their spawn points and delete areas
            foreach (int first in new[] { 17, 29 }) { keys[new KnyttPoint(0, first + i)] = sign; }
            keys[new KnyttPoint(0, 14 + i)] = shift;
            foreach (int first in new[] { 32, 42, 45 }) { keys[new KnyttPoint(0, first + i)] = trigger; }
        }
        return keys;
    }

    // Custom objects are [Custom Object 5] for 255:5 and [Custom Object B5] for 254:5
    private static ObjectKey objectKey(KnyttPoint id)
    {
        if (!ObjectCatalog.isCustom(id.x) || id.y == 0) { return OBJECT_KEYS.TryGetValue(id, out var key) ? key : null; }
        string name = $"{(id.x == 254 ? "B" : "")}{id.y}";
        return new ObjectKey($"Edit custom object {name}", "Image", $"Custom Object {name}");
    }
    private bool quit_game;
    private KnyttPoint? hovered;
    private string status_message = "";
    private Placing placing;
    private List<string> test_profiles = new List<string>();
    private string script_text;
    private string script_newline;

    private ScreenView screen_view;
    private MiniMap mini_map;
    private ObjectPalette palette;
    private TestDialog test_dialog;
    private Control script;
    private TextEdit ini_edit;

    // What a right-click in the text found: its section, the screen that is, and a form field for its key
    private const int INI_GOTO = 1000, INI_FORM = 1001;
    private int ini_menu_items = -1;
    private string ini_section;
    private KnyttPoint? ini_screen;
    private IniForm ini_form;
    private Control ini_field;
    private string[] ini_letters;
    private bool follow_script, following;
    private string ini_key;
    private ConfirmationDialog goto_dialog;
    private ConfirmationDialog overwrite_dialog;
    private ConfirmationDialog save_as_dialog;
    private ConfirmationDialog unsaved_dialog;
    private AcceptDialog keys_dialog;

    public static bool Available => OS.GetName() != "Unix";

    // While a test plays, the editor waits off the tree with its world and history
    public static bool Testing => tested != null;

    public static void open(SceneTree tree, GDKnyttWorldImpl world, bool created = false)
    {
        if (created) { overwrite_allowed.Add(world.WorldDirectory); }
        GDKnyttDataStore.KWorld = world;
        tree.ChangeScene("res://knytt/editor/LevelEditor.tscn");
    }

    public override void _Ready()
    {
        script = GetNode<Control>("%Script");
        ini_edit = GetNode<TextEdit>("%IniEdit");
        ini_edit.AddColorRegion("[", "]", Color.Color8(170, 50, 150));
        ini_edit.AddColorRegion(";", "", new Color(0.5f, 0.5f, 0.5f), lineOnly: true);
        ini_edit.GetMenu().Connect("about_to_show", this, nameof(fillIniMenu));
        ini_edit.GetMenu().Connect("id_pressed", this, nameof(_on_IniMenu_id_pressed));
        setupScreen();

        screen_view = GetNode<ScreenView>("%ScreenView");
        mini_map = GetNode<MiniMap>("%MiniMap");
        palette = GetNode<ObjectPalette>("%ObjectPalette");
        test_dialog = GetNode<TestDialog>("%TestDialog");
        goto_dialog = GetNode<ConfirmationDialog>("%GoToDialog");
        overwrite_dialog = GetNode<ConfirmationDialog>("%OverwriteDialog");
        overwrite_dialog.GetOk().Text = "Overwrite";
        save_as_dialog = GetNode<ConfirmationDialog>("%SaveAsDialog");
        save_as_dialog.GetOk().Text = "Save";
        unsaved_dialog = GetNode<ConfirmationDialog>("%UnsavedDialog");
        unsaved_dialog.GetOk().Text = "Leave";
        keys_dialog = GetNode<AcceptDialog>("%KeysDialog");

        var file_menu = GetNode<MenuButton>("%FileMenu").GetPopup();
        file_menu.AddItem("Save", (int)FileMenu.Save, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.S);
        file_menu.AddItem("Save as", (int)FileMenu.SaveAs, (uint)KeyModifierMask.MaskCtrl | (uint)KeyModifierMask.MaskShift | (uint)KeyList.S);
        file_menu.AddSeparator();
        file_menu.AddItem("Quit", (int)FileMenu.Quit);
        file_menu.Connect("id_pressed", this, nameof(_on_FileMenu_id_pressed));

        var edit_menu = GetNode<MenuButton>("%EditMenu").GetPopup();
        edit_menu.AddItem("Undo", (int)EditMenu.Undo, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.Z);
        edit_menu.AddItem("Redo", (int)EditMenu.Redo, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.Y);
        edit_menu.AddSeparator();
        edit_menu.AddItem("Level start", (int)EditMenu.LevelStart);
        edit_menu.Connect("id_pressed", this, nameof(_on_EditMenu_id_pressed));

        var options_menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        options_menu.AddCheckItem("Fade other layers (hold Space to see all)", (int)OptionsMenu.FadeLayers);
        options_menu.AddCheckItem("Follow the script cursor (screen and form)", (int)OptionsMenu.FollowScript);
        options_menu.SetItemChecked(options_menu.GetItemIndex((int)OptionsMenu.FadeLayers), screen_view.FadeOthers);
        options_menu.AddSeparator();
        options_menu.AddCheckItem("Guides", (int)OptionsMenu.Guides, (uint)KeyList.Tab);
        options_menu.AddCheckItem("Neighbour screens", (int)OptionsMenu.Neighbours);
        showGuides(GDKnyttSettings.EditorGuides);
        showNeighbours(GDKnyttSettings.EditorNeighbours);
        showBorderColor(GDKnyttSettings.EditorBorderColor);
        showTilesetColor(GDKnyttSettings.EditorTilesetColor);
        options_menu.Connect("id_pressed", this, nameof(_on_OptionsMenu_id_pressed));

        var goto_menu = GetNode<MenuButton>("%GoToMenu").GetPopup();
        goto_menu.AddItem("Start position", (int)GoToMenu.Start);
        goto_menu.AddItem($"x{DEFAULT_SCREEN.x}y{DEFAULT_SCREEN.y}", (int)GoToMenu.Default);
        goto_menu.AddSeparator();
        goto_menu.AddItem("Coordinates", (int)GoToMenu.Coordinates, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.G);
        goto_menu.AddItem("World map", (int)GoToMenu.WorldMap, (uint)KeyList.M);
        goto_menu.Connect("id_pressed", this, nameof(_on_GoToMenu_id_pressed));

        var test_menu = GetNode<MenuButton>("%TestMenu").GetPopup();
        test_menu.Connect("about_to_show", this, nameof(fillTestMenu));
        test_menu.Connect("id_pressed", this, nameof(_on_TestMenu_id_pressed));

        var help_menu = GetNode<MenuButton>("%HelpMenu").GetPopup();
        help_menu.AddItem("Keys", 0, (uint)KeyList.F1);
        help_menu.Connect("id_pressed", this, nameof(_on_HelpMenu_id_pressed));

        loadWorld(GDKnyttDataStore.KWorld);
        fillTestMenu();
    }

    private void loadWorld(GDKnyttWorldImpl world)
    {
        this.world = world;
        if (world.Map == null) { world.loadWorldMap(new MemoryStream(world.getWorldData("Map.bin"))); }
        assets = new EditorAssets(world);
        catalog = new ObjectCatalog(world);
        palette.init(assets, catalog);
        test_dialog.init(world);

        screen_view.init(world, assets);
        GetNode<WorldView>("%WorldView").init(world, assets, catalog);
        showStart();
        selectObject(0, 0);
        selectLayer(0);
        GDKnyttDataStore.setTitle(world.WorldDirectoryName);
        goTo(start_area);
    }

    private void goTo(KnyttPoint coords)
    {
        applyScript();
        this.coords = coords;

        // An empty screen keeps the last values, for when it gets painted
        var area = world.getArea(coords);
        if (area != null && !area.Empty)
        {
            foreach (var value in ScreenValues.ALL) { values[(int)value] = ScreenValues.get(area, value); }
        }

        screen_view.showScreen(coords, value(ScreenValue.Background));
        mini_map.showScreen(world, coords);
        foreach (var value in ScreenValues.ALL) { GetNode<Label>($"%{value}Value").Text = $"{this.value(value)}"; }
        showMapColor();
        showStatus();
        showTileset();
        showPreviews();
        showScript();
        showChanged();
    }

    private int value(ScreenValue which) => values[(int)which];

    private void changeValue(ScreenValue which, int step)
    {
        values[(int)which] = (value(which) + step) & 0xFF;
        var area = world.getArea(coords);
        bool empty = area == null || area.Empty;
        if (!empty)
        {
            history.setValue(area, which, value(which));
            history.end();
        }
        goTo(coords);
        if (empty) { showStatus("No screen here yet. Painting makes one with these"); }
    }

    // Each channel whose button is down plays what this screen has
    private void showPreviews()
    {
        for (int i = 0; i < SOUNDS.Length; i++)
        {
            int? number = GetNode<BaseButton>($"%Play{SOUNDS[i]}").Pressed ? value(SOUNDS[i]) : (int?)null;
            if (number == playing[i]) { continue; }
            playing[i] = number;
            var player = GetNode<AudioStreamPlayer>($"%{SOUNDS[i]}Player");
            player.Stream = number == null ? null : SOUNDS[i] == ScreenValue.Music ? assets.getMusic(number.Value) : assets.getAmbiance(number.Value);
            if (player.Stream == null) { player.Stop(); } else { player.Play(); }
        }
    }

    private void stopPreviews()
    {
        foreach (var sound in SOUNDS) { GetNode<BaseButton>($"%Play{sound}").SetPressedNoSignal(false); }
        showPreviews();
    }

    private void showChanged()
    {
        bool typed = script.Visible && ini_edit.Text != script_text;
        GetNode<ColorRect>("%ChangeLineTop").Color = GetNode<ColorRect>("%ChangeLineBottom").Color =
            history.Modified || typed ? CHANGED_COLOR : SAVED_COLOR;
    }

    // World.ini, at this screen's section
    private void showScript()
    {
        if (!script.Visible) { return; }
        string text = worldFile("World.ini");
        script_newline = text.Contains("\r\n") ? "\r\n" : "\n";
        script_text = text.Replace("\r\n", "\n");
        if (ini_edit.Text != script_text) { ini_edit.Text = script_text; }
        showForms();

        int line = iniLine(script_text, ScreenSection);
        if (line < 0 || following) { return; }
        ini_edit.CursorSetLine(line, adjustViewport: false);
        ini_edit.SetDeferred("scroll_vertical", line); // the new text isn't laid out yet
    }

    private string ScreenSection => $"x{coords.x}y{coords.y}";

    // The forms show the text as it is, typed or not
    private void showForms()
    {
        var ini = KnyttWorld.parseWorldIni(ini_edit.Text);
        foreach (IniForm form in GetNode("%Forms").GetChildren()) { form.show(ini, ScreenSection); }
    }

    // New text, with the view kept where it was
    private void setScriptText(string text)
    {
        var scroll = ini_edit.ScrollVertical;
        ini_edit.Text = text;
        ini_edit.ScrollVertical = scroll;
        showChanged();
    }

    // Selects a key's line, or its section's when the key isn't there
    private void showKey(string section, string key)
    {
        int line = iniLine(ini_edit.Text, section, key);
        if (line < 0) { return; }
        ini_edit.CursorSetLine(line);
        ini_edit.Select(line, 0, line, ini_edit.GetLine(line).Length);
    }

    // The line of a key in a section, or of the section when the key isn't there. -1: no section
    private static int iniLine(string text, string section, string key = null)
    {
        var lines = text.Split('\n');
        int found = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("["))
            {
                if (found >= 0) { break; }
                if (line.ToLower() == $"[{section.ToLower()}]") { found = i; }
            }
            else if (found >= 0 && key != null && line.Split('=')[0].Trim().ToLower() == key.ToLower()) { return i; }
        }
        return found;
    }

    // What was typed goes into the level when the text loses focus, or before anything that needs it
    private void applyScript()
    {
        if (!script.Visible || ini_edit.Text == script_text) { return; }
        script_text = ini_edit.Text;
        setWorldIni(script_text.Replace("\n", script_newline));
    }

    private void setWorldIni(string text)
    {
        history.setFile(world, "World.ini", text);
        history.end();
        assets.forgetObjects();
        showChanged();
        showMapColor();
        screen_view.Update();
    }

    private string worldFile(string name)
    {
        var data = world.getWorldData(name);
        return data == null ? "" : GDKnyttAssetManager.loadTextFile(data);
    }

    private void selectTile(int index) => selectTiles(index, new KnyttPoint(1, 1));

    // A block of tiles, its corner kept so the whole of it stays on the tileset
    private void selectTiles(int index, KnyttPoint block)
    {
        int x = Mathf.Clamp(index % GDKnyttAssetManager.TILESET_WIDTH, 0, GDKnyttAssetManager.TILESET_WIDTH - block.x);
        int y = Mathf.Clamp(index / GDKnyttAssetManager.TILESET_WIDTH, 0, GDKnyttAssetManager.TILESET_HEIGHT - block.y);
        selected_tile = y * GDKnyttAssetManager.TILESET_WIDTH + x;
        tile_block = block;
        copied = null;
        var cursor = GetNode<Control>("%TileCursor");
        cursor.RectPosition = new Vector2(x, y) * TILE;
        cursor.RectSize = new Vector2(block.x, block.y) * TILE;
        showTileset();
    }

    private bool TileBlock => !(tile_block.x == 1 && tile_block.y == 1);

    // Whatever painting puts down: an area copied off a screen, the tileset's selection or the chosen object
    private KnyttStamp Stamp => copied ?? (TileLayer ?
        KnyttStamp.tiles(selected_tile, tile_block.x, tile_block.y, GDKnyttAssetManager.TILESET_WIDTH, GetNode<BaseButton>("%TilesetBButton").Pressed ? 128 : 0) :
        new KnyttStamp(1, 1, new[] { SelectedObject.x << 8 | SelectedObject.y }));

    private LayerButton layerButton(int layer) => GetNode<LayerButton>($"%Layer{layer}");

    private bool TileLayer => layer >= 0 && layer < KnyttArea.AREA_TILE_LAYERS;
    private bool ObjectLayer => layer >= KnyttArea.AREA_TILE_LAYERS;

    // Choosing the current layer again shows all layers and turns painting off
    private void selectLayer(int layer)
    {
        this.layer = layer == this.layer ? -1 : layer;
        if (copied != null && this.layer >= 0 && ObjectLayer != copied_objects) { copied = null; }
        screen_view.CurrentLayer = this.layer;
        screen_view.Painting = this.layer >= 0;
        for (int i = 0; i < ScreenView.LAYERS; i++) { layerButton(i).Selected = i == this.layer; }

        GetNode<Control>("%Palette").Modulate = TileLayer ? Colors.White : INACTIVE_COLOR;
        GetNode<Control>("%Tileset").MouseFilter = TileLayer ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        GetNode<BaseButton>("%TilesetAButton").Disabled = GetNode<BaseButton>("%TilesetBButton").Disabled = !TileLayer;
        GetNode<Control>("%Objects").Modulate = ObjectLayer ? Colors.White : INACTIVE_COLOR;
        foreach (string picker in new[] { "%BankBox", "%BankValue", "%ObjectValue", "%ObjectPreview" })
        {
            GetNode<Control>(picker).MouseFilter = ObjectLayer ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        }

        showBrush();
        showStatus();
        screen_view.Update();
    }

    // Object 0 erases, whatever the bank
    private KnyttPoint SelectedObject => new KnyttPoint(selected_object == 0 ? 0 : selected_bank, selected_object);

    private void selectObject(int bank, int obj)
    {
        selected_bank = bank & 0xFF;
        selected_object = obj & 0xFF;
        copied = null;
        var id = new KnyttPoint(selected_bank, selected_object);
        string bank_name = $"Bank {selected_bank}: {catalog.bankName(selected_bank)}";
        string details = catalog.details(id);

        var box = GetNode<TextureRect>("%BankBox");
        box.Texture = assets.getBank(selected_bank);
        box.HintTooltip = $"{bank_name}\nClick: all objects. Wheel: next, back";
        var bank_value = GetNode<Label>("%BankValue");
        bank_value.Text = selected_bank.ToString();
        bank_value.HintTooltip = $"{bank_name}\nClick: next. Right-click: back. Wheel works too\nShift: 10 at a time. Keys: Z, X";
        var object_value = GetNode<Label>("%ObjectValue");
        object_value.Text = selected_object.ToString();
        object_value.HintTooltip = $"{catalog.title(id)}\nClick: next. Right-click: back. Wheel works too\nShift: 10 at a time. Keys: C, V";
        var preview = GetNode<TextureRect>("%ObjectPreview");
        preview.Texture = selected_object == 0 ? null : assets.getObject(id);
        preview.HintTooltip = $"{catalog.title(id)}\n{(details == "" ? "" : details + "\n")}Click: all objects. Wheel: next, back";
        GetNode<Control>("%PlusFrame").Visible = ObjectCatalog.isPlus(id);
        showBrush();
    }

    private void openPalette()
    {
        GetTree().SetInputAsHandled();
        palette.open(new KnyttPoint(selected_bank, selected_object));
    }

    private void showBrush()
    {
        bool many = placing == Placing.None && layer >= 0 && !Stamp.Single;
        screen_view.setStamp(many ? Stamp : null, ObjectLayer, assets.getTileset(value(ScreenValue.TilesetA)), assets.getTileset(value(ScreenValue.TilesetB)));
        showHint();
        if (many) { screen_view.setBrush(null, new Rect2()); }
        else if (placing != Placing.None) { screen_view.showJuni(); }
        else if (TileLayer && selected_tile != 0)
        {
            var position = new Vector2(selected_tile % GDKnyttAssetManager.TILESET_WIDTH, selected_tile / GDKnyttAssetManager.TILESET_WIDTH);
            screen_view.setBrush(GetNode<TextureRect>("%Tileset").Texture, new Rect2(position * TILE, TILE));
        }
        else if (ObjectLayer && assets.getObject(SelectedObject) is Texture icon)
        {
            screen_view.setBrush(icon, new Rect2(Vector2.Zero, icon.GetSize()));
        }
        else { screen_view.setBrush(null, new Rect2()); }
    }

    private void showStart()
    {
        var start = TestProfile.levelDefault(world);
        start_area = start.HasStart ? start.Save.getArea() : DEFAULT_SCREEN;
        start_position = start.Save.getAreaPosition();
        screen_view.setStart(start_area, start_position);
    }

    // Only the positions change, the rest of DefaultSavegame.ini stays as it was
    private void setStart(KnyttPoint area, KnyttPoint position)
    {
        string ini = worldFile("DefaultSavegame.ini");
        ini = KnyttUtil.setIniValue(ini, "Positions", "X Map", $"{area.x}");
        ini = KnyttUtil.setIniValue(ini, "Positions", "Y Map", $"{area.y}");
        ini = KnyttUtil.setIniValue(ini, "Positions", "X Pos", $"{position.x}");
        ini = KnyttUtil.setIniValue(ini, "Positions", "Y Pos", $"{position.y}");
        history.setFile(world, "DefaultSavegame.ini", ini);
        history.end();
        showStart();
        showChanged();
    }

    // The next click on the screen says where Juni starts
    private void place(Placing what)
    {
        placing = what;
        screen_view.Placing = what != Placing.None;
        GetNode<BaseButton>("%SetStartButton").Pressed = what == Placing.Start;
        showBrush();
        showStatus(what == Placing.None ? "" : "Click where Juni starts. Right-click or Esc: cancel");
    }

    private void cancelPlacing()
    {
        bool reopen = placing == Placing.Profile;
        place(Placing.None);
        if (reopen) { test_dialog.reopen(); }
    }

    private void test(KnyttPoint? area = null, KnyttPoint? position = null)
    {
        applyScript();
        var save = TestProfile.startSave(world, test_dialog.Profile);
        if (area != null)
        {
            save.setArea(area.Value);
            save.setAreaPosition(position.Value);
        }
        world.CurrentSave = save;
        GDKnyttDataStore.KWorld = world;

        GetTree().SetInputAsHandled();
        stopPreviews();
        restoreWindow();
        tested = this;
        GetTree().Root.RemoveChild(this);
        GDKnyttDataStore.startGame(new_game: false);
    }

    // Back from a test: the game goes and the editor takes its place again
    public static void endTest()
    {
        var editor = tested;
        tested = null;
        var tree = GDKnyttDataStore.Tree;
        tree.Paused = false;
        tree.CurrentScene?.QueueFree();
        tree.Root.AddChild(editor);
        tree.CurrentScene = editor;

        editor.setupScreen();
        editor.test_dialog.LastTest = editor.world.CurrentSave;
        GDKnyttDataStore.setTitle(editor.world.WorldDirectoryName);
    }

    private void fillTestMenu()
    {
        test_profiles = test_dialog.Names;
        var menu = GetNode<MenuButton>("%TestMenu").GetPopup();
        menu.Clear();
        menu.AddItem("From start", (int)TestMenu.FromStart, (uint)KeyList.F5);
        menu.AddItem("From here", (int)TestMenu.FromHere, (uint)KeyList.F6);
        menu.AddSeparator();
        menu.AddRadioCheckItem("Level default", (int)TestMenu.Profile);
        menu.SetItemChecked(menu.GetItemCount() - 1, test_dialog.Profile == null);
        for (int i = 0; i < test_profiles.Count; i++)
        {
            menu.AddRadioCheckItem(test_profiles[i], (int)TestMenu.Profile + 1 + i);
            menu.SetItemChecked(menu.GetItemCount() - 1, test_profiles[i] == test_dialog.Profile?.Name);
        }
        menu.AddSeparator();
        menu.AddItem("Profiles", (int)TestMenu.Profiles, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.T);
        menu.AddItem(test_dialog.Profile == null ? "Use as level start" : $"Use {test_dialog.Profile.Name} as level start", (int)TestMenu.UseAsStart);
        menu.SetItemDisabled(menu.GetItemCount() - 1, test_dialog.Profile == null);
    }

    private void showLayer(int layer, bool shown)
    {
        screen_view.LayerShown[layer] = shown;
        layerButton(layer).Shown = shown;
        screen_view.Update();
    }

    // The screen to paint on, made when there's none, with the current layer shown
    private KnyttArea paintedArea()
    {
        var area = world.getArea(coords);
        if (area == null || area.Empty)
        {
            area = KnyttArea.blank(world, coords);
            foreach (var value in ScreenValues.ALL) { ScreenValues.set(area, value, this.value(value)); }
            history.addArea(world, area);
            mini_map.showScreen(world, coords);
        }
        if (!screen_view.LayerShown[layer]) { showLayer(layer, true); }
        return area;
    }

    private void paint(int x, int y)
    {
        if (layer < 0) { return; }
        var area = paintedArea();
        var stamp = Stamp;
        var cell = new KnyttPoint(x, y);
        if (stroke_origin == null) { stroke_origin = cell; }
        var at = stamp.snap(stroke_origin.Value, cell);
        for (int row = 0; row < stamp.Height; row++)
        {
            for (int column = 0; column < stamp.Width; column++)
            {
                int cell_x = at.x + column, cell_y = at.y + row;
                if (cell_x < 0 || cell_y < 0 || cell_x >= KnyttArea.AREA_WIDTH || cell_y >= KnyttArea.AREA_HEIGHT) { continue; }
                paintCell(area, cell_y * KnyttArea.AREA_WIDTH + cell_x, stamp.at(column, row));
            }
        }
        screen_view.Update();
    }

    private void paintCell(KnyttArea area, int index, int value)
    {
        if (TileLayer) { history.setTile(area, layer, index, (byte)value); }
        else { history.setObject(area, layer - KnyttArea.AREA_TILE_LAYERS, index, new KnyttPoint(value >> 8, value & 255)); }
    }

    // The current layer's cells as numbers: a tile, or an object's bank and number
    private int[] layerCells(KnyttArea area)
    {
        var cells = new int[KnyttArea.AREA_WIDTH * KnyttArea.AREA_HEIGHT];
        for (int i = 0; i < cells.Length; i++)
        {
            if (TileLayer) { cells[i] = area.TileLayers[layer].tiles[i]; }
            else
            {
                var objects = area.ObjectLayers[layer - KnyttArea.AREA_TILE_LAYERS];
                cells[i] = objects.lo[i] << 8 | objects.hi[i];
            }
        }
        return cells;
    }

    // Shift + click fills what's joined to the cell, Shift + right-click replaces it all over the layer. One undo step each
    private void fill(int x, int y, bool replace)
    {
        if (layer < 0) { return; }
        var area = paintedArea();
        var cells = layerCells(area);
        int start = y * KnyttArea.AREA_WIDTH + x;
        var changed = replace ? KnyttFill.matching(cells, cells[start]) : KnyttFill.flood(cells, KnyttArea.AREA_WIDTH, start);
        var stamp = Stamp;

        history.begin();
        if (ObjectLayer && selected_object != 0 && copied == null) { palette.addRecent(SelectedObject); }
        foreach (int index in changed) { paintCell(area, index, stamp.at(index % KnyttArea.AREA_WIDTH - x, index / KnyttArea.AREA_WIDTH - y)); }
        history.end();
        screen_view.Update();
        showChanged();
    }

    private void pick(int x, int y)
    {
        var area = world.getArea(coords);
        if (layer < 0 || area == null || area.Empty) { return; }
        if (ObjectLayer)
        {
            var id = area.ObjectLayers[layer - KnyttArea.AREA_TILE_LAYERS].getObjectID(x, y);
            selectObject(id.x, id.y);
            return;
        }

        int tile = area.TileLayers[layer].getTile(x, y);
        if (tile % 128 == 0) { return; }
        GetNode<BaseButton>(tile < 128 ? "%TilesetAButton" : "%TilesetBButton").Pressed = true;
        selectTile(tile % 128);
    }

    private void refresh()
    {
        assets.forgetObjects();
        showStart();
        goTo(coords);
        test_dialog.refresh();
    }

    private void showStatus(string message = "")
    {
        status_message = message;
        updateStatus();
    }

    // What's under the cursor: the object on the current layer, or the top one when all layers show
    private void updateStatus()
    {
        string layer_text = layer < 0 ? "All layers" : $"Layer {layer}";
        string text = $"Map: x{coords.x}y{coords.y}   {layer_text}   ";

        var area = world.getArea(coords);
        if (hovered is KnyttPoint cell && area != null && !area.Empty && (ObjectLayer || layer < 0))
        {
            for (int i = ScreenView.LAYERS - 1; i >= KnyttArea.AREA_TILE_LAYERS; i--)
            {
                if ((ObjectLayer && i != layer) || !screen_view.LayerShown[i]) { continue; }
                var id = area.ObjectLayers[i - KnyttArea.AREA_TILE_LAYERS].getObjectID(cell.x, cell.y);
                if (id.y == 0) { continue; }
                text += catalog.title(id) + "   ";
                break;
            }
        }
        GetNode<Label>("%StatusLabel").Text = text + status_message;
    }

    private bool mouseOver(params string[] nodes)
    {
        var at = GetGlobalMousePosition();
        return nodes.Any(node => GetNode<Control>(node).GetGlobalRect().HasPoint(at));
    }

    private void showHint()
    {
        string text = hint();
        var label = GetNode<Label>("%HintLabel");
        if (label.Text != text) { label.Text = text; }
    }

    private static string keys(params string[] items) => string.Join("     ", items.Where(item => item != ""));

    // The keys that do something where the mouse is. A held key shows what it changes
    private string hint()
    {
        if (palette.Visible || GetNode<Control>("%WorldView").Visible) { return ""; }
        bool shift = Input.IsKeyPressed((int)KeyList.Shift);
        bool ctrl = Input.IsKeyPressed((int)KeyList.Control);

        if (mouseOver("%ScreenView"))
        {
            if (placing != Placing.None) { return keys("Click place", "Right-click cancel"); }
            if (layer < 0) { return keys("0-7 paint on a layer", "Right-click menu"); }
            if (screen_view.Peeking) { return "All layers while Space is held"; }
            if (ctrl) { return "Drag copy an area as the brush"; }
            if (shift) { return keys("Click fill", "Right-click replace"); }
            return keys("Shift+click fill", "Shift+right-click replace", "Ctrl+drag copy", Stamp.Single ? "" : "F one tile", PeekHint);
        }
        if (TileLayer && mouseOver("%Tileset"))
        {
            return TileBlock ? keys("Right-click move", "WASD nudge", "F one tile", "Q E tileset") : keys("Drag select an area", "Q E tileset");
        }
        if (ObjectLayer && mouseOver("%Objects")) { return keys("Z X bank", "C V object", "Shift 10 at a time"); }
        if (mouseOver("%Layer0", "%Layer1", "%Layer2", "%Layer3", "%Layer4", "%Layer5", "%Layer6", "%Layer7"))
        {
            return shift ? keys("Click only this layer", "Right-click all layers") : keys("Right-click hide", "Shift+click only this", "Shift+right-click all", "0-7 keys", PeekHint);
        }
        if (mouseOver("Panel/Tabs/BackgroundTab", "Panel/Tabs/TilesetATab", "Panel/Tabs/TilesetBTab", "Sounds/MusicRow", "Sounds/AmbianceARow", "Sounds/AmbianceBRow"))
        {
            return shift ? keys("Click +10", "Right-click -10") : keys("Click +1", "Right-click -1", "Wheel", "Shift 10 at a time");
        }
        if (mouseOver("MiniMap")) { return keys("Click go there", "Arrows next screen", "M world map"); }
        return "F1 all keys";
    }

    private string PeekHint => screen_view.FadeOthers && layer >= 0 ? "Space all layers" : "";

    private void showTileset()
    {
        bool show_b = GetNode<BaseButton>("%TilesetBButton").Pressed;
        GetNode<TextureRect>("%Tileset").Texture = assets.getTileset(value(show_b ? ScreenValue.TilesetB : ScreenValue.TilesetA));
        showBrush();
    }

    private bool Resizable => OS.HasFeature("pc") && !OS.WindowFullscreen && !OS.WindowMaximized;

    private Vector2 ScreenSize => script.Visible ? SCRIPT_SIZE : SCREEN_SIZE;

    private void setupScreen()
    {
        game_window_size = OS.WindowSize;
        stretch();
    }

    private void stretch()
    {
        GetTree().SetScreenStretch(
            GDKnyttSettings.SmoothScaling ? SceneTree.StretchMode.Mode2d : SceneTree.StretchMode.Viewport,
            SceneTree.StretchAspect.Keep, ScreenSize);
        fitWindow();
    }

    private void restoreWindow()
    {
        if (!Resizable) { return; }
        OS.WindowSize = game_window_size;
        OS.CenterWindow();
    }

    private void fitWindow()
    {
        if (!Resizable) { return; }
        var screen = OS.GetScreenSize() * 0.9f;
        OS.WindowSize = ScreenSize * Mathf.Max(1, Mathf.Floor(Mathf.Min(screen.x / ScreenSize.x, screen.y / ScreenSize.y)));
        OS.CenterWindow();
    }

    private void save()
    {
        applyScript();
        if (world.WorldDirectory.StartsWith("res://")) { showSaveAsDialog(); return; }
        if (!overwrite_allowed.Contains(world.WorldDirectory))
        {
            overwrite_dialog.DialogText = $"This will overwrite\n{world.WorldDirectory.GetFile()}\n\nThis can't be undone. Use Save as to keep the original.";
            overwrite_dialog.PopupCentered();
            return;
        }
        showSaveResult(world.save());
    }

    private void showSaveAsDialog()
    {
        GetNode<LineEdit>("%SaveAsName").Text = world.Info.Name;
        GetNode<LineEdit>("%SaveAsAuthor").Text = world.Info.Author;
        GetNode<Label>("%SaveAsError").Text = "";
        save_as_dialog.PopupCentered();
        GetNode<LineEdit>("%SaveAsName").GrabFocus();
    }

    private void saveAs()
    {
        string name = GetNode<LineEdit>("%SaveAsName").Text.Trim();
        string author = GetNode<LineEdit>("%SaveAsAuthor").Text.Trim();
        string error = WorldName.check(name, author);
        if (error != null) { GetNode<Label>("%SaveAsError").Text = error; return; }

        save_as_dialog.Hide();
        var result = world.saveAs(WorldName.path(name, author), name, author);
        if (result == Error.Ok) { overwrite_allowed.Add(world.WorldDirectory); history.markSaved(); }
        GDKnyttDataStore.setTitle(world.WorldDirectoryName);
        showSaveResult(result);
        showScript();
    }

    private void showSaveResult(Error result)
    {
        if (result == Error.Ok) { history.markSaved(); }
        showChanged();
        showStatus(result == Error.Ok ? $"Saved {world.WorldDirectory.GetFile()}" : $"Can't save ({result})");
    }

    private void quit(bool game = false)
    {
        applyScript();
        if (history.Modified)
        {
            quit_game = game;
            unsaved_dialog.PopupCentered();
            return;
        }
        if (game) { GetTree().Quit(); return; }
        stopPreviews();
        restoreWindow();
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest) { quit(game: true); }
        if (what == MainLoop.NotificationWmGoBackRequest) { quit(); }
    }

    // Clicking anywhere else lets go of a script field, which keeps what was typed
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion || @event is InputEventKey) { showHint(); }

        var focus = GetFocusOwner();

        // Space held shows every layer at full strength. Caught here so a focused button isn't pressed; text fields keep it
        if (@event is InputEventKey peek && peek.Scancode == (uint)KeyList.Space && !(focus is LineEdit || focus is TextEdit) &&
            !(goto_dialog.Visible || overwrite_dialog.Visible || save_as_dialog.Visible || unsaved_dialog.Visible || keys_dialog.Visible || palette.Visible))
        {
            GetTree().SetInputAsHandled();
            if (!peek.Echo && screen_view.Peeking != peek.Pressed)
            {
                screen_view.Peeking = peek.Pressed;
                screen_view.Update();
                showHint();
            }
            return;
        }

        if (@event is InputEventMouseButton button && button.Pressed && focus != null && script.IsAParentOf(focus) &&
            !focus.GetGlobalRect().HasPoint(button.Position))
        {
            focus.ReleaseFocus();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (goto_dialog.Visible || overwrite_dialog.Visible || save_as_dialog.Visible || unsaved_dialog.Visible || keys_dialog.Visible || palette.Visible) { return; }

        // Esc lets go of a script field, stops placing, then stops painting. Back and F2 leave
        bool cancel = @event.IsActionPressed("ui_cancel");
        if (cancel || @event.IsActionPressed("ui_back") || @event.IsActionPressed("main_menu"))
        {
            GetTree().SetInputAsHandled();
            var focus = GetFocusOwner();
            if (focus != null && script.IsAParentOf(focus)) { focus.ReleaseFocus(); }
            else if (placing != Placing.None) { cancelPlacing(); }
            else if (!cancel) { quit(); }
            else if (layer >= 0)
            {
                selectLayer(layer);
                showStatus("Painting off. A layer key or diamond paints again");
            }
        }
        else if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Scancode >= (uint)KeyList.Key0 && key.Scancode <= (uint)KeyList.Key7)
        {
            GetTree().SetInputAsHandled();
            selectLayer((int)(key.Scancode - (uint)KeyList.Key0));
        }
        else if (@event is InputEventKey ks_key && ks_key.Pressed && !ks_key.Echo && !ks_key.Control && ksKey(ks_key))
        {
            GetTree().SetInputAsHandled();
        }
        else if (TileLayer && TileBlock && copied == null && @event is InputEventKey nudge && nudge.Pressed && !nudge.Control && tileKeys(nudge) is KnyttPoint move)
        {
            GetTree().SetInputAsHandled();
            var at = new KnyttPoint(selected_tile % GDKnyttAssetManager.TILESET_WIDTH, selected_tile / GDKnyttAssetManager.TILESET_WIDTH) + move;
            selectTiles(Mathf.Clamp(at.y, 0, GDKnyttAssetManager.TILESET_HEIGHT - 1) * GDKnyttAssetManager.TILESET_WIDTH + Mathf.Clamp(at.x, 0, GDKnyttAssetManager.TILESET_WIDTH - 1), tile_block);
        }
        else if (direction(@event) is KnyttPoint step)
        {
            GetTree().SetInputAsHandled();
            goTo(coords + step);
        }
        else if (ObjectLayer && @event is InputEventKey letter && letter.Pressed && !letter.Control && objectKeys(letter) is KnyttPoint change)
        {
            GetTree().SetInputAsHandled();
            int size = letter.Shift ? BIG_STEP : 1;
            selectObject(selected_bank + change.x * size, selected_object + change.y * size);
        }
    }

    // KS+ keys: Q and E show tileset A and B, F3 opens the script panel, F goes back to one tile
    private bool ksKey(InputEventKey key)
    {
        if (key.Scancode == (uint)KeyList.F3)
        {
            var button = GetNode<Button>("%ScriptButton");
            button.Pressed = !button.Pressed;
        }
        else if (key.Scancode == (uint)KeyList.F && (copied != null || TileBlock))
        {
            copied = null;
            selectTile(selected_tile);
        }
        else if (key.Scancode == (uint)KeyList.Q) { GetNode<BaseButton>("%TilesetAButton").Pressed = true; }
        else if (key.Scancode == (uint)KeyList.E) { GetNode<BaseButton>("%TilesetBButton").Pressed = true; }
        else { return false; }
        return true;
    }

    // KS+ keys: WASD move the tileset's selection
    private static KnyttPoint? tileKeys(InputEventKey key) =>
        key.Scancode == (uint)KeyList.A ? new KnyttPoint(-1, 0) :
        key.Scancode == (uint)KeyList.D ? new KnyttPoint(1, 0) :
        key.Scancode == (uint)KeyList.W ? new KnyttPoint(0, -1) :
        key.Scancode == (uint)KeyList.S ? new KnyttPoint(0, 1) : (KnyttPoint?)null;

    // KS+ keys: Z and X step the bank, C and V the object
    private static KnyttPoint? objectKeys(InputEventKey key) =>
        key.Scancode == (uint)KeyList.Z ? new KnyttPoint(-1, 0) :
        key.Scancode == (uint)KeyList.X ? new KnyttPoint(1, 0) :
        key.Scancode == (uint)KeyList.C ? new KnyttPoint(0, -1) :
        key.Scancode == (uint)KeyList.V ? new KnyttPoint(0, 1) : (KnyttPoint?)null;

    private static KnyttPoint? direction(InputEvent @event) =>
        @event.IsActionPressed("ui_left") ? new KnyttPoint(-1, 0) :
        @event.IsActionPressed("ui_right") ? new KnyttPoint(1, 0) :
        @event.IsActionPressed("ui_up") ? new KnyttPoint(0, -1) :
        @event.IsActionPressed("ui_down") ? new KnyttPoint(0, 1) : (KnyttPoint?)null;

    private void showGoToDialog()
    {
        var edit = GetNode<LineEdit>("%GoToEdit");
        edit.Text = $"x{coords.x}y{coords.y}";
        goto_dialog.PopupCentered();
        edit.GrabFocus();
        edit.SelectAll();
    }

    private void _on_HelpMenu_id_pressed(int id) => keys_dialog.PopupCentered();

    private void _on_FileMenu_id_pressed(int id)
    {
        switch ((FileMenu)id)
        {
            case FileMenu.Save: save(); break;
            case FileMenu.SaveAs: showSaveAsDialog(); break;
            case FileMenu.Quit: quit(); break;
        }
    }

    private void _on_EditMenu_id_pressed(int id)
    {
        if ((EditMenu)id == EditMenu.LevelStart) { test_dialog.open(level: true); return; }
        applyScript();
        bool changed = (EditMenu)id == EditMenu.Undo ? history.undo() : history.redo();
        if (changed) { refresh(); }
    }

    private void _on_OptionsMenu_id_pressed(int id)
    {
        var menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        int index = menu.GetItemIndex(id);
        menu.SetItemChecked(index, !menu.IsItemChecked(index));
        if ((OptionsMenu)id == OptionsMenu.FadeLayers) { screen_view.FadeOthers = menu.IsItemChecked(index); }
        if ((OptionsMenu)id == OptionsMenu.FollowScript) { follow_script = menu.IsItemChecked(index); }
        if ((OptionsMenu)id == OptionsMenu.Guides) { showGuides(menu.IsItemChecked(index), save: true); }
        if ((OptionsMenu)id == OptionsMenu.Neighbours) { showNeighbours(menu.IsItemChecked(index), save: true); }
        screen_view.Update();
    }

    private void _on_GoToMenu_id_pressed(int id)
    {
        switch ((GoToMenu)id)
        {
            case GoToMenu.Start: goTo(start_area); break;
            case GoToMenu.Default: goTo(DEFAULT_SCREEN); break;
            case GoToMenu.Coordinates: showGoToDialog(); break;
            case GoToMenu.WorldMap: openWorldView(); break;
        }
    }

    private void _on_GoToDialog_confirmed()
    {
        var match = Regex.Match(GetNode<LineEdit>("%GoToEdit").Text, @"x\s*(-?\d+)\s*y\s*(-?\d+)", RegexOptions.IgnoreCase);
        if (match.Success) { goTo(new KnyttPoint(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value))); }
    }

    private void _on_GoToEdit_text_entered(string text)
    {
        goto_dialog.Hide();
        _on_GoToDialog_confirmed();
    }

    private void _on_TestMenu_id_pressed(int id)
    {
        switch ((TestMenu)id)
        {
            case TestMenu.FromStart: test(); break;
            case TestMenu.FromHere: testFromHere(); break;
            case TestMenu.Profiles: test_dialog.open(); break;
            case TestMenu.UseAsStart:
                test_dialog.useAsLevelStart(test_dialog.Profile);
                showStatus($"Level start: {test_dialog.Profile.Name}");
                break;
            default:
                int index = id - (int)TestMenu.Profile - 1;
                test_dialog.choose(index < 0 ? null : test_profiles[index]);
                fillTestMenu();
                break;
        }
    }

    // From the cell under the cursor, or from the next click
    private void testFromHere()
    {
        if (hovered is KnyttPoint cell) { test(coords, cell); }
        else { place(Placing.Test); }
    }

    private void _on_TestLevelButton_pressed()
    {
        test();
    }

    private void _on_TestLevelButton_gui_input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton button) || !button.Pressed || button.ButtonIndex != (int)ButtonList.Right) { return; }
        GetTree().SetInputAsHandled();
        test_dialog.open();
    }

    private void _on_SetStartButton_pressed()
    {
        place(placing == Placing.Start ? Placing.None : Placing.Start);
    }

    private void _on_TestDialog_confirmed()
    {
        test_dialog.chooseShown();
        test();
    }

    private void _on_TestDialog_LevelChanged(string text, bool done)
    {
        history.setFile(world, "DefaultSavegame.ini", text);
        if (done) { history.end(); }
        showStart();
        showChanged();
    }

    private void _on_SetStartButton_gui_input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton button) || !button.Pressed || button.ButtonIndex != (int)ButtonList.Right) { return; }
        GetTree().SetInputAsHandled();
        test_dialog.open(level: true);
    }

    private void _on_TestDialog_StartPickRequested()
    {
        place(Placing.Profile);
    }

    private void _on_OverwriteDialog_confirmed()
    {
        overwrite_allowed.Add(world.WorldDirectory);
        save();
    }

    private void _on_SaveAsDialog_confirmed()
    {
        saveAs();
    }

    private void _on_SaveAsEdit_text_entered(string text)
    {
        saveAs();
    }

    private void _on_UnsavedDialog_confirmed()
    {
        history = new EditorHistory();
        quit(quit_game);
    }

    private void _on_ScreenView_StrokeStarted()
    {
        history.begin();
        stroke_origin = null;
        if (ObjectLayer && selected_object != 0 && copied == null) { palette.addRecent(SelectedObject); }
    }

    // One cell is the same as picking it
    private void _on_ScreenView_AreaCopied(int x, int y, int width, int height)
    {
        if (width == 1 && height == 1) { pick(x, y); return; }
        var area = world.getArea(coords);
        var cells = area == null || area.Empty ? new int[KnyttArea.AREA_WIDTH * KnyttArea.AREA_HEIGHT] : layerCells(area);
        copied = KnyttStamp.cut(cells, KnyttArea.AREA_WIDTH, x, y, width, height);
        copied_objects = ObjectLayer;
        showBrush();
    }

    private void _on_ScreenView_CellPainted(int x, int y)
    {
        paint(x, y);
    }

    private void _on_ScreenView_CellFilled(int x, int y)
    {
        fill(x, y, false);
    }

    private void _on_ScreenView_CellReplaced(int x, int y)
    {
        fill(x, y, true);
    }

    private void _on_ScreenView_StrokeEnded()
    {
        history.end();
        showChanged();
    }

    // Right-click, or a long press on touch
    private void _on_ScreenView_CellMenu(int x, int y)
    {
        if (placing != Placing.None) { cancelPlacing(); return; }
        menu_cell = new KnyttPoint(x, y);
        var area = world.getArea(coords);
        var menu = GetNode<PopupMenu>("%CellMenu");
        menu.SetItemDisabled((int)CellMenu.Pick, layer < 0 || area == null || area.Empty);
        menu.SetItemDisabled((int)CellMenu.Fill, layer < 0);
        menu.SetItemDisabled((int)CellMenu.Replace, layer < 0);
        menu.SetItemText((int)CellMenu.Test, test_dialog.Profile == null ? "Test from here" : $"Test from here: {test_dialog.Profile.Name}");
        showObjectKeys(menu, area, menu_cell);
        var size = menu.GetCombinedMinimumSize();
        var view = GetViewportRect().Size;
        var at = GetGlobalMousePosition();
        menu.RectSize = Vector2.Zero;
        menu.RectPosition = new Vector2(Mathf.Min(at.x, view.x - size.x), Mathf.Min(at.y, view.y - size.y));
        menu.Popup_();
    }

    private void _on_CellMenu_id_pressed(int id)
    {
        GetNode<WorldView>("%WorldView").close();
        switch ((CellMenu)id)
        {
            case CellMenu.Pick: pick(menu_cell.x, menu_cell.y); break;
            case CellMenu.Fill: fill(menu_cell.x, menu_cell.y, false); break;
            case CellMenu.Replace: fill(menu_cell.x, menu_cell.y, true); break;
            case CellMenu.Test: test(coords, menu_cell); break;
            case CellMenu.Start: setStart(coords, menu_cell); break;
            default: editKey(menu_keys[id - CELL_MENU_KEYS].section ?? ScreenSection, menu_keys[id - CELL_MENU_KEYS].key); break;
        }
    }

    private const int CELL_MENU_KEYS = 10;

    // One item for each object on the cell with World.ini data, under the fixed ones
    private void showObjectKeys(PopupMenu menu, KnyttArea area, KnyttPoint cell)
    {
        while (menu.GetItemCount() > (int)CellMenu.Start + 1) { menu.RemoveItem(menu.GetItemCount() - 1); }
        menu_keys.Clear();
        if (area == null || area.Empty) { return; }
        foreach (var objects in area.ObjectLayers)
        {
            var key = objectKey(objects.getObjectID(cell.x, cell.y));
            if (key == null || menu_keys.Any(shown => shown.label == key.label)) { continue; }
            if (menu_keys.Count == 0) { menu.AddSeparator(); }
            menu.AddItem(key.label, CELL_MENU_KEYS + menu_keys.Count);
            menu_keys.Add(key);
        }
    }

    // The form field for a key, the form and the letters it needs
    private Control findField(string section, string key, out IniForm form, out string[] letters)
    {
        letters = null;
        foreach (IniForm each in GetNode("%Forms").GetChildren())
        {
            form = each;
            var field = each.find(section, key, out letters);
            if (field != null) { return field; }
        }
        form = null;
        return null;
    }

    private void editKey(string section, string key)
    {
        var field = findField(section, key, out var form, out var letters);
        if (field != null) { showField(form, letters, field); }
    }

    // Opens the script panel at a form, with the letter chosen and the field ready to type in
    private void showField(IniForm form, string[] letters, Control field)
    {
        GetNode<Button>("%ScriptButton").Pressed = true;
        GetNode<TabContainer>("%Forms").CurrentTab = form.GetIndex();
        if (letters != null) { form.choose(letters); }
        if (field is LineEdit edit)
        {
            edit.GrabFocus();
            edit.CaretPosition = edit.Text.Length;
        }
    }

    // What's on a line of the text: its section, the screen that is, and the form field for its key.
    // On a section's own line, a form for the section when it isn't a screen
    private void readIniLine(int line)
    {
        var lines = ini_edit.Text.Split('\n');
        ini_section = null;
        ini_screen = null;
        ini_field = null;
        for (int i = line; i >= 0 && ini_section == null; i--)
        {
            var header = Regex.Match(lines[i], @"^\s*\[([^\]]+)\]");
            if (header.Success) { ini_section = header.Groups[1].Value; }
        }
        if (ini_section == null) { return; }

        var screen = Regex.Match(ini_section, @"^x(-?\d+)y(-?\d+)$", RegexOptions.IgnoreCase);
        if (screen.Success) { ini_screen = new KnyttPoint(int.Parse(screen.Groups[1].Value), int.Parse(screen.Groups[2].Value)); }
        ini_key = lines[line].Contains("=") ? lines[line].Split('=')[0].Trim() : null;
        if (ini_key == null && ini_screen != null) { return; }
        ini_field = findField(ini_section, ini_key, out ini_form, out ini_letters);
    }

    // Right-click in the text: go to the screen whose section it's in, or edit the line in its form
    private void fillIniMenu()
    {
        var menu = ini_edit.GetMenu();
        if (ini_menu_items < 0) { ini_menu_items = menu.GetItemCount(); }
        while (menu.GetItemCount() > ini_menu_items) { menu.RemoveItem(menu.GetItemCount() - 1); }

        readIniLine(ini_edit.CursorGetLine());
        bool away = ini_screen != null && !ini_screen.Value.Equals(coords);
        if (away || ini_field != null) { menu.AddSeparator(); }
        if (away) { menu.AddItem($"Go to {ini_section}", INI_GOTO); }
        if (ini_field != null) { menu.AddItem($"Edit {ini_key ?? $"[{ini_section}]"} in its form", INI_FORM); }
    }

    private void _on_IniMenu_id_pressed(int id)
    {
        if (id != INI_GOTO && id != INI_FORM) { return; }
        if (ini_screen != null && !ini_screen.Value.Equals(coords)) { goTo(ini_screen.Value); }
        if (id == INI_FORM) { showField(ini_form, ini_letters, ini_field); }
    }

    private void _on_ScreenView_CellPlaced(int x, int y)
    {
        var what = placing;
        var spot = new KnyttPoint(x, y);
        place(Placing.None);
        switch (what)
        {
            case Placing.Start: setStart(coords, spot); break;
            case Placing.Test: test(coords, spot); break;
            case Placing.Profile:
                if (test_dialog.LevelShown) { setStart(coords, spot); }
                else { test_dialog.setStart(coords, spot); }
                test_dialog.reopen();
                break;
        }
    }

    private void _on_ScreenView_CellHovered(int x, int y)
    {
        hovered = x < 0 ? (KnyttPoint?)null : new KnyttPoint(x, y);
        updateStatus();
    }

    // Click selects a tile, drag an area, right-click moves the area
    private void _on_Tileset_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && tile_drag != null) { dragTiles(tileAt(motion.Position)); }
        if (!(@event is InputEventMouseButton button)) { return; }
        var cell = tileAt(button.Position);

        if (button.ButtonIndex == (int)ButtonList.Left && !button.Pressed) { tile_drag = null; }
        if (!button.Pressed || button.Position.y >= GDKnyttAssetManager.TILESET_HEIGHT * TILE.y) { return; }
        if (button.ButtonIndex == (int)ButtonList.Left)
        {
            tile_drag = cell;
            dragTiles(cell);
        }
        else if (button.ButtonIndex == (int)ButtonList.Right && TileBlock)
        {
            selectTiles(cell.y * GDKnyttAssetManager.TILESET_WIDTH + cell.x, tile_block);
        }
    }

    private KnyttPoint tileAt(Vector2 position) => new KnyttPoint(
        Mathf.Clamp((int)(position.x / TILE.x), 0, GDKnyttAssetManager.TILESET_WIDTH - 1),
        Mathf.Clamp((int)(position.y / TILE.y), 0, GDKnyttAssetManager.TILESET_HEIGHT - 1));

    private void dragTiles(KnyttPoint cell)
    {
        var start = tile_drag.Value;
        var corner = start.min(cell);
        var far = start.max(cell);
        selectTiles(corner.y * GDKnyttAssetManager.TILESET_WIDTH + corner.x, far - corner + new KnyttPoint(1, 1));
    }

    // Click or wheel up steps forward; right-click, wheel down or a long press steps back. Shift steps by 10
    private int stepOf(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton button)) { return 0; }

        int step;
        if (button.Device == TOUCH && button.ButtonIndex == (int)ButtonList.Left)
        {
            if (button.Pressed) { press_time = OS.GetTicksMsec(); return 0; }
            step = OS.GetTicksMsec() - press_time >= LONG_PRESS_MS ? -1 : 1;
        }
        else if (!button.Pressed) { return 0; }
        else if (button.ButtonIndex == (int)ButtonList.Left || button.ButtonIndex == (int)ButtonList.WheelUp) { step = 1; }
        else if (button.ButtonIndex == (int)ButtonList.Right || button.ButtonIndex == (int)ButtonList.WheelDown) { step = -1; }
        else { return 0; }

        GetTree().SetInputAsHandled();
        return button.Shift ? step * BIG_STEP : step;
    }

    private void _on_ScriptButton_toggled(bool pressed)
    {
        applyScript();
        script.Visible = pressed;
        GetNode<Button>("%ScriptButton").Text = pressed ? "Script <" : "Script >";
        stretch();
        showScript();
    }

    private void _on_IniEdit_text_changed()
    {
        showChanged();
        showForms();
    }

    // Follow: the screen and the form go where the text cursor is
    private void _on_IniEdit_cursor_changed()
    {
        if (!follow_script || !ini_edit.HasFocus()) { return; }
        readIniLine(ini_edit.CursorGetLine());
        if (ini_screen != null && !ini_screen.Value.Equals(coords))
        {
            following = true;
            goTo(ini_screen.Value);
            following = false;
        }
        if (ini_field == null) { return; }
        GetNode<TabContainer>("%Forms").CurrentTab = ini_form.GetIndex();
        ini_form.choose(ini_letters);
    }

    private void _on_IniEdit_focus_exited()
    {
        applyScript();
    }

    // Form edits go into the text as they're typed, and into the level when the field is done
    private void _on_Form_Edited(string section, string key, string value)
    {
        setScriptText(KnyttUtil.setIniValue(ini_edit.Text, section, key, value == "" ? null : value));
        showKey(section, key);
    }

    private void _on_Form_Done()
    {
        applyScript();
    }

    private void _on_Form_Focused(string section, string key)
    {
        showKey(section, key);
    }

    private void _on_Value_gui_input(InputEvent @event, int which)
    {
        int step = stepOf(@event);
        if (step != 0) { changeValue((ScreenValue)which, step); }
    }

    private void _on_Play_toggled(bool pressed)
    {
        showPreviews();
    }

    // All on, or all off when they all play
    private void _on_PlayAll_pressed()
    {
        bool all = true;
        foreach (var sound in SOUNDS) { all &= GetNode<BaseButton>($"%Play{sound}").Pressed; }
        foreach (var sound in SOUNDS) { GetNode<BaseButton>($"%Play{sound}").SetPressedNoSignal(!all); }
        showPreviews();
    }

    private void _on_Bank_gui_input(InputEvent @event)
    {
        int step = stepOf(@event);
        if (step != 0) { selectObject(selected_bank + step, selected_object); }
    }

    private void _on_Object_gui_input(InputEvent @event)
    {
        int step = stepOf(@event);
        if (step != 0) { selectObject(selected_bank, selected_object + step); }
    }

    private static bool clicked(InputEvent @event) =>
        @event is InputEventMouseButton button && button.Pressed && button.ButtonIndex == (int)ButtonList.Left;

    private static bool isWheel(InputEvent @event) =>
        @event is InputEventMouseButton button && (button.ButtonIndex == (int)ButtonList.WheelUp || button.ButtonIndex == (int)ButtonList.WheelDown);

    // The pictures open the palette, the wheel still steps
    private void _on_BankBox_gui_input(InputEvent @event)
    {
        if (clicked(@event)) { openPalette(); }
        else if (isWheel(@event)) { _on_Bank_gui_input(@event); }
    }

    private void _on_ObjectPreview_gui_input(InputEvent @event)
    {
        if (clicked(@event)) { openPalette(); }
        else if (isWheel(@event)) { _on_Object_gui_input(@event); }
    }

    private void _on_ObjectPalette_ObjectChosen(int bank, int obj)
    {
        selectObject(bank, obj);
    }

    private void _on_Layer_Chosen(int layer)
    {
        selectLayer(layer);
    }

    private void _on_Layer_VisibilityToggled(int layer)
    {
        showLayer(layer, !screen_view.LayerShown[layer]);
    }

    private void _on_Layer_Solo(int layer)
    {
        for (int i = 0; i < ScreenView.LAYERS; i++) { showLayer(i, i == layer); }
    }

    private void _on_Layer_AllShown()
    {
        for (int i = 0; i < ScreenView.LAYERS; i++) { showLayer(i, true); }
    }

    private void openWorldView()
    {
        GetNode<WorldView>("%WorldView").open(coords, start_area, start_position);
    }

    private void _on_ExpandMapButton_pressed()
    {
        openWorldView();
    }

    private void _on_WorldView_ScreenChosen(int x, int y)
    {
        GetNode<WorldView>("%WorldView").close();
        goTo(new KnyttPoint(x, y));
    }

    // A mark or line on the map: its screen, and its form when it has one
    private void _on_WorldView_ThingChosen(int x, int y, int bank, int obj, string key)
    {
        _on_WorldView_ScreenChosen(x, y);
        var found = bank >= 0 ? objectKey(new KnyttPoint(bank, obj)) : key != "" ? new ObjectKey("", key) : null;
        if (found != null) { editKey(found.section ?? ScreenSection, found.key); }
    }

    // The cell menu works on the screen it was opened over
    private void _on_WorldView_ScreenMenu(int x, int y, int cell_x, int cell_y)
    {
        goTo(new KnyttPoint(x, y));
        _on_ScreenView_CellMenu(cell_x, cell_y);
    }

    private void _on_MiniMap_ScreenSelected(int x, int y)
    {
        goTo(new KnyttPoint(x, y));
    }

    // The colour this screen has on YKnytt's map: MapColor, or its gradient's
    private void showMapColor()
    {
        var area = world.getArea(coords);
        var swatch = GetNode<ColorRect>("%MapColor");
        swatch.Color = area != null && !area.Empty ? MapPanel.areaColor(area) : MapPanel.backgroundColor(value(ScreenValue.Background));
        swatch.Visible = area == null || area.Empty || MapPanel.areaVisible(area) != false;
    }

    private void showGuides(bool on, bool save = false)
    {
        screen_view.Guides = on;
        GetNode<BaseButton>("%GuidesButton").SetPressedNoSignal(on);
        var menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        menu.SetItemChecked(menu.GetItemIndex((int)OptionsMenu.Guides), on);
        screen_view.Update();
        if (save) { GDKnyttSettings.EditorGuides = on; GDKnyttSettings.saveSettings(); }
    }

    private void showNeighbours(bool on, bool save = false)
    {
        screen_view.Neighbours = on;
        GetNode<BaseButton>("%NeighboursButton").SetPressedNoSignal(on);
        var menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        menu.SetItemChecked(menu.GetItemIndex((int)OptionsMenu.Neighbours), on);
        screen_view.Update();
        if (save) { GDKnyttSettings.EditorNeighbours = on; GDKnyttSettings.saveSettings(); }
    }

    private void showBorderColor(int color, bool save = false)
    {
        color = KnyttPoint.positiveMod(color, BORDER_COLORS.Length);
        screen_view.BorderColor = BORDER_COLORS[color];
        GetNode<CanvasItem>("%BorderButton").SelfModulate = BUTTON_COLORS[color];
        screen_view.Update();
        if (save) { GDKnyttSettings.EditorBorderColor = color; GDKnyttSettings.saveSettings(); }
    }

    // The checks are made here: two tones of each colour, 6 pixels square
    private void showTilesetColor(int color, bool save = false)
    {
        color = KnyttPoint.positiveMod(color, CHECKS.GetLength(0));
        var image = new Image();
        image.Create(CHECK_SIZE * 2, CHECK_SIZE * 2, false, Image.Format.Rgb8);
        image.Lock();
        for (int y = 0; y < CHECK_SIZE * 2; y++)
        {
            for (int x = 0; x < CHECK_SIZE * 2; x++) { image.SetPixel(x, y, CHECKS[color, (x / CHECK_SIZE + y / CHECK_SIZE) % 2]); }
        }
        image.Unlock();
        var checks = new ImageTexture();
        checks.CreateFromImage(image, (uint)Texture.FlagsEnum.Repeat);
        GetNode<TextureRect>("%Checker").Texture = checks;
        GetNode<CanvasItem>("%TilesetColorButton").SelfModulate = color == CHECKS.GetLength(0) - 1 ? TILESET_BUTTON_DEFAULT : BUTTON_COLORS[color];
        if (save) { GDKnyttSettings.EditorTilesetColor = color; GDKnyttSettings.saveSettings(); }
    }

    private void _on_GuidesButton_toggled(bool pressed)
    {
        showGuides(pressed, save: true);
    }

    private void _on_NeighboursButton_toggled(bool pressed)
    {
        showNeighbours(pressed, save: true);
    }

    // Click: next colour, right-click: back
    private void _on_BorderButton_gui_input(InputEvent @event)
    {
        if (colorStep(@event) is int step) { showBorderColor(GDKnyttSettings.EditorBorderColor + step, save: true); }
    }

    private void _on_TilesetColorButton_gui_input(InputEvent @event)
    {
        if (colorStep(@event) is int step) { showTilesetColor(GDKnyttSettings.EditorTilesetColor + step, save: true); }
    }

    private static int? colorStep(InputEvent @event) =>
        !(@event is InputEventMouseButton button) || !button.Pressed ? (int?)null :
        button.ButtonIndex == (int)ButtonList.Left ? 1 : button.ButtonIndex == (int)ButtonList.Right ? -1 : (int?)null;

    private void _on_TilesetButton_toggled(bool pressed)
    {
        showTileset();
    }
}
