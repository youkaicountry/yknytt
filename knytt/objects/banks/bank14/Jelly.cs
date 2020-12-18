using Godot;
using YUtil.Random;

public class Jelly : Muff
{
    [Export] int jellySpeed = 0;
    
    private void _on_AnimatedSprite_animation_finished()
    {
        if (sprite.Animation == "default") { changeSpeed(jellySpeed); }
    }

    protected override void changeSpeed(float s)
    {
        if (s == 0) { changeDirection(random.NextBoolean() ? -1 : 1); }
        base.changeSpeed(s);
    }
}
