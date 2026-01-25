using Godot;
using YUtil.Random;

public partial class Explosion : GDKnyttBaseObject
{
    private Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        sprite.GetNode<AnimationPlayer>("AnimationPlayer").Play("Explode");
    }

    public void randomizePosition()
    {
        sprite.Position = new Vector2(random.NextFloat(0, 24f), random.NextFloat(0, 24f));
    }
}
