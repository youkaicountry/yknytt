using System.Collections.Generic;
using Godot;
using YUtil.Random;

public class BuzzFlyer : GDKnyttBaseObject
{
    struct BuzzFlyerParams
    {
        public float speed;
        public bool enemy;

        public BuzzFlyerParams(float speed, bool enemy)
        {
            this.speed = speed;
            this.enemy = enemy;
        }
    }

    static readonly Vector2[] Directions = new Vector2[] {
        new Vector2(1, 0),
        new Vector2(1, -1),
        new Vector2(1, 1),
        new Vector2(-1, 0),
        new Vector2(-1, -1),
        new Vector2(-1, 1),
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(0, 0) };
    
    Vector2 NextDirection { get { return random.NextElement(Directions); } }
    Vector2 Buzz 
    {
        get 
        {
            return new Vector2(random.NextFloat(-BuzzStrength, BuzzStrength), 
                               random.NextFloat(-BuzzStrength, BuzzStrength));
        }
    }
    Vector2 CurrentDirection { get; set; }
    static Vector2 MoveTime = new Vector2(.5f, 4f);
    const float BuzzStrength = 100f;

    static Dictionary<int, BuzzFlyerParams> ID2Params;

    static BuzzFlyer()
    {
        ID2Params = new Dictionary<int, BuzzFlyerParams>();
        ID2Params.Add(15, new BuzzFlyerParams(speed:45f, enemy:true));
        ID2Params.Add(16, new BuzzFlyerParams(speed:45f, enemy:false));
        ID2Params.Add(17, new BuzzFlyerParams(speed:45f, enemy:false));
    }

    BuzzFlyerParams _params;

    public override void _Ready()
    {
        _params = ID2Params[ObjectID.y];
        OrganicEnemy =_params.enemy;
        GetNode<AnimatedSprite>("AnimatedSprite").Play(ObjectID.y.ToString());
        startMove();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        var movement = (CurrentDirection*_params.speed*delta) + Buzz*delta;
        var col = Call("move_and_collide", movement, true, true, true) as KinematicCollision2D;
        var offscreen = !GDArea.isIn(Center + (movement*6f));
        if (col == null && !offscreen) { Translate(movement); }
        else { startMove(); }
    }

    private void startMove()
    {
        CurrentDirection = NextDirection;
        var timer = GetNode<Timer>("FlyTimer");
        timer.Stop();
        timer.WaitTime = random.NextFloat(MoveTime.x, MoveTime.y);
        timer.Start();
    }

    public void _on_FlyTimer_timeout()
    {
        startMove();
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (_params.enemy) { ((Juni)body).die(); }
    }
}
