using Godot;

public partial class JuniAudio : Node2D
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
}
