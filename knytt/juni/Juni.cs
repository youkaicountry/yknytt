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

    PackedScene double_jump_scene;

    public GDKnyttGame Game { get; private set; }
    public GDKnyttArea GDArea { get { return Game.CurrentArea; } }
    public KnyttPoint AreaPosition { get { return GDArea.getPosition(GlobalPosition); } }

    public JuniPowers Powers { get; }

    public ClimbCheckers ClimbCheckers { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    public float just_climbed = 0f;
    public bool dead = false;
    public int just_reset = 0;

    public int jumps = 0;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left");  } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool UpHeld { get { return Input.IsActionPressed("up"); } }
    public bool DownHeld { get { return Input.IsActionPressed("down"); } }
    public bool DownPressed { get { return Input.IsActionJustPressed("down"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }
    public bool JumpHeld { get { return Input.IsActionPressed("jump"); } }
    public bool WalkHeld { get { return Input.IsActionPressed("walk"); } }

    public int JumpLimit { get { return Powers.getPower(PowerNames.DoubleJump) ? 2 : 1; } }
    public bool CanClimb { get { return Powers.getPower(PowerNames.Climb) && (FacingRight ? ClimbCheckers.RightColliding : ClimbCheckers.LeftColliding); } }
    public bool Grounded { get { return IsOnFloor(); } }
    public bool DidJump { get { return JumpEdge && Grounded; } } // TODO: This would check jumps since ground for double jump
    public bool FacingRight { get { return !Sprite.FlipH; } }
    public bool DidAirJump { get { return JumpEdge && ((just_climbed > 0f) || (jumps < JumpLimit)); } }

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
        this.double_jump_scene = ResourceLoader.Load("res://knytt/juni/DoubleJump.tscn") as PackedScene;
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

    private void checkDebugInput()
    {
        if (Input.IsActionJustPressed("debug_die")) { die(); }
        if (Input.IsActionJustPressed("debug_save")) { Game.saveGame(this, true); }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (dead) { return; }

        if (just_reset > 0) 
        {
            just_reset--; 
            if (just_reset == 0) { GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false; }
        }

        this.checkDebugInput(); // TODO: Check the mode for debug

        // Handle state transitions
        if (next_state != null) { executeStateTransition(); }

        this.CurrentState?.PreProcess(delta);

        handleXMovement(delta);

        // This particular value needs to be multiplied by delta to ensure
        // framerate independence
        velocity.y += gravity * delta;
        if (JumpHeld) 
        {
            velocity.y -= ( Powers.getPower(PowerNames.HighJump) ? 550 : 125) * delta;
        } 
        
        this.CurrentState.PostProcess(delta);

        // Pull-over-edge
        if (just_climbed > 0f)
        {
            velocity.x += (FacingRight ? 1f:-1f) * 30f;
            just_climbed -= delta;
            if (just_climbed < 0f) { jumps++; }
        }

        velocity.y = Mathf.Min(350f, velocity.y);
        velocity = MoveAndSlide(velocity, Godot.Vector2.Up);

        // TODO: Do this only if the local player
        if (!this.Game.CurrentArea.isIn(this.GlobalPosition)) 
        {
            this.Game.changeArea(GDKnyttWorld.getAreaCoords(this.GlobalPosition));
        }
    }

    public void doubleJumpEffect()
    {
        var dj_node = double_jump_scene.Instance() as Node2D;
        dj_node.GlobalPosition = GlobalPosition;
        GetParent().AddChild(dj_node);
    }

    // This kills the Juni
    public async void die()
    {
        if (dead) { return; }
        GetNode<DeathParticles>("DeathParticles").Play();
        GetNode<AudioStreamPlayer2D>("Audio/DiePlayer2D").Play();
        this.next_state = null;
        this.clearState();
        GetNode<Sprite>("Sprite").Visible = false;
        this.dead = true;
        var timer = GetNode<Timer>("RespawnTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        Game.respawnJuni();
    }

    public void reset()
    {
        Sprite.FlipH = false;
        GetNode<Sprite>("Sprite").Visible = true;
        this.dead = false;
        this.velocity = Godot.Vector2.Zero;
        this.transitionState(new IdleState(this));

        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        this.just_reset = 2;
    }

    private void handleXMovement(float delta)
    {
        // Move, then clamp
        if (dir != 0)
        {
            MathTools.MoveTowards(ref velocity.x, dir*max_speed, 2500f*delta);
        }
        else
        {
            MathTools.MoveTowards(ref velocity.x, 0f, 1500f*delta ); // deceleration
        }
    }

    private void executeStateTransition()
    {
        this.clearState();
        this.CurrentState = next_state;
        this.CurrentState.onEnter();
        this.next_state = null;
    }

    private void clearState()
    {
        if (this.CurrentState != null) { this.CurrentState.onExit(); }
    }

    public void continueFall()
    {
        Anim.Play("Falling");
    }
}
