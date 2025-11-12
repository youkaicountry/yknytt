using Godot;
using YUtil.Math;

public class GDKnyttAmbiTrack : AudioStreamPlayer
{
    public int AmbiNum { get; set; }

    float fade_i = 0f;
    float fade_target = 0f;
    public bool fading = false;
    public bool muting = false;

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

    public void fadeIn(bool smart = false)
    {
        if (smart) { fade_i = Mathf.Clamp(Mathf.InverseLerp(-15f, 0f, VolumeDb), 0, 1); }
        fade_target = 1f;
        fading = true;
        muting = false;
    }

    public void fadeOut(bool mute = false, bool smart = false)
    {
        if (smart) { fade_i = Mathf.Clamp(Mathf.InverseLerp(-15f, 0f, VolumeDb), 0, 1); }
        fade_target = 0f;
        fading = true;
        muting = mute;
    }

    public void mute()
    {
        VolumeDb = -100f;
        fading = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!fading) { return; }

        if (MathTools.MoveTowards(ref fade_i, fade_target, .8f * delta))
        {
            fading = false;
            if (fade_i == 0 && !muting)
            {
                AmbiNum = 0;
                Stop();
                Stream = null;
            }
        }

        VolumeDb = Mathf.Lerp(-15f, 0f, fade_i);
        if (muting && !fading) { muting = false; VolumeDb = -100; }
    }
}
