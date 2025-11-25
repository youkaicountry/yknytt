using Godot;
using System.Threading.Tasks;
using YKnyttLib;

public class MainMenu : BasicScreen
{
    PackedScene level_select_scene;
    PackedScene settings_scene;
    PackedScene credits_scene;
    FadeLayer fade;

    bool quitting = false;

    public override void _Ready()
    {
        base._Ready();
        this.level_select_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/LevelSelection.tscn");
        this.settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        this.credits_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/CreditsScreen.tscn");
        fade = GetNode<FadeLayer>("Fade");
        initFocus();
        VisualServer.SetDefaultClearColor(new Color(0, 0, 0));
        GDKnyttSettings.setupViewport(for_ui: true);
        if (OS.GetName() == "HTML5") { GetNode<Button>("ButtonRow/QuitButton").Visible = false; }
        GetNode<Button>("ButtonRow/TutorialButton").Text =
            new File().FileExists(GDKnyttDataStore.BaseDataDirectory.PlusFile("lastplayed.ini")) ? "Continue" : "Tutorial";
        GDKnyttDataStore.setTitle(null);
    }

    public override void initFocus()
    {
        GetNode<Control>(OS.GetName() != "Unix" ? "ButtonRow" : "ButtonRow/PlayButton").GrabFocus();
        GetNode<Button>("ButtonRow/DownloadButton").Disabled = GDKnyttSettings.Connection == GDKnyttSettings.ConnectionType.Offline;
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest) { quit(); }
        if (what == MainLoop.NotificationWmGoBackRequest)
        {
            if (GetNode<Console>("/root/Console").IsOpen) { GetNode<Console>("/root/Console").toggleConsole(); return; }
            if (GetNode<HBoxContainer>("ButtonRow").GetFocusOwner() is LineEdit) { ActiveScreen.initFocus(); return; }
            ActiveScreen.goBack();
        }
    }

    public const string TUTORIAL_PATH = "res://knytt/worlds/Nifflas - Tutorial.knytt.bin";
    public const string TOUCH_TUTORIAL_PATH = "res://knytt/worlds/Nifflas - Touch Tutorial.knytt.bin";
    public const string WEB_TUTORIAL_PATH = "res://knytt/worlds/Nifflas - Original Tutorial.knytt.bin";

    public async void _on_TutorialButton_pressed()
    {
        ClickPlayer.Play();
        Task task = null;
        bool load_last_played = false;
        if (new File().FileExists(GDKnyttDataStore.BaseDataDirectory.PlusFile("lastplayed.ini")))
        {
            var f = new File();
            f.Open(GDKnyttDataStore.BaseDataDirectory.PlusFile("lastplayed.ini"), File.ModeFlags.Read);
            string path = f.GetAsText();
            f.Close();
            string file = path.Substring(0, path.LastIndexOf('/'));
            int slot = int.Parse(path.Substring(file.Length + 1));

            if (new File().FileExists(file) || new Directory().DirExists(file))
            {
                load_last_played = true;
                if (OS.GetName() == "HTML5") { loadLevel(file, slot); }
                else { task = Task.Run(() => loadLevel(file, slot)); }                
            }
        }
        
        if (!load_last_played)
        {
            if (OS.GetName() == "HTML5") { loadLevel(WEB_TUTORIAL_PATH, 0); }
            else if (TouchSettings.EnablePanel) { task = Task.Run(() => loadLevel(TOUCH_TUTORIAL_PATH, 0)); }
            else { task = Task.Run(() => loadLevel(TUTORIAL_PATH, 0)); }
        }

        fade.startFade();
        await ToSignal(fade, "FadeDone");
        task?.Wait();
        GDKnyttDataStore.setTitle(GDKnyttDataStore.KWorld.WorldDirectoryName);
        GetTree().ChangeScene("res://knytt/GDKnyttGame.tscn");
    }

    public void loadLevel(string path, int slot)
    {
        bool bin = !new Directory().DirExists(path);
        var binloader = bin ? new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(path)) : null;
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        if (bin) { world.setBinMode(binloader); }
        world.setDirectory(path, bin ? binloader.RootDirectory : path.GetFile());
        string world_txt = GDKnyttAssetManager.loadTextFileRaw(world.getWorldData("World.ini"));
        world.loadWorldConfig(world_txt);
        var save_file = GDKnyttSettings.Saves.PlusFile($"{world.WorldDirectoryName} {slot}.ini");
        var save_txt = new File().FileExists(save_file) ? 
            GDKnyttAssetManager.loadTextFile(save_file) : 
            GDKnyttAssetManager.loadTextFile(world.getWorldData("DefaultSavegame.ini"));
        world.CurrentSave = new KnyttSave(world, save_txt, slot != 0 ? slot : 1);
        GDKnyttDataStore.KWorld = world;
    }

    public void _on_PlayButton_pressed(bool local_load)
    {
        var level_node = this.level_select_scene.Instance() as LevelSelection;
        level_node.localLoad = local_load;
        loadScreen(level_node);
    }

    public void _on_SettingsButton_pressed()
    {
        loadScreen(this.settings_scene.Instance() as SettingsScreen);
    }

    private void _on_CreditsButton_pressed()
    {
        loadScreen(this.credits_scene.Instance() as CreditsScreen);
    }

    public void _on_QuitButton_pressed()
    {
        ClickPlayer.Play();
        quit();
    }

    private async void quit()
    {
        if (quitting) { return; }
        quitting = true;
        fade.startFade(reset: false);
        // Sometimes quit fails leading to white screen:
        // ERROR: Mono: Domain finalization timeout. at: _unload_scripts_domain (modules/mono/mono_gd/gd_mono.cpp:1042)
        // To reproduce it, play Azure Serenity Expansion from start to x1009y1004 on Android or handheld Linux
        CustomObject.clean();
        System.GC.Collect();
        await ToSignal(fade, "FadeDone");
        GetTree().Quit();
    }

    private void _on_CloudControl_resized()
    {
        GetNode<MenuCloud>("CloudControl/Control2/MenuCloud").Scale = 
            Vector2.One * GetNode<Control>("CloudControl").RectSize.y / (240 + GetNode<Control>("CloudControl").MarginBottom);
    }

    public override void goBack()
    {
        quit();
    }
}
