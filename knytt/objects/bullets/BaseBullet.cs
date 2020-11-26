using Godot;
using System;

public class BaseBullet : KinematicBody2D
{
    private float _velocity;
    protected float _velocity_x;
    protected float _velocity_y;
    private float _direction;
    private float _gravity;
    private bool _enabled;

    public float Velocity { get { return _velocity; } set { _velocity = value; updateAxisVelocity(); } }
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public float Direction { get { return _direction; } set { _direction = value; updateAxisVelocity(); } }
    
    private const float VELOCITY_SCALE = 50f / 8;
    private const float GRAVITY_SCALE = VELOCITY_SCALE * VELOCITY_SCALE;
    private const float DIRECTION_SCALE = Mathf.Pi / 16f;

    public float VelocityMMF2 { get { return Velocity / VELOCITY_SCALE; } set { Velocity = value * VELOCITY_SCALE; } }
    public float GravityMMF2 { get { return Gravity / GRAVITY_SCALE; } set { Gravity = value * GRAVITY_SCALE; } }
    public float DirectionMMF2 { get { return Direction / DIRECTION_SCALE; } set { Direction = value * DIRECTION_SCALE; } }
    public float DecelerationMMF2 { get; set; }

    protected void updateAxisVelocity()
    {
        _velocity_x = _velocity * -Mathf.Cos(_direction);
        _velocity_y = _velocity * -Mathf.Sin(_direction);
    }

    
    public GDKnyttArea GDArea { protected get; set; }
    public AudioStreamPlayer2D DisapperarPlayer { protected get; set; }
    
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
        _velocity_y += _gravity * delta;
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
            if (DisapperarPlayer != null)
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
            collisionShape.SetDeferred("disabled", !value);
        }
    }
}
