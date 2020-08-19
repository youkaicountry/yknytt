using Godot;

public class KnyttAudioChannel : AudioStreamPlayer
{
    public delegate AudioStream FetchStream(int num);

    public FetchStream StreamFetcher { get; set; }
    public int TrackNumber { get; private set; }

    [Export]
    public float fadeTime = 5f;

    // Queued track
    private int q_track;
    private AudioStream q_stream;

    public void setTrack(int num)
    {
        // If it's the same track already playing nothing happens
        if (num == this.TrackNumber) 
        {
            // If the song is fading out, reverse the fading
            //resetAnimation();
            ((AnimationPlayer)GetNode("AnimationPlayer")).PlaybackSpeed = -(1f / this.fadeTime);
            return; 
        }

        // Track is different
        var next_stream = this.StreamFetcher?.Invoke(num);

        this.changeTrack(num, next_stream);
    }

    private void changeTrack(int track, AudioStream stream)
    {
        // Queue gets changed no matter what
        this.q_track = track;
        this.q_stream = stream;

        var player = (AnimationPlayer)GetNode("AnimationPlayer");
        // If already fading, simply change the queue
        if (player.IsPlaying()) { return; }
        // Else if this is actively playing a track, start a fade
        else if (this.Playing)
        {
            player.PlaybackSpeed = 1f / this.fadeTime;
            player.Play("FadeOut");
            return;
        }

        // else we just immediately start the track
        this.playQueued();
    }

    private void playQueued()
    {
        this.resetAnimation();
        this.Stop();
        this.TrackNumber = this.q_track;

        // If null track, keep the player stopped
        if (this.q_stream == null) { return; }

        this.Stream = this.q_stream;
        this.Play();
    }

    private void resetAnimation()
    {
        var player = (AnimationPlayer)GetNode("AnimationPlayer");
        player.Stop();
        this.VolumeDb = 0.0f;
    }
}
