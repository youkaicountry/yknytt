using Godot;

public class ClickPlayer : Node
{
    public static AudioStreamPlayer AudioPlayer { get; private set; }

    public override void _Ready()
    {
        AudioPlayer = GetNode<AudioStreamPlayer>("RawAudioPlayer");
    }

    public static void Play()
    {
        AudioPlayer.Play();
    }
}
