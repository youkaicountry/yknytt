using Godot;
using System.IO;
using System.Text.RegularExpressions;
using YKnyttLib;

public class LevelEditor : Control
{
    private static readonly Vector2 SCREEN_SIZE = new Vector2(700, 610);
    private static readonly KnyttPoint DEFAULT_SCREEN = new KnyttPoint(1000, 1000);

    private enum FileMenu { Quit }
    private enum GoToMenu { Start, Default, Coordinates }

    private Vector2 game_window_size;

    private KnyttWorld world;
    private EditorAssets assets;
    private KnyttPoint start_area;

    private KnyttPoint coords;
    private int background;
    private int tileset_a;
    private int tileset_b;
    private int music;
    private int ambiance_a;
    private int ambiance_b;

    private ScreenView screen_view;
    private MiniMap mini_map;
    private ConfirmationDialog goto_dialog;

    public static bool Available => OS.GetName() != "Unix";

    public static void open(SceneTree tree, GDKnyttWorldImpl world)
    {
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
        goto_dialog = GetNode<ConfirmationDialog>("%GoToDialog");

        var file_menu = GetNode<MenuButton>("%FileMenu").GetPopup();
        file_menu.AddItem("Quit", (int)FileMenu.Quit);
        file_menu.Connect("id_pressed", this, nameof(_on_FileMenu_id_pressed));

        var goto_menu = GetNode<MenuButton>("%GoToMenu").GetPopup();
        goto_menu.AddItem("Start position", (int)GoToMenu.Start);
        goto_menu.AddItem($"x{DEFAULT_SCREEN.x}y{DEFAULT_SCREEN.y}", (int)GoToMenu.Default);
        goto_menu.AddSeparator();
        goto_menu.AddItem("Coordinates", (int)GoToMenu.Coordinates);
        goto_menu.Connect("id_pressed", this, nameof(_on_GoToMenu_id_pressed));

        loadWorld(GDKnyttDataStore.KWorld);
    }

    private void loadWorld(KnyttWorld world)
    {
        this.world = world;
        world.loadWorldMap(new MemoryStream(world.getWorldData("Map.bin")));
        assets = new EditorAssets(world);

        var save_data = world.getWorldData("DefaultSavegame.ini");
        var save = new KnyttSave(world, save_data != null ? GDKnyttAssetManager.loadTextFile(save_data) : "", 1);
        start_area = save_data != null ? save.getArea() : DEFAULT_SCREEN;

        screen_view.init(world, assets, start_area, save.getAreaPosition());
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
        GetNode<Label>("%StatusLabel").Text = $"Map: x{coords.x}y{coords.y}";
        showTileset();
    }

    private void showTileset()
    {
        bool show_b = GetNode<Button>("%TilesetBButton").Pressed;
        GetNode<TextureRect>("%Tileset").Texture = assets.getTileset(show_b ? tileset_b : tileset_a);
    }

    private bool Resizable => OS.HasFeature("pc") && !OS.WindowFullscreen && !OS.WindowMaximized;

    private void fitWindow()
    {
        if (!Resizable) { return; }
        var screen = OS.GetScreenSize() * 0.9f;
        OS.WindowSize = SCREEN_SIZE * Mathf.Max(1, Mathf.Floor(Mathf.Min(screen.x / SCREEN_SIZE.x, screen.y / SCREEN_SIZE.y)));
        OS.CenterWindow();
    }

    private void quit()
    {
        if (Resizable)
        {
            OS.WindowSize = game_window_size;
            OS.CenterWindow();
        }
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest) { GetTree().Quit(); }
        if (what == MainLoop.NotificationWmGoBackRequest) { quit(); }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (goto_dialog.Visible) { return; }

        if (@event.IsActionPressed("ui_back") || @event.IsActionPressed("main_menu"))
        {
            GetTree().SetInputAsHandled();
            quit();
        }
        else if (@event is InputEventKey key && key.Pressed && key.Control && key.Scancode == (uint)KeyList.G)
        {
            GetTree().SetInputAsHandled();
            showGoToDialog();
        }
        else if (direction(@event) is KnyttPoint step)
        {
            GetTree().SetInputAsHandled();
            goTo(coords + step);
        }
    }

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
        if (id == (int)FileMenu.Quit) { quit(); }
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

    private void _on_MiniMap_ScreenSelected(int x, int y)
    {
        goTo(new KnyttPoint(x, y));
    }

    private void _on_TilesetButton_toggled(bool pressed)
    {
        showTileset();
    }
}
