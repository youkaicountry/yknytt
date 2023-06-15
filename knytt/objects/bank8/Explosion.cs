using Godot;
using YUtil.Random;

public class Explosion : GDKnyttBaseObject
{
    private Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        sprite.GetNode<AnimationPlayer>("AnimationPlayer").Play("Explode");
    }

    public void randomizePosition()
    {
        sprite.Position = new Vector2(random.NextFloat(0, 24f), random.NextFloat(0, 24f));
    }
}
