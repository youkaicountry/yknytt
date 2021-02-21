using Godot;

public class GDKnyttAudioChannel : AudioStreamPlayer
{
    public delegate AudioStream FetchEvent(int num);
    public delegate void CloseEvent(int num);

    public FetchEvent OnFetch { get; set; }
    public CloseEvent OnClose { get; set; }

    public int TrackNumber { get; private set; }

    [Export]
    public float fadeInTime = 2f;

    [Export]
    public float fadeOutTime = 5f;

    // Queued track
    bool track_queued = false;
    private int q_track;
    private AudioStream q_stream;

    private AnimationPlayer player;
    private bool no_fade_in;

    public override void _Ready()
    {
        player = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void setTrack(int num, bool no_fade_in = false)
    {
        this.no_fade_in = no_fade_in;
        
        // If it's the same track already playing nothing happens
        if (num == this.TrackNumber)
        {
            this.clearQ();
            if (no_fade_in) { return; }
            // If the song is fading out, reverse the fading
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(this.Bus), 0f);
            if (player.IsPlaying() && player.CurrentAnimation.Equals("FadeOut")) { player.PlaybackSpeed = -(1f / this.fadeOutTime); }
            return;
        }

        // Track is different
        var next_stream = this.OnFetch?.Invoke(num);

        this.changeTrack(num, next_stream);
    }

    private void changeTrack(int track, AudioStream stream)
    {
        // Queue gets changed no matter what
        this.setQ(track, stream);

        //GD.Print(player.IsPlaying(), " ", this.Playing);
        // If already fading out, simply change the queue
        if (player.IsPlaying() && player.CurrentAnimation.Equals("FadeOut"))
        {
            // Ensure that it's fading out, not back in
            player.PlaybackSpeed = 1f / this.fadeOutTime;
            return;
        }
        // Else if this is actively playing a track, start a fade out
        else if (this.Playing)
        {
            player.PlaybackSpeed = 1f / this.fadeOutTime;
            // Set bus volume to current, as a hack for sounds fading in that suddenly pop to full volume
            // TODO: Maybe find a better way to do this?
            //AudioServer.SetBusVolumeDb()
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(this.Bus), this.VolumeDb);
            player.Play("FadeOut");
            return;
        }

        // else we just immediately start the track
        this.playQueued();
    }

    private void clearQ()
    {
        this.track_queued = false;
    }

    private void setQ(int track, AudioStream stream)
    {
        this.track_queued = true;
        this.q_track = track;
        this.q_stream = stream;
    }

    private void playQueued()
    {
        if (!track_queued) { return; } // If not track queued, get out of here
        this.track_queued = false;
        this.resetAnimation();
        this.Stop();

        this.Stream = null;
        if (TrackNumber != 0) { OnClose?.Invoke(TrackNumber); }
        this.TrackNumber = this.q_track;

        // If null track, keep the player stopped
        if (this.q_stream == null) { return; }

        this.Stream = this.q_stream;
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(this.Bus), 0f);
        this.Play();

        if (!no_fade_in) { fadeIn(this.fadeInTime); }
    }

    private void resetAnimation()
    {
        player.Stop();
        this.VolumeDb = -80f;
    }

    public void fadeIn(float sec)
    {
        player.PlaybackSpeed = 1f / sec;
        player.Play("FadeIn");
    }

    public void _on_finished()
    {
        this.playQueued();
    }
}
