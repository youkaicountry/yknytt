public class RollerMuff : Muff
{
    protected bool roll = false;

    private void _on_RollTimer_timeout_ext()
    {
        roll = true;
        speed = 24;
        changeDirection(getDirection(DirectionChange.TowardsJuni));
        sprite.Play("roll");
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!roll) { base._on_SpeedTimer_timeout(); }
    }

    protected override void collide()
    {
        if (roll)
        {
            roll = false;
            speed = 0;
            sprite.Play("default");
        }
        base.collide();
    }
}
