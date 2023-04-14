using Godot;

public partial class HomingEnemy : GDKnyttBaseObject
{
    [Export] protected int initialSpeed = 0;
    [Export] protected int wallSpeed = 0;
    [Export] protected int airSpeed = 0;
    [Export] protected Color wallColor = new Color(1, 1, 1, 0.5f);

    protected const float SPEED_SCALE = 48f / 8;
    protected const float JUNI_MIN_DISTANCE = 6;

    protected int speed;
    protected Vector2 speedVector;

    protected AnimatedSprite2D sprite;
    protected bool hasWallAnimation;

    public override void _Ready()
    {
        speed = initialSpeed;
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        hasWallAnimation = sprite.SpriteFrames.HasAnimation("wall");
        sprite.Play("default");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        float distance = Juni.distance(Center);
        if (distance > JUNI_MIN_DISTANCE)
        {
            var inertia = 0.9f - (float)delta * 3; // TODO: replace this magic formula with more correct and fps-independent behaviour
            var new_speed_vector = speed * SPEED_SCALE * (Juni.ApparentPosition - Center) / distance;
            speedVector = speedVector * inertia + new_speed_vector * (1 - inertia);
        }
        Translate(speedVector * (float)delta);
    }

    private void _on_Area2D_body_entered(Node2D body)
    {
        if (body is Juni juni) { juniDie(juni); return; }
        Modulate = wallColor;
        speed = wallSpeed;
        if (hasWallAnimation) { sprite.Play("wall"); }
    }

    private void _on_Area2D_body_exited(Node2D body)
    {
        if (GetNode<Area2D>("Area2D").GetOverlappingBodies().Count > 1) { return; }
        Modulate = new Color(1, 1, 1, 1);
        speed = airSpeed;
        if (hasWallAnimation) { sprite.Play("default"); }
    }
}
