using System.Threading.Tasks;
using Godot;
using YKnyttLib;

public class MainMenu : Node2D
{
    PackedScene level_select_scene;
    PackedScene settings_scene;
    AnimationPlayer fade_anim;

    public override void _Ready()
    {
        this.level_select_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/LevelSelection.tscn");
        this.settings_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/SettingsScreen.tscn");
        fade_anim = GetNode<AnimationPlayer>("FadeLayer/FadeMask/AnimationPlayer");
        //GetNode<Button>("MenuLayer/ButtonRow/PlayButton").GrabFocus();
        GetNode<HBoxContainer>("MenuLayer/ButtonRow").GrabFocus();
    }

    public async void _on_TutorialButton_pressed()
    {
        GetNode<AudioStreamPlayer>("MenuClickPlayer").Play();
        var task = Task.Run(() => loadTutorial());
        fade_anim.Play("FadeOut");
        await ToSignal(fade_anim, "animation_finished");
        task.Wait();
        GetTree().ChangeScene("res://knytt/GDKnyttGame.tscn");
    }

    public void loadTutorial()
    {
        var binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile("res://knytt/worlds/Nifflas - Tutorial.knytt.bin"));
        var txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("World.ini"));
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.setDirectory("res://knytt/worlds", binloader.RootDirectory);
        world.loadWorldConfig(txt);
        var save_txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("DefaultSavegame.ini"));
        world.CurrentSave = new KnyttSave(world, save_txt, 1);
        world.setBinMode(binloader);
        GDKnyttDataStore.KWorld = world;
    }

    public void _on_PlayButton_pressed()
    {
        GetNode<AudioStreamPlayer>("MenuClickPlayer").Play();
        var level_node = this.level_select_scene.Instance() as LevelSelection;
        this.AddChild(level_node);
    }

    public void _on_SettingsButton_pressed()
    {
        GetNode<AudioStreamPlayer>("MenuClickPlayer").Play();
        var settings_node = this.settings_scene.Instance() as SettingsScreen;
        this.AddChild(settings_node);
    }

    public async void _on_QuitButton_pressed()
    {
        var player = GetNode<AudioStreamPlayer>("MenuClickPlayer");
        player.Play();
        fade_anim.Play("FadeOut");
        await ToSignal(fade_anim, "animation_finished");
        GetTree().Quit();
    }

    public void _on_ButtonRow_focus_entered()
    {

    }

    public void _on_ButtonRow_focus_exited()
    {
        
    }
}
