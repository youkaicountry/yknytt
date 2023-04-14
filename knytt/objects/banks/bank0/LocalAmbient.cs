using Godot;

public partial class LocalAmbient : GDKnyttBaseObject
{
    private int bus;

    public override void _Ready()
    {
        GDKnyttAmbiChannel channel = null;

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

        bus = AudioServer.GetBusIndex(channel.CurrentTrack.Bus);
    }

    public override void _PhysicsProcess(double delta)
    {
        AudioServer.SetBusVolumeDb(bus, -Mathf.Pow(40, Juni.distance(Center) / 240));
    }
}
