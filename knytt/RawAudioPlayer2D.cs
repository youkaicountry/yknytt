using Godot;
using System.Collections.Generic;

public class RawAudioPlayer2D : AudioStreamPlayer2D
{
    [Export] string rawPath;
    [Export] int sampleRate = 11025;
    [Export] bool loop = false;
    [Export] float fromPosition = 0;

    static Dictionary<string, AudioStream> streamCache = new Dictionary<string, AudioStream>();

    public override void _Ready()
    {
        if (!streamCache.ContainsKey(rawPath))
        {
            streamCache.Add(rawPath, GDKnyttAssetManager.loadRaw(rawPath, sampleRate));
        }
        this.Stream = streamCache[rawPath];

        if (this.Autoplay) { this.Play(); }
    }

    public void _on_RawAudioPlayer2D_finished()
    {
        if (loop) { this.Play(); }
    }

    public void Play()
    {
        base.Play(fromPosition);
    }
}
