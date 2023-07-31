using Godot;

public class GDKnyttAmbiChannel : Node
{
    public delegate AudioStream FetchEvent(int num);
    public delegate void CloseEvent(int num);

    public FetchEvent OnFetch { get; set; }
    public CloseEvent OnClose { get; set; }

    GDKnyttAmbiTrack track1;
    GDKnyttAmbiTrack track2;
    bool on_track1 = true;
    public GDKnyttAmbiTrack CurrentTrack
    {
        get { return on_track1 ? track1 : track2; }
    }

    bool has_custom_volume;
    public float Volume;

    public override void _Ready()
    {
        track1 = GetNode<GDKnyttAmbiTrack>("AmbiTrack1");
        track2 = GetNode<GDKnyttAmbiTrack>("AmbiTrack2");
    }

    public void setTrack(int num, bool has_custom_volume = false)
    {
        this.has_custom_volume = has_custom_volume;

        // If already playing it, no change
        if (CurrentTrack.AmbiNum == num) { return; }
        
        AudioStream stream = null;
        if (num != 0) { stream = this.OnFetch?.Invoke(num); }

        // Fade out and close the track's stream if it's not already 0
        if (CurrentTrack.AmbiNum != 0) 
        { 
            OnClose?.Invoke(CurrentTrack.AmbiNum);
            CurrentTrack.fadeOut();
        }
        
        // Flip the tracks
        flipTracks();

        // If already playing, simply fade back in, else change to that track
        if (CurrentTrack.AmbiNum == num) { CurrentTrack.fadeIn(); }
        else { CurrentTrack.changeTrack(num, stream); }

        if (has_custom_volume)
        {
            CurrentTrack.mute();
            Volume = -100;
        }
    }

    private void flipTracks()
    {
        on_track1 = !on_track1;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (has_custom_volume)
        {
            CurrentTrack.VolumeDb = Volume;
            Volume = -100;
        }
    }
}
