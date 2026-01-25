using Godot;

public partial class SFXAudioPlayer2D : AudioStreamPlayer2D
{
    public static float GlobalPanningStrength;

    public override void _Ready()
    {
        PanningStrength = GlobalPanningStrength;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PanningStrength != GlobalPanningStrength) { PanningStrength = GlobalPanningStrength; }
    }
}
