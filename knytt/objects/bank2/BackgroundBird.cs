using Godot;
using YUtil.Random;

public class BackgroundBird : GDKnyttBaseObject
{
    private float speed;
    private int direction;

    public override void _Ready()
    {
        direction = random.NextBoolean() ? -1 : 1;
        GetNode<AnimatedSprite>("AnimatedSprite").FlipH = direction < 0;
        randomize();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Position.x + 12 > 640) { Position = new Vector2(-24 - 12, Position.y); randomize(); }
        if (Position.x + 12 < -40) { Position = new Vector2(624 - 12, Position.y); randomize(); }
        Translate(new Vector2(speed, 0) * direction * delta);
    }

    private void randomize()
    {
        speed = (4 + random.Next(5)) * 50f / 8;
        GetNode<AnimatedSprite>("AnimatedSprite").Play($"fly{random.Next(2)}");
    }
}
