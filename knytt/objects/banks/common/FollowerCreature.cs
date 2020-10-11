using System.Collections.Generic;
using Godot;

public class FollowerCreature : GDKnyttBaseObject
{
    bool _at_juni = false;
    AnimatedSprite _sprite;
    Area2D _right_checker;
    Area2D _left_checker;
    bool FacingRight { get { return !_sprite.FlipH; } set { _sprite.FlipH = !value; } }

    struct FollowerParams
    {
        public float speed;
        public bool horizontal;
        public string name;
        public bool organic_enemy;
        public bool deadly;

        public FollowerParams(string name, float speed, bool horizontal, bool deadly, bool organic_enemy)
        {
            this.name = name;
            this.speed = speed;
            this.horizontal = horizontal;
            this.organic_enemy = organic_enemy;
            this.deadly = deadly;
        }
    }

    static FollowerCreature()
    {
        ID2Params = new Dictionary<string, FollowerParams>();
        ID2Params["(3, 5)"] = new FollowerParams(name:"YellowDog", speed:45f, horizontal:true, organic_enemy:false, deadly:false);
        ID2Params["(4, 1)"] = new FollowerParams(name:"RedFollowBall", speed:60f, horizontal:false, organic_enemy:true, deadly:true);
    }

    private static Dictionary<string, FollowerParams> ID2Params;
    FollowerParams _params;

    public override void _Ready()
    {
        base._Ready();

        _params = ID2Params[ObjectID.ToString()];

        _sprite = GetNode<AnimatedSprite>("FollowCreature/AnimatedSprite");
        _right_checker = GetNode<Area2D>("FollowCreature/RightChecker");
        _left_checker = GetNode<Area2D>("FollowCreature/LeftChecker");

        GetNode<Area2D>("FollowCreature/Area2D").Connect("body_entered", this, "_on_Area2D_body_entered");
        GetNode<Area2D>("FollowCreature/Area2D").Connect("body_exited", this, "_on_Area2D_body_exited");

        _sprite.Animation = _params.name;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (_at_juni) { return; }
        var gp = GlobalPosition;
        var jgp = Juni.GlobalPosition;
        if (_params.horizontal && Mathf.Abs(jgp.y - (gp.y+12f)) > 12f)
        {
            tryStopAnim();
            return;
        }
        
        bool right = jgp.x > gp.x;
        if (FacingRight != right) { FacingRight = right; }

        if ((FacingRight ? _right_checker : _left_checker).GetOverlappingBodies().Count > 0)
        {
            tryStopAnim();
            return;
        }

        if (!_sprite.Playing) { _sprite.Play(_params.name); }

        Translate(new Vector2(_params.speed*delta*(FacingRight ? 1 : -1), 0f));
    }

    private void tryStopAnim()
    {
        if (_sprite.Playing)
        { 
            _sprite.Stop();
            _sprite.Frame = 0;
        }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (_params.deadly) { Juni.die(); }
        tryStopAnim();
        _at_juni = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        _at_juni = false;
    }
}
