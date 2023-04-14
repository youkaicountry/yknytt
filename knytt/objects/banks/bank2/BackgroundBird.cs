using Godot;
using YUtil.Random;

public partial class BackgroundBird : GDKnyttBaseObject
{
    private float speed;
    private int direction;

    public override void _Ready()
    {
        direction = random.NextBoolean() ? -1 : 1;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = direction < 0;
        randomize();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Position.X + 12 > 640) { Position = new Vector2(-24 - 12, Position.Y); randomize(); }
        if (Position.X + 12 < -40) { Position = new Vector2(624 - 12, Position.Y); randomize(); }
        Translate(new Vector2(speed, 0) * direction * (float)delta);
    }

    private void randomize()
    {
        speed = (4 + random.Next(5)) * 50f / 8;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play($"fly{random.Next(2)}");
    }
}
