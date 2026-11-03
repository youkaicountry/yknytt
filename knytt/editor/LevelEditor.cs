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
    private enum OptionsMenu { FadeLayers, FollowScript }
    private enum GoToMenu { Start, Default, Coordinates, WorldMap }
    private enum TestMenu { FromStart, FromHere, Profiles, UseAsStart, Profile = 100 }
    private enum Placing { None, Start, Test, Profile }
    private enum CellMenu { Pick, Test, Start }

    private static readonly HashSet<string> overwrite_allowed = new HashSet<string>();
    private static LevelEditor tested;
    private static readonly Color SAVED_COLOR = Color.Color8(87, 135, 231);
    private static readonly Color CHANGED_COLOR = Color.Color8(247, 195, 79);
    private static readonly Color INACTIVE_COLOR = new Color(1, 1, 1, 0.3f);
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
        ini_edit.AddColorRegion("[", "]", Color.Color8(222, 140, 210));
        ini_edit.AddColorRegion(";", "", new Color(0.6f, 0.6f, 0.6f), lineOnly: true);
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
        options_menu.AddCheckItem("Fade other layers (hold Tab to see all)", (int)OptionsMenu.FadeLayers);
        options_menu.AddCheckItem("Follow the script cursor (screen and form)", (int)OptionsMenu.FollowScript);
        options_menu.SetItemChecked(options_menu.GetItemIndex((int)OptionsMenu.FadeLayers), screen_view.FadeOthers);
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
    }

    private string worldFile(string name)
    {
        var data = world.getWorldData(name);
        return data == null ? "" : GDKnyttAssetManager.loadTextFile(data);
    }

    private int SelectedTileValue => selected_tile == 0 ? 0 : selected_tile + (GetNode<BaseButton>("%TilesetBButton").Pressed ? 128 : 0);

    private void selectTile(int index)
    {
        selected_tile = index;
        GetNode<Control>("%TileCursor").RectPosition = new Vector2(
            index % GDKnyttAssetManager.TILESET_WIDTH * GDKnyttAssetManager.TILE_WIDTH,
            index / GDKnyttAssetManager.TILESET_WIDTH * GDKnyttAssetManager.TILE_HEIGHT);
        showTileset();
    }

    private LayerButton layerButton(int layer) => GetNode<LayerButton>($"%Layer{layer}");

    private bool TileLayer => layer >= 0 && layer < KnyttArea.AREA_TILE_LAYERS;
    private bool ObjectLayer => layer >= KnyttArea.AREA_TILE_LAYERS;

    // Choosing the current layer again shows all layers and turns painting off
    private void selectLayer(int layer)
    {
        this.layer = layer == this.layer ? -1 : layer;
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
        if (placing != Placing.None) { screen_view.showJuni(); }
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

    private void paint(int x, int y)
    {
        if (layer < 0) { return; }
        var area = world.getArea(coords);
        if (area == null || area.Empty)
        {
            area = KnyttArea.blank(world, coords);
            foreach (var value in ScreenValues.ALL) { ScreenValues.set(area, value, this.value(value)); }
            history.addArea(world, area);
            mini_map.showScreen(world, coords);
        }
        if (!screen_view.LayerShown[layer]) { showLayer(layer, true); }

        int index = y * KnyttArea.AREA_WIDTH + x;
        if (TileLayer) { history.setTile(area, layer, index, (byte)SelectedTileValue); }
        else { history.setObject(area, layer - KnyttArea.AREA_TILE_LAYERS, index, SelectedObject); }
        screen_view.Update();
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
        var focus = GetFocusOwner();
        if (@event is InputEventMouseButton button && button.Pressed && focus != null && script.IsAParentOf(focus) &&
            !focus.GetGlobalRect().HasPoint(button.Position))
        {
            focus.ReleaseFocus();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (goto_dialog.Visible || overwrite_dialog.Visible || save_as_dialog.Visible || unsaved_dialog.Visible || palette.Visible) { return; }

        if (@event is InputEventKey tab && tab.Scancode == (uint)KeyList.Tab && !tab.Echo)
        {
            GetTree().SetInputAsHandled();
            screen_view.Peeking = tab.Pressed;
            screen_view.Update();
            return;
        }

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
        if (ObjectLayer && selected_object != 0) { palette.addRecent(SelectedObject); }
    }

    private void _on_ScreenView_CellPainted(int x, int y)
    {
        paint(x, y);
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

    private void _on_Tileset_gui_input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton button) || !button.Pressed || button.ButtonIndex != (int)ButtonList.Left) { return; }
        var cell = (button.Position / new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT)).Floor();
        if (cell.y >= GDKnyttAssetManager.TILESET_HEIGHT) { return; }
        selectTile((int)cell.y * GDKnyttAssetManager.TILESET_WIDTH + (int)cell.x);
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

    private void _on_TilesetButton_toggled(bool pressed)
    {
        showTileset();
    }
}
