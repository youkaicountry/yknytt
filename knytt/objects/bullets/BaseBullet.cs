using Godot;
using System;

public class BaseBullet : KinematicBody2D
{
    private float _velocity;
    private float _velocity_x;
    private float _velocity_y;
    private float _direction;
    private float _gravity;

    public float Velocity { get { return _velocity; } set { _velocity = value; updateAxisVelocity(); } }
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public float Direction { get { return _direction; } set { _direction = value; updateAxisVelocity(); } }
    
    // TODO: more accurate values to make the same behavior as in the orginal game
    private const float VELOCITY_SCALE = 70f / 11; // 4.5f;
    private const float GRAVITY_SCALE = 40f; //20f;
    private const float DIRECTION_SCALE = Mathf.Pi / 16f;

    public float VelocityMMF2 { get { return Velocity / VELOCITY_SCALE; } set { Velocity = value * VELOCITY_SCALE; } }
    public float GravityMMF2 { get { return Gravity / GRAVITY_SCALE; } set { Gravity = value * GRAVITY_SCALE; } }
    public float DirectionMMF2 { get { return Direction / DIRECTION_SCALE; } set { Direction = value * DIRECTION_SCALE; } }

    protected void updateAxisVelocity()
    {
        _velocity_x = _velocity * -Mathf.Cos(_direction);
        _velocity_y = _velocity * -Mathf.Sin(_direction);
    }

    
    public GDKnyttArea GDArea { get; set; }
    
    protected AnimatedSprite sprite;
    protected CollisionShape2D collisionShape;
    protected bool has_disappear;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        has_disappear = sprite.Frames.HasAnimation("disappear");
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
        _velocity_x = _velocity_y = _gravity = 0;
        if (collide && has_disappear)
        {
            sprite.Play("disappear");
            await ToSignal(sprite, "animation_finished");
        }
        Enabled = false;
    }

    public bool Enabled
    {
        get { return Visible; }
        set
        {
            if (value && has_disappear) { GetNode<AnimatedSprite>("AnimatedSprite").Play("default"); }
            collisionShape.SetDeferred("disabled", !value);
            SetDeferred("visible", value);
        }
    }
}

