using Godot;

public class LocalAmbient : GDKnyttBaseObject
{
    private GDKnyttAmbiChannel channel;

    public override void _Ready()
    {
        if (ObjectID.y == 37)
        {
            GDArea.NoAmbiance1FadeIn = true;
            channel = GDArea.GDWorld.Game.AmbianceChannel1;
        }
        else
        {
            GDArea.NoAmbiance2FadeIn = true;
            channel = GDArea.GDWorld.Game.AmbianceChannel2;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        channel.CurrentTrack.VolumeDb = -Mathf.Pow(40, Juni.distance(Center) / 240);
    }
}
