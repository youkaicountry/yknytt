using Godot;
using YUtil.Random;

public class Ghost : GhostObject
{
    float _speed;
    private float Speed 
    { 
        get { return _speed; }

        set 
        {
            _speed = value;
            GetNode<AnimatedSprite>("AnimatedSprite").FlipH = _speed < 0f;
        }
    }

    private bool Invisible
    {
        set
        {
            if (value)
            {
                flickerMax = .05f;
                flickerMin = 0f;
            }
            else
            {
                flickerMin = .35f;
                flickerMax = .7f;
            }
        }
    }

    private Area2D Area { get; set; }

    private float height;
    private float time = 0f;
    
    private const float HOVER_HEIGHT = 6f;
    private const float HOVER_SPEED = 1.5f;

    private const float MIN_SPEED = 12f;
    private const float MAX_SPEED = 48f;

    private const float MIN_WAIT = 4f;
    private const float MAX_WAIT = 12f;

    public override void _Ready()
    {
        base._Ready();
        Area = GetNode<Area2D>("Area2D");
        flip_time = .08f;
        change_fraction = 4f;
        height = GlobalPosition.y;
        move();
    }

    protected override void _InvProcess(float delta)
    {
        time += delta;

        var gp = GlobalPosition;
        gp.x += Speed*delta;
        gp.y = height + (Mathf.Sin(time*HOVER_SPEED)*HOVER_HEIGHT);
        GlobalPosition = gp;

        // Collide with area edge
        var dp = GlobalPosition + new Vector2(12f+(12f*Mathf.Sign(Speed)), 0f);
        if (!GDArea.isIn(dp)) { Speed *= -1f; }
    }

    private void move()
    {
        Speed = GDKnyttDataStore.random.NextFloat(MIN_SPEED, MAX_SPEED);
        if (GDKnyttDataStore.random.NextBoolean()) { Speed *= -1f; }

        var timer = GetNode<Timer>("MoveTimer");
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(MIN_WAIT, MAX_WAIT);
        timer.Start();
    }

    public void _on_MoveTimer_timeout()
    {
        move();
    }

    public void _on_Area2D_body_entered(Node body)
    {
        Invisible = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (Area.GetOverlappingBodies().Count == 1) { Invisible = false; }
    }
}
