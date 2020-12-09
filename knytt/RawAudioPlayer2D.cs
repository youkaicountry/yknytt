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

    // TODO: Workaround for late played sounds. Maybe implement cache and get rid of this and DisappearPlayer?
    public bool IsDisposed { get; set; }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    public void Play()
    {
        base.Play();
    }
}
