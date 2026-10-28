using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YKnyttLib;

public class LevelEditor : Control
{
    private static readonly Vector2 SCREEN_SIZE = new Vector2(700, 610);
    private static readonly KnyttPoint DEFAULT_SCREEN = new KnyttPoint(1000, 1000);

    private enum FileMenu { Save, SaveAs, Quit }
    private enum EditMenu { Undo, Redo }
    private enum OptionsMenu { FadeLayers }
    private enum GoToMenu { Start, Default, Coordinates }

    private static readonly HashSet<string> overwrite_allowed = new HashSet<string>();
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

    private KnyttPoint coords;
    private int background;
    private int tileset_a;
    private int tileset_b;
    private int music;
    private int ambiance_a;
    private int ambiance_b;

    private int layer = -1;
    private int selected_tile;
    private int selected_bank;
    private int selected_object;
    private ulong press_time;
    private bool quit_game;
    private KnyttPoint? hovered;
    private string status_message = "";

    private ScreenView screen_view;
    private MiniMap mini_map;
    private ObjectPalette palette;
    private ConfirmationDialog goto_dialog;
    private ConfirmationDialog overwrite_dialog;
    private ConfirmationDialog save_as_dialog;
    private ConfirmationDialog unsaved_dialog;

    public static bool Available => OS.GetName() != "Unix";

    public static void open(SceneTree tree, GDKnyttWorldImpl world, bool created = false)
    {
        if (created) { overwrite_allowed.Add(world.WorldDirectory); }
        GDKnyttDataStore.KWorld = world;
        tree.ChangeScene("res://knytt/editor/LevelEditor.tscn");
    }

    public override void _Ready()
    {
        GetTree().SetScreenStretch(
            GDKnyttSettings.SmoothScaling ? SceneTree.StretchMode.Mode2d : SceneTree.StretchMode.Viewport,
            SceneTree.StretchAspect.Keep, SCREEN_SIZE);
        game_window_size = OS.WindowSize;
        fitWindow();

        screen_view = GetNode<ScreenView>("%ScreenView");
        mini_map = GetNode<MiniMap>("%MiniMap");
        palette = GetNode<ObjectPalette>("%ObjectPalette");
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
        edit_menu.Connect("id_pressed", this, nameof(_on_EditMenu_id_pressed));

        var options_menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        options_menu.AddCheckItem("Fade other layers (hold Tab to see all)", (int)OptionsMenu.FadeLayers);
        options_menu.SetItemChecked(options_menu.GetItemIndex((int)OptionsMenu.FadeLayers), screen_view.FadeOthers);
        options_menu.Connect("id_pressed", this, nameof(_on_OptionsMenu_id_pressed));

        var goto_menu = GetNode<MenuButton>("%GoToMenu").GetPopup();
        goto_menu.AddItem("Start position", (int)GoToMenu.Start);
        goto_menu.AddItem($"x{DEFAULT_SCREEN.x}y{DEFAULT_SCREEN.y}", (int)GoToMenu.Default);
        goto_menu.AddSeparator();
        goto_menu.AddItem("Coordinates", (int)GoToMenu.Coordinates, (uint)KeyModifierMask.MaskCtrl | (uint)KeyList.G);
        goto_menu.Connect("id_pressed", this, nameof(_on_GoToMenu_id_pressed));

        loadWorld(GDKnyttDataStore.KWorld);
    }

    private void loadWorld(GDKnyttWorldImpl world)
    {
        this.world = world;
        if (world.Map == null) { world.loadWorldMap(new MemoryStream(world.getWorldData("Map.bin"))); }
        assets = new EditorAssets(world);
        catalog = new ObjectCatalog(world);
        palette.init(assets, catalog);

        var save_data = world.getWorldData("DefaultSavegame.ini");
        var save = new KnyttSave(world, save_data != null ? GDKnyttAssetManager.loadTextFile(save_data) : "", 1);
        start_area = save_data != null ? save.getArea() : DEFAULT_SCREEN;

        screen_view.init(world, assets, start_area, save.getAreaPosition());
        selectObject(0, 0);
        selectLayer(0);
        GDKnyttDataStore.setTitle(world.WorldDirectoryName);
        goTo(start_area);
    }

    private void goTo(KnyttPoint coords)
    {
        this.coords = coords;

        var area = world.getArea(coords);
        if (area != null && !area.Empty)
        {
            background = area.Background;
            tileset_a = area.TilesetB;
            tileset_b = area.TilesetA;
            music = area.Song;
            ambiance_a = area.AtmosphereA;
            ambiance_b = area.AtmosphereB;
        }

        screen_view.showScreen(coords, background);
        mini_map.showScreen(world, coords);

        GetNode<Label>("%MusicValue").Text = $"{music}";
        GetNode<Label>("%AmbianceAValue").Text = $"{ambiance_a}";
        GetNode<Label>("%AmbianceBValue").Text = $"{ambiance_b}";
        GetNode<Label>("%BackgroundLabel").Text = $"Background {background}";
        GetNode<Button>("%TilesetAButton").Text = $"A Tileset {tileset_a}";
        GetNode<Button>("%TilesetBButton").Text = $"B Tileset {tileset_b}";
        showStatus();
        showTileset();
        showChanged();
    }

