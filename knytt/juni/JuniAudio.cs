using Godot;

public class JuniAudio : Node2D
{
    public void stopAll()
    {
        foreach (var child in GetChildren())
        {
            ((AudioStreamPlayer2D)child).Stop();
        }
    }
}
