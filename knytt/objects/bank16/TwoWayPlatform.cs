public class TwoWayPlatform : OneWayPlatform
{
    private void _on_EnterArea_body_entered(Juni juni)
    {
        if (juni.juniInput.DownHeld && juni.juniInput.JumpHeld) { disablePlatform(true); }
        juni.Connect(nameof(Juni.Jumped), this, nameof(juniJumped));
        juni.OnPlatform = true;
    }

    private void _on_EnterArea_body_exited(Juni juni)
    {
        Juni.Disconnect(nameof(Juni.Jumped), this, nameof(juniJumped));
        juni.OnPlatform = false;
    }

    public void juniJumped(Juni juni, bool real)
    {
        if (!juni.juniInput.DownHeld) { return; }
        disablePlatform(true);
        if (juni.velocity.y != 0) { juni.executeJump(0, sound: false, reset_jumps: true); }
    }
}
