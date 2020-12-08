using Godot;
using System;

public class BaseBullet : KinematicBody2D
{
    private float _velocity;
    private float _velocity_x;
    private float _velocity_y;
    private float _deceleration;
    private float _deceleration_x;
    private float _deceleration_y;
    private float _direction;
    private float _gravity;
    private bool _enabled;

    public float Velocity { get { return _velocity; } set { _velocity = value; updateAxisVelocity(); } }
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public float Direction { get { return _direction; } set { _direction = value; updateAxisVelocity(); } }
    public float Deceleration { get { return _deceleration; } set { _deceleration = value; updateAxisVelocity(); } }
    public bool EnableRotation { get; set; } = false;

    public float DecelerationCorrectionX { get; set; } = 1;
    public float DecelerationCorrectionUp { get; set; } = 1;
    public float DecelerationCorrectionDown { get; set; } = 1;
    
    private const float VELOCITY_SCALE = 50f / 8;
    private const float GRAVITY_SCALE = VELOCITY_SCALE * VELOCITY_SCALE;
    private const float DIRECTION_SCALE = Mathf.Pi / 16f;
    // TODO: more accurate value when gravity != 0!
    private const float DECELERATION_SCALE = 0.05f; // 0.025f;

    public float VelocityMMF2 { get { return Velocity / VELOCITY_SCALE; } set { Velocity = value * VELOCITY_SCALE; } }
    public float GravityMMF2 { get { return Gravity / GRAVITY_SCALE; } set { Gravity = value * GRAVITY_SCALE; } }
    public float DirectionMMF2 { get { return Direction / DIRECTION_SCALE; } set { Direction = value * DIRECTION_SCALE; } }
    public float DecelerationMMF2 { get { return Deceleration / DECELERATION_SCALE; } set { Deceleration = value * DECELERATION_SCALE; } }

    // Be careful! Doesn't support deceleration, total velocity and direction change
    public float VelocityX { get { return _velocity_x; } set { _velocity_x = value; } }
    public float VelocityY { get { return _velocity_y; } set { _velocity_y = value; } }

    protected void updateAxisVelocity()
    {
        _velocity_x = _velocity * -Mathf.Cos(_direction);
        _velocity_y = _velocity * -Mathf.Sin(_direction);
        _deceleration_x = _velocity_x * _deceleration;
        _deceleration_y = Mathf.Abs(_velocity_y * _deceleration);
    }

    public void ApllyPinballCorrections()
    {
        // TODO: more accurate values
        DecelerationCorrectionX = 0.75f;
        DecelerationCorrectionUp = 0.5f;
        DecelerationCorrectionDown = 0.5f;
    }

    
    public GDKnyttArea GDArea { protected get; set; }
    public RawAudioPlayer2D DisapperarPlayer { protected get; set; }
    
    protected AnimatedSprite sprite;
    protected CollisionShape2D collisionShape;
    protected bool hasDisappear;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        hasDisappear = sprite.Frames.HasAnimation("disappear");
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (!Enabled) { return; }
        // Workaround to make sure that Translate was made before actual enabling
        // Without this, player can move with a particle!
        if (collisionShape.Disabled) { collisionShape.SetDeferred("disabled", false); }
        
        _velocity_x -= _deceleration_x * delta * DecelerationCorrectionX;
        if (_deceleration_x > 0 && _velocity_x < 0) { _velocity_x = 0; }
        if (_deceleration_x < 0 && _velocity_x > 0) { _velocity_x = 0; }

        if (_velocity_y > 0)
        {
            _velocity_y -= _deceleration_y * delta * DecelerationCorrectionDown;
            if (_gravity == 0 && _velocity_y < 0) { _velocity_y = 0; }
        }
        else if (_velocity_y < 0)
        {
            _velocity_y += _deceleration_y * delta * DecelerationCorrectionUp;
            if (_gravity == 0 && _velocity_y > 0) { _velocity_y = 0; }
        }

        if (_gravity == 0 && _velocity_x == 0 && _velocity_y == 0)
        {
            disappear(collide: false);
        }

        _velocity_y += _gravity * delta;

        if (EnableRotation) { Rotation = Mathf.Atan2(_velocity_y, _velocity_x); }

        var collision = MoveAndCollide(new Vector2(delta * _velocity_x, delta * _velocity_y));
        if (collision != null)
        {
            if (collision.Collider is Juni juni) { juni.die(); }
            disappear(!(collision.Collider is Juni));
        }
        else if (!GDArea.isIn(GlobalPosition))
        { 
            disappear(collide: false);
        }
    }

    protected virtual async void disappear(bool collide)
    {
        Enabled = false;
        _velocity_x = _velocity_y = _gravity = 0;
        if (collide)
        {
            if (DisapperarPlayer != null && !DisapperarPlayer.IsDisposed)
            {
                DisapperarPlayer.GlobalPosition = GlobalPosition;
                DisapperarPlayer.Play();
            }
            if (hasDisappear)
            {
                sprite.Play("disappear");
                await ToSignal(sprite, "animation_finished");
            }
        }
        Visible = false;
    }

    public bool Enabled
    {
        get { return _enabled; }
        set
        {
            _enabled = value;
            if (value && hasDisappear) { GetNode<AnimatedSprite>("AnimatedSprite").Play("default"); }
            if (!value) { collisionShape.SetDeferred("disabled", true); }
        }
    }
}
