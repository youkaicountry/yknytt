public class HoloAutomation : Automation
{
    private bool late_holo;

    protected override void _on_Area2D_body_entered(Juni juni)
    {
        if (!(juni.CurrentState is WalkRunState || juni.CurrentState is IdleState)) { late_holo = true; }
        base._on_Area2D_body_entered(juni);
    }

    protected override void _on_Area2D_body_exited(Juni juni)
    {
        base._on_Area2D_body_exited(juni);
        late_holo = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (juni != null && late_holo && (juni.CurrentState is WalkRunState || juni.CurrentState is IdleState))
        {
            juni.juniInput.altInput.ActionPress("hologram");
        }
    }
}
