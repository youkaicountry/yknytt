using YUtil.Random;

public class Jelly : Muff
{
    protected override void init()
    {
        changeSpeed(0);
    }

    private void _on_AnimatedSprite_animation_finished()
    {
        if (sprite.Animation == "default") { changeSpeed(initialSpeed); }
    }

    protected override void changeSpeed(float s)
    {
        if (s == 0) { changeDirection(GDKnyttDataStore.random.NextBoolean() ? -1 : 1); }
        base.changeSpeed(s);
    }
}
