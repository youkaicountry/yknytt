using Godot;

public partial class ClickPlayer : Node
{
    public static AudioStreamPlayer AudioPlayer { get; private set; }

    public override void _Ready()
    {
        AudioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");
    }

    public static void Play()
    {
        AudioPlayer.Play();
    }
}
