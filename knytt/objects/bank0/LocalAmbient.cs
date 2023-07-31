using Godot;

public class LocalAmbient : GDKnyttBaseObject
{
    GDKnyttAmbiChannel channel = null;

    public override void _Ready()
    {
        if (ObjectID.y == 37)
        {
            GDArea.Ambiance1CustomVolume = true;
            channel = GDArea.GDWorld.Game.AmbianceChannel1;
        }
        else
        {
            GDArea.Ambiance2CustomVolume = true;
            channel = GDArea.GDWorld.Game.AmbianceChannel2;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        channel.Volume = Mathf.Max(channel.Volume, -Mathf.Pow(40, Juni.distance(Center) / 240));
    }
}
