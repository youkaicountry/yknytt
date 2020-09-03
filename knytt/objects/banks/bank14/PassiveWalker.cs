using Godot;
using YUtil.Random;

public class PassiveWalker : GDKnyttBaseObject
{
    public Area2D LeftArea { get; private set; }
    public Area2D RightArea { get; private set; }
    public AnimatedSprite Sprite { get; private set; }
    public bool FacingRight { get { return !Sprite.FlipH; } set { Sprite.FlipH = !value; }}
    public bool Moving { get; private set; }
    public bool IsBlocked 
    { 
        get 
        { 
            var carea = FacingRight ? RightArea : LeftArea;
            return carea.GetOverlappingBodies().Count > 0 || !GDArea.isIn(carea.GlobalPosition);
        } 
    }

    public float speed;

    float base_speed = 25f;

    public override void _Ready()
    {
        Sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        LeftArea = GetNode<Area2D>("LeftArea");
        RightArea = GetNode<Area2D>("RightArea");
        startMoving();
    }

    private void stopMoving()
    {
        this.Moving = false;
        Sprite.Stop();
        Sprite.Frame = 0;
        GetNode<Timer>("Timer").WaitTime = GDKnyttDataStore.random.NextFloat(1f, 6f);
        GetNode<Timer>("Timer").Start();
    }

    private void startMoving()
    {
        FacingRight = GDKnyttDataStore.random.Next(2) == 0;
        this.Moving = true;
        Sprite.Play(string.Format("Passive{0}", ObjectID.y));
        var mul = GDKnyttDataStore.random.NextFloat(1.2f, 2.5f);
        speed = base_speed * mul;
        Sprite.SpeedScale = mul;
        GetNode<Timer>("Timer").WaitTime = GDKnyttDataStore.random.NextFloat(.5f, 5f);
        GetNode<Timer>("Timer").Start();
    }

    public void _on_Timer_timeout()
    {
        if (Moving) { stopMoving(); }
        else { startMoving(); }
    }

    protected override void _impl_initialize() { }

    protected override void _impl_process(float delta)
    {
        if (!Moving) { return; }
        if (IsBlocked) { FacingRight = !FacingRight; }
        Translate(new Vector2(delta*speed * (FacingRight ? 1f : -1f), 0f));
    }
}
