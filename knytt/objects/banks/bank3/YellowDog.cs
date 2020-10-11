using Godot;

public class YellowDog : GDKnyttBaseObject
{
    bool _at_juni = false;
    AnimatedSprite _sprite;
    Area2D _right_checker;
    Area2D _left_checker;
    bool FacingRight { get { return !_sprite.FlipH; } set { _sprite.FlipH = !value; } }

    public const float SPEED = 40f;

    public override void _Ready()
    {
        base._Ready();
        _sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _right_checker = GetNode<Area2D>("RightChecker");
        _left_checker = GetNode<Area2D>("LeftChecker");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (_at_juni) { return; }
        var gp = GlobalPosition;
        var jgp = Juni.GlobalPosition;
        if (Mathf.Abs(jgp.y - (gp.y+12f)) > 12f) 
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

        if (!_sprite.Playing) { _sprite.Play("YellowDog"); }

        Translate(new Vector2(SPEED*delta*(FacingRight ? 1 : -1), 0f));
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
        tryStopAnim();
        _at_juni = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        _at_juni = false;
    }
}
