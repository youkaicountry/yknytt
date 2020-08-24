using Godot;

public class MenuClickPlayer : AudioStreamPlayer
{
    public override void _Ready()
    {
        this.Stream = GDKnyttAssetManager.loadRaw("res://knytt/data/Sfx/Menu Click.raw", 44100);
    }

}
