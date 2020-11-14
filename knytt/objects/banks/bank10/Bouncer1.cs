
public class Bouncer1 : Bouncer
{
    protected float newSpeed = 6;

    protected override float nextSpeed()
    {
        newSpeed = newSpeed >= 6 ? 2 : newSpeed + 1;
        return newSpeed;
    }
}
