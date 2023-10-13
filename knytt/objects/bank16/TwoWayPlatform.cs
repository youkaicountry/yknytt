public class TwoWayPlatform : OneWayPlatform
{
    private void _on_EnterArea_body_entered(Juni juni)
    {
        juni.Connect(nameof(Juni.Jumped), this, nameof(juniJumped));
    }

    private void _on_EnterArea_body_exited(Juni juni)
    {
        Juni.Disconnect(nameof(Juni.Jumped), this, nameof(juniJumped));
    }

    public void juniJumped(Juni juni, bool real)
    {
        if (!juni.juniInput.DownHeld) { return; }
        disablePlatform(true);
        if (juni.velocity.y != 0) { juni.executeJump(0, sound: false, reset_jumps: true); }
    }
}
