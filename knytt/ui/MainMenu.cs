using System.Threading.Tasks;
using Godot;
using YKnyttLib;

public class MainMenu : Node2D
{
    PackedScene level_select_scene;
    AnimationPlayer fade_anim;

    public override void _Ready()
    {
        this.level_select_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/LevelSelection.tscn");
        fade_anim = GetNode<AnimationPlayer>("FadeLayer/FadeMask/AnimationPlayer");
    }

    public async void _on_TutorialButton_pressed()
    {
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
        world.CurrentSave = new KnyttSave(world, save_txt, 0);
        world.setBinMode(binloader);
        GDKnyttDataStore.KWorld = world;
    }

    public void _on_PlayButton_pressed()
    {
        GetNode<AudioStreamPlayer>("MenuClickPlayer").Play();
        var level_node = this.level_select_scene.Instance() as LevelSelection;
        this.AddChild(level_node);
    }

    public async void _on_QuitButton_pressed()
    {
        var player = GetNode<AudioStreamPlayer>("MenuClickPlayer");
        player.Play();
        fade_anim.Play("FadeOut");
        await ToSignal(fade_anim, "animation_finished");
        GetTree().Quit();
    }
}
