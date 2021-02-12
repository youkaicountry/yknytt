using Godot;

public abstract class WalkingBouncer : Muff
{
    [Export] protected float initialJumpSpeed = 0;
    [Export] protected float jumpGravity = 0;

    protected float originY;
    protected bool inAir = false;
    protected float jumpSpeed;

    protected const float JUMP_SCALE = 45; // 50;

    public override void _Ready()
    {
        base._Ready();
        originY = GlobalPosition.y;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (inAir)
        {
            if (moveAndCollide(new Vector2(0, jumpSpeed * JUMP_SCALE * delta)) != null)
            {
                inAir = false;
                GlobalPosition = new Vector2(GlobalPosition.x, originY);
                GetNodeOrNull<RawAudioPlayer2D>("BouncePlayer")?.Play();
                _on_DirectionTimer_timeout();
                _on_SpeedTimer_timeout();
            }
            else
            {
                jumpSpeed += jumpGravity * JUMP_SCALE * delta;
            }
        }
    }

    protected void jump()
    {
        inAir = true;
        speed = 0;
        jumpSpeed = -initialJumpSpeed;
        sprite.Play("jump");
        GetNodeOrNull<RawAudioPlayer2D>("JumpPlayer")?.Play();
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!inAir) { base._on_SpeedTimer_timeout(); }
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (!inAir) { base._on_DirectionTimer_timeout(); }
    }
}
