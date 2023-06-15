using Godot;
using YUtil.Random;

public class FlyingBomb : GDKnyttBaseObject
{
    protected float xSpeed;
    protected float ySpeed;
    protected float originalY;
    protected bool inAttack;

    protected GhostMod ghostMod;
    protected Timer attackTimer;
    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();
        ghostMod = GetNode<GhostMod>("GhostMod");
        attackTimer = GetNode<Timer>("AttackTimer");
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        xSpeed = random.NextBoolean() ? 100 : -100;
        originalY = GlobalPosition.y;
        sprite.Play("walk");
        sprite.FlipH = xSpeed < 0;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (!inAttack && Mathf.Abs(Juni.ApparentPosition.x - Center.x) < 100 && attackTimer.IsStopped())
        {
            inAttack = true;
            ySpeed = random.Next(150, 200);
            sprite.Play("attack");
        }

        if (inAttack)
        {
            ySpeed -= 250 * delta;
        }

        Translate(new Vector2(xSpeed * delta, ySpeed * delta));

        float pos = Center.x - GDArea.GlobalPosition.x;
        if (pos < 10) { xSpeed = 100; sprite.FlipH = false; }
        if (pos > 590) { xSpeed = -100; sprite.FlipH = true; }

        if (GlobalPosition.y < originalY)
        {
            inAttack = false;
            GlobalPosition = new Vector2(GlobalPosition.x, originalY);
            ySpeed = 0;
            sprite.Play("walk");
            attackTimer.Start();
        }
    }

    private void _on_Area2D_body_entered(object body)
    {
        ghostMod.flickerMax = .1f;
        ghostMod.flickerMin = 0f;
    }

    private void _on_Area2D_body_exited(object body)
    {
        if (GetNode<Area2D>("Area2D").GetOverlappingBodies().Count > 1) { return; }
        ghostMod.flickerMax = .4f;
        ghostMod.flickerMin = .2f;
    }
}
