using Godot;

public class GDKnyttMusicChannel : AudioStreamPlayer
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
    private AudioStream q_intro;
    private AudioStream q_stream;
    private AudioStream cur_intro;
    private AudioStream cur_stream;

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
        this.changeTrack(num, this.OnFetch?.Invoke(num), this.OnFetch?.Invoke(-num));
    }

    private void changeTrack(int track, AudioStream stream, AudioStream intro_stream)
    {
        // Queue gets changed no matter what
        this.setQ(track, stream, intro_stream);

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
        if (track_queued && q_track != 0 && q_stream != null) { OnClose?.Invoke(q_track); }
        if (track_queued && q_track != 0 && q_intro != null) { OnClose?.Invoke(-q_track); }
        this.track_queued = false;
    }

    private void setQ(int track, AudioStream stream, AudioStream intro_stream)
    {
        this.track_queued = true;
        this.q_track = track;
        this.q_stream = stream;
        this.q_intro = intro_stream;
    }

    public void playQueued()
    {
        bool intro_playing = this.Stream == cur_intro;
        if (!track_queued && !intro_playing) { return; } // If not track queued, get out of here

        this.resetAnimation();
        this.Stop();

        if (track_queued)
        {
            if (TrackNumber != 0) { OnClose?.Invoke(TrackNumber); }
            cur_intro = q_intro;
            cur_stream = q_stream;
            TrackNumber = q_track;
            this.Stream = cur_intro != null ? cur_intro : cur_stream;
        }
        else if (intro_playing)
        {
            OnClose?.Invoke(-TrackNumber);
            this.Stream = cur_stream;
        }
        
        track_queued = false;

        // If null track, keep the player stopped
        if (this.Stream == null) { return; }

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
