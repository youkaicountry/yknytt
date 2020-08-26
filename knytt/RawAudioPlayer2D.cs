using Godot;

public class RawAudioPlayer2D : AudioStreamPlayer2D
{
    [Export] string rawPath;
    [Export] int sampleRate = 11025;
    [Export] bool loop = false;

    public override void _Ready()
    {
        this.Stream = GDKnyttAssetManager.loadRaw(rawPath, sampleRate);
        if (this.Autoplay) { this.Play(); }
    }

    public void _on_RawAudioPlayer2D_finished()
    {
        if (loop) { this.Play(); }
    }
}