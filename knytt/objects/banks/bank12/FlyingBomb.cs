using Godot;
using YUtil.Random;

public partial class FlyingBomb : GDKnyttBaseObject
{
    protected float xSpeed;
    protected float ySpeed;
    protected float originalY;
    protected bool inAttack;

    protected GhostMod ghostMod;
    protected Timer attackTimer;
    protected AnimatedSprite2D sprite;

    public override void _Ready()
    {
        base._Ready();
        ghostMod = GetNode<GhostMod>("GhostMod");
        attackTimer = GetNode<Timer>("AttackTimer");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        xSpeed = random.NextBoolean() ? 100 : -100;
        originalY = GlobalPosition.Y;
        sprite.Play("walk");
        sprite.FlipH = xSpeed < 0;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!inAttack && Mathf.Abs(Juni.ApparentPosition.X - Center.X) < 100 && attackTimer.IsStopped())
        {
            inAttack = true;
            ySpeed = random.Next(150, 200);
            sprite.Play("attack");
        }

        if (inAttack)
        {
            ySpeed -= 250 * (float)delta;
        }

        Translate(new Vector2(xSpeed * (float)delta, ySpeed * (float)delta));

        float pos = Center.X - GDArea.GlobalPosition.X;
        if (pos < 10) { xSpeed = 100; sprite.FlipH = false; }
        if (pos > 590) { xSpeed = -100; sprite.FlipH = true; }

        if (GlobalPosition.Y < originalY)
        {
            inAttack = false;
            GlobalPosition = new Vector2(GlobalPosition.X, originalY);
            ySpeed = 0;
            sprite.Play("walk");
            attackTimer.Start();
        }
    }

    private void _on_Area2D_body_entered(Node2D body)
    {
        ghostMod.flickerMax = .1f;
        ghostMod.flickerMin = 0f;
    }

    private void _on_Area2D_body_exited(Node2D body)
    {
        if (GetNode<Area2D>("Area2D").GetOverlappingBodies().Count > 1) { return; }
        ghostMod.flickerMax = .4f;
        ghostMod.flickerMin = .2f;
    }
}
