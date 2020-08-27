using Godot;

public class Raindrop : Node2D
{
    [Export] float baseFallspeed = 420f;
    [Export] float variance = 60f;
    float speed;
    public float max_distance = 100f;
    float distance = 0f;
    Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        int drop_type = GDKnyttDataStore.random.Next(3);
        this.sprite.Frame = drop_type;
        this.speed = baseFallspeed / ((float)(drop_type+1));
        this.speed -= ((float)GDKnyttDataStore.random.NextDouble()*variance);
    }

    public override void _PhysicsProcess(float delta)
    {
        var mv = speed*delta;
        this.Translate(new Vector2(0, this.speed*delta));
        distance += mv;
        if (distance >= max_distance) { QueueFree(); }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (sprite.Frame == 0 && (GDKnyttDataStore.random.NextDouble() < .3)) { splash(); }
        else { QueueFree(); }
    }

    public async void splash()
    {
        speed = 0f;
        sprite.Visible = false;
        GetNode<CPUParticles2D>("Splash").Emitting = true;
        var timer = GetNode<Timer>("SplashTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        QueueFree();
    }
}
