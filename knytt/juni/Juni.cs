using Godot;
using YKnyttLib;
using YUtil.Math;
using static YKnyttLib.JuniPowers;

public class Juni : KinematicBody2D
{
    [Export] public float jump_speed = -250f;
    [Export] public float gravity = 1125f;

    public Godot.Vector2 velocity = Godot.Vector2.Zero;
    public int dir = 0;
    public float max_speed;

    public GDKnyttGame Game { get; private set; }

    public JuniPowers Powers { get; }

    public ClimbCheckers ClimbCheckers { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    public float just_climbed = 0f;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left");  } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool UpHeld { get { return Input.IsActionPressed("up"); } }
    public bool DownHeld { get { return Input.IsActionPressed("down"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }
    public bool JumpHeld { get { return Input.IsActionPressed("jump"); } }
    public bool WalkHeld { get { return Input.IsActionPressed("walk"); } }

    public bool CanClimb { get { return Powers.getPower(PowerNames.Climb) && (FacingRight ? ClimbCheckers.RightColliding : ClimbCheckers.LeftColliding); } }

    public bool Grounded { get { return IsOnFloor(); } }

    public bool DidJump { get { return JumpEdge && Grounded; } } // TODO: This would check jumps since ground for double jump

    public bool WalkRun 
    { 
        get 
        { 
            if (!Powers.getPower(PowerNames.Run)) { return false; }
            return !WalkHeld;
        } 
    }

    public int MoveDirection 
    { 
        get
        {  
            var d = 0;
            if (RightHeld) { d = 1; Sprite.FlipH = false; }
            else if (LeftHeld) { d = -1; Sprite.FlipH = true; }
            return d;
        } 
    }

    public bool FacingRight { get { return !Sprite.FlipH; } }

    public Sprite Sprite { get; private set; }
    public AnimationPlayer Anim { get; private set; }

    public Juni()
    {
        this.Powers = new JuniPowers();
    }

    public override void _Ready()
    {
        this.ClimbCheckers = GetNode<ClimbCheckers>("ClimbCheckers");
        this.Sprite = GetNode<Sprite>("Sprite");
        this.Anim = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
        transitionState(new IdleState(this));
    }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
        this.Powers.readFromSave(Game.GDWorld.KWorld.CurrentSave);
    }

    public void transitionState(JuniState state)
    {
        this.next_state = state;
    }

    public override void _PhysicsProcess(float delta)
    {
        // Handle state transitions
        if (next_state != null) { executeStateTransition(); }

        this.CurrentState?.PreProcess(delta);

        handleXMovement(delta);

        // This particular value needs to be multiplied byt delta to ensure
        // framerate independence
        velocity.y += gravity * delta;
        if (JumpHeld) {velocity.y -= 125 * delta;} // jump hold
        
        this.CurrentState.PostProcess(delta);

        // Pull-over-edge
        if (just_climbed > 0f)
        {
            velocity.x += (FacingRight ? 1f:-1f) * 30f;
            just_climbed -= delta;
        }

        velocity.y = Mathf.Min(350f, velocity.y);
        velocity = MoveAndSlide(velocity, Godot.Vector2.Up);

        // TODO: Do this only if the local player
        if (!this.Game.CurrentArea.isIn(this.GlobalPosition)) 
        {
            this.Game.changeArea(GDKnyttWorld.getAreaCoords(this.GlobalPosition));
        }
    }

    private void handleXMovement(float delta)
    {
        // Move, then clamp
        if (dir != 0)
        {
            YUtil.Math.MathTools.MoveTowards(ref velocity.x, dir*max_speed, 2500f*delta);
        }
        else
        {
            YUtil.Math.MathTools.MoveTowards(ref velocity.x, 0f, 1500f*delta ); // deceleration
        }
    }

    private void executeStateTransition()
    {
        if (this.CurrentState != null) { this.CurrentState.onExit(); }
        this.CurrentState = next_state;
        this.CurrentState.onEnter();
        this.next_state = null;
    }

    public void continueFall()
    {
        Anim.Play("Falling");
    }
}
