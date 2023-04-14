using Godot;
using System.Threading.Tasks;
using YKnyttLib;

public partial class MainMenu : Node2D
{
    PackedScene level_select_scene;
    PackedScene settings_scene;
    FadeLayer fade;

    bool quitting = false;

    public override void _Ready()
    {
        this.level_select_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/LevelSelection.tscn");
        this.settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        fade = GetNode<FadeLayer>("MenuLayer/Fade");
        GetNode<HBoxContainer>("MenuLayer/ButtonRow").GrabFocus();
        RenderingServer.SetDefaultClearColor(new Color(0, 0, 0));
        GDKnyttSettings.setupViewport(for_ui: true);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest) { quit(); }
        if (what == NotificationWMGoBackRequest) { quit(); }
    }

    public async void _on_TutorialButton_pressed()
    {
        ClickPlayer.Play();
        var task = Task.Run(() => loadTutorial());
        fade.startFade();
        await ToSignal(fade, "FadeDone");
        task.Wait();
        GetTree().ChangeSceneToFile("res://knytt/GDKnyttGame.tscn");
    }

    private const string TUTORIAL_PATH = "res://knytt/worlds/Nifflas - Tutorial.knytt.bin";

    public void loadTutorial()
    {
        var binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(TUTORIAL_PATH));
        var txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("World.ini"));
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.setDirectory(TUTORIAL_PATH, binloader.RootDirectory);
        world.loadWorldConfig(txt);
        var save_txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("DefaultSavegame.ini"));
        world.CurrentSave = new KnyttSave(world, save_txt, 1);
        world.setBinMode(binloader);
        GDKnyttDataStore.KWorld = world;
    }

    public void _on_PlayButton_pressed(bool local_load)
    {
        ClickPlayer.Play();
        var level_node = this.level_select_scene.Instantiate<LevelSelection>();
        level_node.localLoad = local_load;
        this.AddChild(level_node);
    }

    public void _on_SettingsButton_pressed()
    {
        ClickPlayer.Play();
        var settings_node = this.settings_scene.Instantiate<SettingsScreen>();
        this.AddChild(settings_node);
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

    public void _on_ButtonRow_focus_entered()
    {

    }

    public void _on_ButtonRow_focus_exited()
    {

    }
}
