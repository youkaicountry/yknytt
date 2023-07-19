using Godot;

public class JuniAudio : Node2D
{
    public void stopAll()
    {
        foreach (var child in GetChildren())
        {
            switch (child)
            {
                case AudioStreamPlayer player:
                    player.Stop();
                    break;

                case StandartSoundPlayer player:
                    player.stopAll();
                    break;
            }
        }
    }

    public void workaroundPanning(float value)
    {
        foreach (var child in GetChildren())
        {
            switch (child)
            {
                case AudioStreamPlayer2D player:
                    player.PanningStrength *= value;
                    break;

                case StandartSoundPlayer player:
                    player.workaroundPanning(value);
                    break;
            }
        }
    }
}
