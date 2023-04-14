using Godot;
using YUtil.Math;

public partial class GDKnyttAmbiTrack : AudioStreamPlayer
{
    public int AmbiNum { get; set; }

    float fade_i = 0f;
    float fade_target = 0f;
    bool fading = false;

    // Set the track
    public void changeTrack(int num, AudioStream stream)
    {
        AmbiNum = num;
        Stream = stream;

        if (num == 0) { return; }

        fade_i = 0f;
        fadeIn();
        Play();
    }

    public void fadeIn()
    {
        fade_target = 1f;
        fading = true;
    }

    public void fadeOut()
    {
        fade_target = 0f;
        fading = true;
    }

    public void fullVolume()
    {
        fade_i = 1f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!fading) { return; }

        if (MathTools.MoveTowards(ref fade_i, fade_target, .8f * (float)delta))
        {
            fading = false;
            if (fade_i == 0)
            {
                AmbiNum = 0;
                Stop();
                Stream = null;
            }
        }

        VolumeDb = Mathf.Lerp(-15f, 0f, fade_i);
    }
}