    private void showChanged()
    {
        GetNode<ColorRect>("%ChangeLineTop").Color = GetNode<ColorRect>("%ChangeLineBottom").Color =
            history.Modified ? CHANGED_COLOR : SAVED_COLOR;
    }

    private int SelectedTileValue => selected_tile == 0 ? 0 : selected_tile + (GetNode<Button>("%TilesetBButton").Pressed ? 128 : 0);

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
        GetNode<Button>("%TilesetAButton").Disabled = GetNode<Button>("%TilesetBButton").Disabled = !TileLayer;
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
        bank_value.Text = $"Bank {selected_bank}";
        bank_value.HintTooltip = $"{bank_name}\nClick: next. Right-click: back. Wheel works too\nShift: 10 at a time. Keys: Z, X";
        var object_value = GetNode<Label>("%ObjectValue");
        object_value.Text = $"Object {selected_object}";
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
        if (TileLayer && selected_tile != 0)
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
            area.Background = background;
            area.TilesetB = tileset_a;
            area.TilesetA = tileset_b;
            area.Song = music;
            area.AtmosphereA = ambiance_a;
            area.AtmosphereB = ambiance_b;
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
        GetNode<Button>(tile < 128 ? "%TilesetAButton" : "%TilesetBButton").Pressed = true;
        selectTile(tile % 128);
    }

    private void refresh()
    {
        screen_view.Update();
        mini_map.showScreen(world, coords);
        showChanged();
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
        bool show_b = GetNode<Button>("%TilesetBButton").Pressed;
        GetNode<TextureRect>("%Tileset").Texture = assets.getTileset(show_b ? tileset_b : tileset_a);
        showBrush();
    }

    private bool Resizable => OS.HasFeature("pc") && !OS.WindowFullscreen && !OS.WindowMaximized;

    private void fitWindow()
    {
        if (!Resizable) { return; }
        var screen = OS.GetScreenSize() * 0.9f;
        OS.WindowSize = SCREEN_SIZE * Mathf.Max(1, Mathf.Floor(Mathf.Min(screen.x / SCREEN_SIZE.x, screen.y / SCREEN_SIZE.y)));
        OS.CenterWindow();
    }

    private void save()
    {
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
    }

    private void showSaveResult(Error result)
    {
        if (result == Error.Ok) { history.markSaved(); }
        showChanged();
        showStatus(result == Error.Ok ? $"Saved {world.WorldDirectory.GetFile()}" : $"Can't save ({result})");
    }

    private void quit(bool game = false)
    {
        if (history.Modified)
        {
            quit_game = game;
            unsaved_dialog.PopupCentered();
            return;
        }
        if (game) { GetTree().Quit(); return; }
        if (Resizable)
        {
            OS.WindowSize = game_window_size;
            OS.CenterWindow();
        }
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest) { quit(game: true); }
        if (what == MainLoop.NotificationWmGoBackRequest) { quit(); }
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

        if (@event.IsActionPressed("ui_back") || @event.IsActionPressed("main_menu"))
        {
            GetTree().SetInputAsHandled();
            quit();
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
        bool changed = (EditMenu)id == EditMenu.Undo ? history.undo() : history.redo();
        if (changed) { refresh(); }
    }

    private void _on_OptionsMenu_id_pressed(int id)
    {
        var menu = GetNode<MenuButton>("%OptionsMenu").GetPopup();
        int index = menu.GetItemIndex(id);
        menu.SetItemChecked(index, !menu.IsItemChecked(index));
        if ((OptionsMenu)id == OptionsMenu.FadeLayers) { screen_view.FadeOthers = menu.IsItemChecked(index); }
        screen_view.Update();
    }

    private void _on_GoToMenu_id_pressed(int id)
    {
        switch ((GoToMenu)id)
        {
            case GoToMenu.Start: goTo(start_area); break;
            case GoToMenu.Default: goTo(DEFAULT_SCREEN); break;
            case GoToMenu.Coordinates: showGoToDialog(); break;
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

    private void _on_ScreenView_CellPicked(int x, int y)
    {
        pick(x, y);
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

    private void _on_MiniMap_ScreenSelected(int x, int y)
    {
        goTo(new KnyttPoint(x, y));
    }

    private void _on_TilesetButton_toggled(bool pressed)
    {
        showTileset();
    }
}
