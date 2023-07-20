using Godot;

public class SFXAudioPlayer2D : AudioStreamPlayer2D
{
    public static float GlobalPanningStrength;

    public override void _Ready()
    {
        PanningStrength = GlobalPanningStrength;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (PanningStrength != GlobalPanningStrength) { PanningStrength = GlobalPanningStrength; }
    }
}
