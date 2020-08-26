using Godot;

public class Juni : KinematicBody2D
{
    [Export] public float speed = 90f;
    [Export] public float jump_speed = -220f;
    [Export] public float gravity = 1000f;

    public Vector2 velocity = Vector2.Zero;

    public GDKnyttGame Game { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left");  } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }

    public bool Grounded { get { return IsOnFloor(); } }

    public bool DidJump { get { return JumpEdge && Grounded; } } // TODO: This would check jumps since ground for double jump

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

    public override void _Ready()
    {
        this.Sprite = GetNode<Sprite>("Sprite");
        this.Anim = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
        transitionState(new IdleState(this));
    }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
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
        velocity.y += gravity * delta;
        velocity = MoveAndSlide(velocity, Vector2.Up);
        this.CurrentState.PostProcess(delta);

        // TODO: Do this only if the local player
        if (!this.Game.CurrentArea.isIn(this.GlobalPosition)) 
        {
            this.Game.changeArea(GDKnyttWorld.getAreaCoords(this.GlobalPosition));
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
