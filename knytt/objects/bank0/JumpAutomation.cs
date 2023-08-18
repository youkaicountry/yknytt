public class JumpAutomation : Automation
{
    private bool late_jump;

    protected override void _on_Area2D_body_entered(Juni juni)
    {
        if (!(juni.CurrentState is WalkRunState || juni.CurrentState is IdleState) && juni.jumps == 1) { late_jump = true; }
        base._on_Area2D_body_entered(juni);
    }

    protected override void _on_Area2D_body_exited(Juni juni) { }

    private void _on_ExitArea2D_body_exited(Juni juni)
    {
        if (juni != null)
        {
            base._on_Area2D_body_exited(juni);
            late_jump = false;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni != null && late_jump && (juni.CurrentState is WalkRunState || juni.CurrentState is IdleState))
        {
            juni.juniInput.altInput.ActionPress("jump");
        }
    }
}
