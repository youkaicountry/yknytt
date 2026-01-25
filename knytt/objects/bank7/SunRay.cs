using YUtil.Math;
using YUtil.Random;

public partial class SunRay : GDKnyttBaseObject
{
    const float MIN_BRIGHTNESS = 0f;
    const float MAX_BRIGHTNESS = .25f;

    const float CHANGE_SPEED = .08f;

    float target;

    public override void _Ready()
    {
        base._Ready();
        var m = Modulate;
        m.A = random.NextFloat(MIN_BRIGHTNESS, MAX_BRIGHTNESS);
        Modulate = m;

        chooseTarget();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var m = Modulate;
        double mA = m.A;
        if (MathTools.MoveTowards(ref mA, target, CHANGE_SPEED * delta)) { chooseTarget(); }
        m.A = (float)mA;
        Modulate = m;
    }

    private void chooseTarget()
    {
        target = random.NextFloat(MIN_BRIGHTNESS, MAX_BRIGHTNESS);
    }
}
