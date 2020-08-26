using Godot;

public class Juni : KinematicBody2D
{
    [Export] public float speed = 150f;
    [Export] public float jump_speed = -300f;
    [Export] public float gravity = 1000f;

    public Vector2 velocity = Vector2.Zero;

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left");  } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }

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

    public void transitionState(JuniState state)
    {
        this.next_state = state;
    }

    public void getInput()
    {
        //velocity.x = 0f;
        //if (Input.IsActionPressed("right")) { velocity.x += speed; }
        //if (Input.IsActionPressed("left")) { velocity.x -= speed; }

    }

    public override void _PhysicsProcess(float delta)
    {
        // Handle state transitions
        if (next_state != null) { executeStateTransition(); }

        getInput();
        this.CurrentState?.PreProcess(delta);
        velocity.y += gravity * delta;
        velocity = MoveAndSlide(velocity, Vector2.Up);
        this.CurrentState.PreProcess(delta);
        //if (Input.IsActionJustPressed("jump") && IsOnFloor()) { velocity.y = jump_speed; }
    }

    private void executeStateTransition()
    {
        if (this.CurrentState != null) { this.CurrentState.onExit(); }
        this.CurrentState = next_state;
        this.CurrentState.onEnter();
        this.next_state = null;
    }
}
