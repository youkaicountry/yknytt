public class JumpAutomation : Automation
{
    private int late_jump;

    protected override void _on_Area2D_body_entered(Juni juni)
    {
        if (!juni.Grounded && juni.jumps == 1) { late_jump = 2; }
        base._on_Area2D_body_entered(juni);
    }

    protected override void _on_Area2D_body_exited(Juni juni) { }

    private void _on_ExitArea2D_body_exited(Juni juni)
    {
        if (juni != null)
        {
            base._on_Area2D_body_exited(juni);
            late_jump = 0;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni != null && late_jump > 0 && juni.Grounded && --late_jump == 0)
        {
            juni.juniInput.altInput.ActionPress("jump");
        }
    }
}
