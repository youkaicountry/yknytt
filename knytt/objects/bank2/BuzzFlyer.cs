using Godot;
using YUtil.Random;

public class BuzzFlyer : GDKnyttBaseObject
{
    [Export] protected float speed = 48;
    [Export] protected float buzzStrength = 66;
    [Export] protected float border = 10;

    protected static readonly Vector2[] Directions = new Vector2[] {
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
        new Vector2(-1, 0),  new Vector2(0, 0),  new Vector2(1, 0),
        new Vector2(-1, 1),  new Vector2(0, 1),  new Vector2(1, 1),
    };

    protected Vector2 currentDirection;

    public override void _Ready()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Play();
        changeDirection();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        var buzz = new Vector2(random.NextFloat(-buzzStrength, buzzStrength),
                               random.NextFloat(-buzzStrength, buzzStrength));
        var movement = currentDirection * speed * delta + buzz * delta;
        var collision = moveAndCollide(movement, testOnly: true);
        var offscreen = !GDArea.isIn(Center + movement, x_border: border, y_border: border);
        if (collision == null && !offscreen) { Translate(movement); }
        else { changeDirection(); GetNode<Timer>("FlyTimer").Start(); }
    }

    protected virtual void changeDirection()
    {
        currentDirection = random.NextElement(Directions);
    }
}
