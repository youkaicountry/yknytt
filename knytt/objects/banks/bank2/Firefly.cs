using Godot;
using System;
using YUtil.Random;

public partial class Firefly : GDKnyttBaseObject
{
    [Export] protected float limitSpeed = 0;
    [Export] protected float resetSpeed = 0;
    [Export] protected float idleSpeed = 0;
    [Export] protected float runSpeed = 0;
    [Export] protected float speedBuzz = 0;
    [Export] protected float speedOffset = 0;
    [Export] protected float directionBuzz = 0;
    [Export] protected float opacityBuzz = 0;
    private float SPEED_SCALE = 50 / 8f;

    protected AnimatedSprite2D sprite;

    protected float speed = 0;
    protected float direction = 0;
    protected float opacity = 1;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        direction = random.NextFloat(2 * Mathf.Pi);
    }

    public override void _PhysicsProcess(double delta)
    {
        // TODO: not very accurate (no reappear, different speed, etc). Rewrite if required.
        speed += random.NextFloat(-speedBuzz + speedOffset, speedBuzz + speedOffset) * (float)delta;
        if (speed > limitSpeed) { speed = resetSpeed; }
        
        direction += random.NextFloat(-directionBuzz, directionBuzz) * (float)delta;
        
        opacity += random.NextFloat(-opacityBuzz, opacityBuzz) * (float)delta;
        opacity = Mathf.Max(0, Mathf.Min(1, opacity));
        
        var diff = new Vector2(speed, 0).Rotated(direction);
        var collision = moveAndCollide(diff * (float)delta * SPEED_SCALE);
        if (collision != null) { direction = diff.Bounce(collision.GetNormal()).Angle(); }
        if (!GDArea.isIn(Center)) { direction = Center.AngleToPoint(GDArea.GlobalPosition + new Vector2(300, 120)); }
        
        sprite.Rotation = direction;
        Modulate = new Color(1, 1, 1, opacity);

        string anim = speed <= idleSpeed ? "idle" :
                      speed > runSpeed ? "run" : "fly";
        if (sprite.Animation != anim) { sprite.Play(anim); }
    }
}
