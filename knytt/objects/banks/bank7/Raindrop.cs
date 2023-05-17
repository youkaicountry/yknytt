using Godot;

public class Raindrop : Node2D
{
    [Export] float baseFallspeed = 420f;
    [Export] float variance = 60f;
    [Export] public float max_distance = 100f;
    float speed;
    float distance = 0f;
    Sprite sprite;
    Rain rain;
    bool init;

    bool recycled = false;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        if (rain == null) { reset(null); }
    }

    public void reset(Rain rain)
    {
        this.rain = rain;
        this.init = true;
        recycled = false;
        this.SetPhysicsProcess(true);
    }

    private void initialize()
    {
        this.distance = 0f;
        recycled = false;
        Visible = true;
        sprite.Visible = true;
        int drop_type = GDKnyttDataStore.random.Next(3);
        this.sprite.Frame = drop_type;
        this.speed = baseFallspeed / ((float)(drop_type + 1));
        this.speed -= ((float)GDKnyttDataStore.random.NextDouble() * variance);
        this.init = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (init) { initialize(); }

        var mv = speed * delta;
        this.Translate(new Vector2(0, mv));
        distance += mv;
        if (distance >= max_distance) { retire(); }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (sprite.Frame == 0 && (GDKnyttDataStore.random.NextDouble() < .4)) { splash(); }
        else { retire(); }
    }

    private void retire()
    {
        if (rain == null) { QueueFree(); return; }
        if (!recycled) { rain.recycleRaindrop(this); }
        this.recycled = true;
    }

    public async void splash()
    {
        speed = 0f;
        sprite.Visible = false;
        var splash = GetNode<AnimatedSprite>("Splash");
        splash.Frame = 0;
        splash.Play();
        await ToSignal(splash, "animation_finished");
        retire();
    }
}
