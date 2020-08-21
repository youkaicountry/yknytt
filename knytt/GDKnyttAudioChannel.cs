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

    public void setTrack(int num)
    {
        // If it's the same track already playing nothing happens
        if (num == this.TrackNumber)
        {
            this.clearQ();
            // If the song is fading out, reverse the fading
            var player = (AnimationPlayer)GetNode("AnimationPlayer");
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(this.Bus), 0f);
            if (player.IsPlaying() && player.CurrentAnimation.Equals("FadeOut")) { player.PlaybackSpeed = -(1f / this.fadeOutTime); }
            return; 
        }

        // Track is different
        var next_stream = this.OnFetch.Invoke(num);

        this.changeTrack(num, next_stream);
    }

    private void changeTrack(int track, AudioStream stream)
    {
        // Queue gets changed no matter what
        this.setQ(track, stream);

        var player = (AnimationPlayer)GetNode("AnimationPlayer");
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
        this.VolumeDb = -80f;
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(this.Bus), 0f);
        this.Play();

        var player = (AnimationPlayer)GetNode("AnimationPlayer");
        player.PlaybackSpeed = 1f / this.fadeInTime;
        player.Play("FadeIn");
    }

    private void resetAnimation()
    {
        var player = (AnimationPlayer)GetNode("AnimationPlayer");
        player.Stop();
        this.VolumeDb = 0.0f;
    }

    public void _on_finished()
    {
        this.playQueued();
    }
}
