using Godot;
using YKnyttLib;
using static YKnyttLib.JuniPowers;

public class Juni : KinematicBody2D
{
    //[Export] public float speed = 90f;
    [Export] public float jump_speed = -230f;
    [Export] public float gravity = 950f;

    public Vector2 velocity = Vector2.Zero;
    public int dir = 0;
    public float max_speed;

    public GDKnyttGame Game { get; private set; }

    public JuniPowers Powers { get; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left");  } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }
    public bool WalkHeld { get { return Input.IsActionPressed("walk"); } }

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

    public Sprite Sprite { get; private set; }
    public AnimationPlayer Anim { get; private set; }

    public Juni()
    {
        this.Powers = new JuniPowers();
    }

    public override void _Ready()
    {
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

        velocity.y += gravity * delta;
        velocity = MoveAndSlide(velocity, Vector2.Up);
        this.CurrentState.PostProcess(delta);

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
            velocity.x += dir * 2500f * delta;
            velocity.x = Mathf.Min(velocity.x, max_speed);
            velocity.x = Mathf.Max(velocity.x, -max_speed);
        }
        else
        {
            YUtil.Math.MathTools.MoveTowards(ref velocity.x, 0f, 2500f * delta );
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
