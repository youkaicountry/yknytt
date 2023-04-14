using Godot;

public abstract partial class ShockDisk : Muff
{
    private void _on_ShotTimer_timeout_ext()
    {
        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        for (int i = 0; i < 17; i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
    }
}
