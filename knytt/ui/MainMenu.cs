using Godot;

public class MainMenu : Node2D
{
    PackedScene level_select_scene;
    public override void _Ready()
    {
        this.level_select_scene = ResourceLoader.Load("res://knytt/ui/LevelSelection.tscn") as PackedScene;
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
        await ToSignal(player, "finished");
        GetTree().Quit();
    }
}
