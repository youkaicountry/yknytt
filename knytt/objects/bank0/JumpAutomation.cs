public class JumpAutomation : Automation
{
    protected override void _on_Area2D_body_exited(Juni juni) { }

    private void _on_ExitArea2D_body_exited(Juni juni)
    {
        if (juni != null)
        {
            base._on_Area2D_body_exited(juni);
        }
    }
}
