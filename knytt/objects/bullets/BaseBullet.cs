using Godot;

public class BaseBullet : KinematicBody2D
{
    private float velocity;
    private float velocity_x;
    private float velocity_y;
    private float deceleration;
    private float deceleration_x;
    private float deceleration_y;
    private float direction;
    private float gravity;
    private bool enabled;
    private int enable_countdown;
    private int disappear_countdown;
    private bool delayed_collide;
    private bool safe;

    public float Velocity { get { return velocity; } set { velocity = value; updateAxisVelocity(); } }
    public float Gravity { get { return gravity; } set { gravity = value; } }
    public float Direction { get { return direction; } set { direction = value; updateAxisVelocity(); } }
    public float Deceleration { get { return deceleration; } set { deceleration = value; updateAxisVelocity(); } }
    public bool EnableRotation { get; set; } = false;
    public bool DisappearWhenStopped { get; set; } = false;
    public bool Safe { get { return safe; } set { safe = value; collisionShape.SetDeferred("disabled", true);  } }

    private float deceleration_correction_x = 1;
    private float deceleration_correction_up = 1;
    private float deceleration_correction_down = 1;

    private const float VELOCITY_SCALE = 50f / 8;
    private const float GRAVITY_SCALE = VELOCITY_SCALE * VELOCITY_SCALE;
    // TODO: more accurate value when gravity != 0!
    private const float DECELERATION_SCALE = 0.05f; // 0.025f;

    public float VelocityMMF2 { get { return Velocity / VELOCITY_SCALE; } set { Velocity = value * VELOCITY_SCALE; } }
    public float GravityMMF2 { get { return Gravity / GRAVITY_SCALE; } set { Gravity = value * GRAVITY_SCALE; } }
    public float DirectionMMF2 { get { return 16 * (1 - Direction / Mathf.Pi); } set { Direction = (1 - value / 16) * Mathf.Pi; } }
    public float DecelerationMMF2 { get { return Deceleration / DECELERATION_SCALE; } set { Deceleration = value * DECELERATION_SCALE; } }

    // Be careful! Doesn't support deceleration, total velocity and direction change
    public float VelocityX { get { return velocity_x; } set { velocity_x = value; } }
    public float VelocityY { get { return velocity_y; } set { velocity_y = value; } }

    protected void updateAxisVelocity()
    {
        velocity_x = velocity * -Mathf.Cos(direction);
        velocity_y = velocity * -Mathf.Sin(direction);
        deceleration_x = velocity_x * deceleration;
        deceleration_y = Mathf.Abs(velocity_y * deceleration);
    }

    public bool PinballCorrection
    {
        set
        {
            // TODO: more accurate values
            deceleration_correction_x = value ? 0.78f : 1;
            deceleration_correction_up = value ? 0.5f : 1;
            deceleration_correction_down = value ? 0.5f : 1;
        }
    }


    public GDKnyttArea GDArea { protected get; set; }

    protected AnimatedSprite sprite;
    protected CollisionShape2D collisionShape;
    protected AudioStreamPlayer2D hitPlayer;
    protected bool hasDisappear;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        hitPlayer = GetNodeOrNull<AudioStreamPlayer2D>("HitPlayer");
        hasDisappear = sprite.Frames.HasAnimation("disappear");
        sprite.Play("default");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        // Workaround to make sure that Translate was made before actual enabling
        // Without this, player can move with a particle and re-enter an area that has been left!
        if (collisionShape.Disabled && --enable_countdown <= 0 && !Safe) { collisionShape.SetDeferred("disabled", false); }
        // Without delay, floor collision can happen before collision with Juni
        if (disappear_countdown > 0)
        {
            if (--disappear_countdown == 0) { disappear(collide: delayed_collide); }
            return;
        }

        velocity_x -= deceleration_x * delta * deceleration_correction_x;
        if (deceleration_x > 0 && velocity_x < 0) { velocity_x = 0; }
        if (deceleration_x < 0 && velocity_x > 0) { velocity_x = 0; }

        if (velocity_y > 0)
        {
            velocity_y -= deceleration_y * delta * deceleration_correction_down;
            if (gravity == 0 && velocity_y < 0) { velocity_y = 0; }
        }
        else if (velocity_y < 0)
        {
            velocity_y += deceleration_y * delta * deceleration_correction_up;
            if (gravity == 0 && velocity_y > 0) { velocity_y = 0; }
        }

        if (DisappearWhenStopped && gravity == 0 && velocity_x == 0 && velocity_y == 0)
        {
            disappear(collide: false);
        }

        velocity_y += gravity * delta;

        if (EnableRotation) { Rotation = Mathf.Atan2(-velocity_y, -velocity_x); }

        var collision = MoveAndCollide(new Vector2(delta * velocity_x, delta * velocity_y));
        if (!GDArea.isIn(GlobalPosition, x_border: 2, y_border: 2))
        {
            disappear_countdown = 1;
            delayed_collide = false;
        }
        else if (collision != null)
        {
            disappear_countdown = 1;
            delayed_collide = true;
        }
    }

    public virtual async void disappear(bool collide)
    {
        Enabled = false;
        velocity_x = velocity_y = gravity = 0;
        if (collide)
        {
            hitPlayer?.Play();

            if (hasDisappear)
            {
                sprite.Play("disappear");
                await ToSignal(sprite, "animation_finished");
            }
        }
        Visible = false;
    }

    public virtual bool Enabled
    {
        get { return enabled; }
        set
        {
            enabled = value;
            if (value && hasDisappear) { GetNode<AnimatedSprite>("AnimatedSprite").Play("default"); }
            if (!value) { collisionShape.SetDeferred("disabled", true); } else { enable_countdown = 2; }
        }
    }
}
