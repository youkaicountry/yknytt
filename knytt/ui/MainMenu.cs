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
    }

    public override void initFocus()
    {
        GetNode<HBoxContainer>("ButtonRow").GrabFocus();
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
        if (OS.GetName() == "HTML5") { loadTutorial(WEB_TUTORIAL_PATH); }
        else if (TouchSettings.EnablePanel) { task = Task.Run(() => loadTutorial(TOUCH_TUTORIAL_PATH)); }
        else { task = Task.Run(() => loadTutorial(TUTORIAL_PATH)); }
        fade.startFade();
        await ToSignal(fade, "FadeDone");
        task?.Wait();
        GetTree().ChangeScene("res://knytt/GDKnyttGame.tscn");
    }

    public void loadTutorial(string path)
    {
        var binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(path));
        var txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("World.ini"));
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.setDirectory(path, binloader.RootDirectory);
        world.loadWorldConfig(txt);
        var save_txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("DefaultSavegame.ini"));
        world.CurrentSave = new KnyttSave(world, save_txt, 1);
        world.setBinMode(binloader);
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
        fade.startFade(reset:false);
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
