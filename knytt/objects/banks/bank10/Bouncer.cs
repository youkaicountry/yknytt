using Godot;

public partial class Bouncer : GDKnyttBaseObject
{
    [Export] float initialSpeed = 0;
    [Export] float gravity = 0.2f;
    [Export] int[] speedValues = null;
    [Export] bool randomSpeed = false;
    [Export] int counter = 0;

    protected float speed;
    protected AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        base._Ready();
        speed = -initialSpeed;
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        speed += gravity * 50 * (float)delta;
        Translate(new Vector2(0, speed));
    }

    protected virtual void _on_Area2D_body_entered(Node2D body)
    {
        if (body is Juni juni) { juniDie(juni); return; }

        animatedSprite.Frame = 0;
        animatedSprite.Play("bounce");
        GetNode<AudioStreamPlayer2D>("BouncePlayer").Play();
        speed = -nextSpeed();
    }

    protected float nextSpeed()
    {
        if (randomSpeed) { return random.NextElement(speedValues); }

        var result = speedValues[counter];
        counter = (counter + 1) % speedValues.Length;
        return result;
    }
}
