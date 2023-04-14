public partial class RandomWalkingBouncer : WalkingBouncer
{
    private void _on_JumpTimer_timeout()
    {
        if (!inAir && random.Next(8) == 0) { jump(); }
    }
}
