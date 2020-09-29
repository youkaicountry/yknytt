using System;
using Godot;
using YKnyttLib;
using YUtil.Math;
using YUtil.Random;
using static YKnyttLib.JuniPowers;

public class Juni : KinematicBody2D
{
    [Export] public float jump_speed = -250f;
    [Export] public float gravity = 1125f;

    [Signal] public delegate void Jumped();
    [Signal] public delegate void PowerChanged();

    PackedScene hologram_scene;

    public Godot.Vector2 velocity = Godot.Vector2.Zero;
    public int dir = 0;
    public float max_speed;

    PackedScene double_jump_scene;

    public GDKnyttGame Game { get; private set; }
    public GDKnyttArea GDArea { get { return Game.CurrentArea; } }
    public Sprite Detector { get; private set; }
    public KnyttPoint AreaPosition { get { return GDArea.getPosition(GlobalPosition); } }

    public JuniPowers Powers { get; }

    public ClimbCheckers ClimbCheckers { get; private set; }
    public GroundChecker GroundChecker { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    public float just_climbed = 0f;
    public bool dead = false;
    public int just_reset = 0;

    public int jumps = 0;

    // Keys
    public bool LeftHeld { get { return Input.IsActionPressed("left"); } }
    public bool RightHeld { get { return Input.IsActionPressed("right"); } }
    public bool UpHeld { get { return Input.IsActionPressed("up"); } }
    public bool DownHeld { get { return Input.IsActionPressed("down"); } }
    public bool DownPressed { get { return Input.IsActionJustPressed("down"); } }
    public bool UmbrellaPressed { get { return Input.IsActionJustPressed("umbrella"); } }
    public bool HologramPressed { get { return Input.IsActionJustPressed("hologram"); } }
    public bool JumpEdge { get { return Input.IsActionJustPressed("jump"); } }
    public bool JumpHeld { get { return Input.IsActionPressed("jump"); } }
    public bool WalkHeld { get { return Input.IsActionPressed("walk"); } }

    public int JumpLimit { get { return Powers.getPower(PowerNames.DoubleJump) ? 2 : 1; } }
    public bool CanClimb { get { return Powers.getPower(PowerNames.Climb) && (FacingRight ? ClimbCheckers.RightColliding : ClimbCheckers.LeftColliding); } }
    public bool CanUmbrella { get { return Powers.getPower(PowerNames.Umbrella); } }
    //public bool Grounded { get { return IsOnFloor(); } }
    public bool Grounded { get { return GroundChecker.IsOnGround; } }
    public bool DidJump { get { return JumpEdge && Grounded && GroundChecker.CanJump; } }
    public bool FacingRight 
    {
        set { Sprite.FlipH = !value; Umbrella.FacingRight = value; } 
        get { return !Sprite.FlipH; } 
    }
    public bool DidAirJump { get { return JumpEdge && ((just_climbed > 0f) || (jumps < JumpLimit)); } }

    public Godot.Vector2 ApparentPosition { get { return (Hologram == null) ? GlobalPosition : Hologram.GlobalPosition; } }
    public bool CanDeployHologram { get {return ((CurrentState is IdleState)||(CurrentState is WalkRunState));} }
    public Node2D Hologram { get; private set; }

    public float organic_enemy_distance = float.MaxValue;

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
            if (RightHeld) { d = 1; FacingRight = true; }
            else if (LeftHeld) { d = -1; FacingRight = false; }
            return d;
        } 
    }

    public Sprite Sprite { get; private set; }
    public Umbrella Umbrella { get; private set; }
    public AnimationPlayer Anim { get; private set; }

    public Juni()
    {
        this.Powers = new JuniPowers();
        this.double_jump_scene = ResourceLoader.Load("res://knytt/juni/DoubleJump.tscn") as PackedScene;
    }

    public override void _Ready()
    {
        hologram_scene = ResourceLoader.Load("res://knytt/juni/Hologram.tscn") as PackedScene;
        Detector = GetNode<Sprite>("Detector");
        ClimbCheckers = GetNode<ClimbCheckers>("ClimbCheckers");
        GroundChecker = GetNode<GroundChecker>("GroundChecker");
        Sprite = GetNode<Sprite>("Sprite");
        Umbrella = GetNode<Umbrella>("Umbrella");
        Umbrella.reset();
        Anim = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
        transitionState(new IdleState(this));
    }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
        this.Powers.readFromSave(Game.GDWorld.KWorld.CurrentSave);
    }

    public void setPower(PowerNames name, bool value)
    {
        Powers.setPower(name, value);
        EmitSignal(nameof(PowerChanged), name, value);
    }

    public void setPower(int power, bool value)
    {
        Powers.setPower(power, value);
        EmitSignal(nameof(PowerChanged), power, value);
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

        // Organic Enemy Distance
        if (Powers.getPower(PowerNames.EnemyDetecor) && organic_enemy_distance <= 170f)
        {
            Detector.Visible = true;
            var m = Detector.Modulate;
            float max = 1f - (organic_enemy_distance / 170f);
            m.a = GDKnyttDataStore.random.NextFloat(.25f, max * .65f);
            Detector.Modulate = m;
        }
        else { Detector.Visible = false; }
        
        organic_enemy_distance = float.MaxValue;

        this.checkDebugInput(); // TODO: Check the mode for debug

        // Handle state transitions
        if (next_state != null) { executeStateTransition(); }

        this.CurrentState?.PreProcess(delta);

        // Handle umbrella
        if (CanUmbrella && UmbrellaPressed)
        {
            Umbrella.Deployed = !Umbrella.Deployed;
        }

        handleHologram();

        handleXMovement(delta);

        // This particular value needs to be multiplied by delta to ensure
        // framerate independence
        velocity.y += gravity * delta;
        if (JumpHeld) 
        {
            var jump_hold = Powers.getPower(PowerNames.HighJump) ? 550f : 125f;
            if (Umbrella.Deployed) { jump_hold *= .75f; }
            velocity.y -= jump_hold * delta;
        } 
        
        this.CurrentState.PostProcess(delta);

        // Pull-over-edge
        if (just_climbed > 0f)
        {
            velocity.x += (FacingRight ? 1f:-1f) * 30f;
            just_climbed -= delta;
            if (just_climbed < 0f) { jumps++; }
        }

        velocity.y = Mathf.Min(Umbrella.Deployed ? 60f : 350f, velocity.y);
        velocity = MoveAndSlide(velocity, Godot.Vector2.Up);
    }

    private void handleHologram()
    {
        if (!Powers.getPower(PowerNames.Hologram)) { return; }

        bool double_down = false;
        if (DownPressed)
        {
            var dtimer = GetNode<Timer>("DoubleDownTimer");
            if (dtimer.TimeLeft > 0f) { double_down = true; dtimer.Stop(); }
            else { dtimer.Start(); }
        }

        if (double_down || HologramPressed)
        {
            if (Hologram == null) { if (CanDeployHologram) { deployHologram(); } }
            else { stopHologram(); }
        }
    }

    private void deployHologram()
    {
        var timer = GetNode<Timer>("HologramTimer");
        if (timer.TimeLeft > 0f) { return; }
        timer.Start();
        GetNode<AudioStreamPlayer2D>("Audio/HoloDeployPlayer2D").Play();
        var node = hologram_scene.Instance() as Node2D;
        GDArea.GDWorld.Game.AddChild(node);
        node.GlobalPosition = GlobalPosition;
        node.GetNode<AnimatedSprite>("AnimatedSprite").FlipH = !FacingRight;
        Hologram = node;
        var m = Modulate; m.a = .45f; Modulate = m;
    }

    public void stopHologram(bool cleanup=false)
    {
        if (Hologram == null) { return; }

        var timer = GetNode<Timer>("HologramTimer");
        if (!cleanup)
        {
            if (timer.TimeLeft > 0f) { return; }
            timer.Start();
            GetNode<AudioStreamPlayer2D>("Audio/HoloStopPlayer2D").Play();
        }
        
        Hologram.QueueFree();
        Hologram = null;
        var m = Modulate; m.a = 1f; Modulate = m;
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
        Sprite.Visible = false;
        Umbrella.Visible = false;
        Detector.Visible = false;
        this.dead = true;
        var timer = GetNode<Timer>("RespawnTimer");
        timer.Start();
        await ToSignal(timer, "timeout");
        Game.respawnJuni();
    }

    public void win(string ending)
    {
        GetNode<JuniAudio>("Audio").stopAll();
        Anim.Stop();
        dead = true;
        Game.win(ending);
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

        Umbrella.reset();
        stopHologram(cleanup:true);
    }

    private void handleXMovement(float delta)
    {
        // Move, then clamp
        if (dir != 0)
        {
            var uspeed = Umbrella.Deployed ? (Mathf.Min(max_speed, 120f)) : max_speed;
            MathTools.MoveTowards(ref velocity.x, dir*uspeed, 2500f*delta);
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

    public void executeJump()
    {
        Anim.Play("Jump");
        GetNode<RawAudioPlayer2D>("Audio/JumpPlayer2D").Play();
        velocity.y = jump_speed;
        
        if (jumps > 0)
        {
            doubleJumpEffect();
        }
        
        jumps++;
        just_climbed = 0f;

        // Do not emit this event if the hologram is out, as it cannot jump
        if (Hologram == null) { EmitSignal(nameof(Jumped), this); }
    }

    public void continueFall()
    {
        Anim.Play("Falling");
    }

    public float manhattanDistance(Godot.Vector2 p, bool apparent=true)
    {
        var jp = apparent ? ApparentPosition : GlobalPosition;
        return Math.Abs(p.x-jp.x) + Math.Abs(p.y-jp.y);
    }

    public float distance(Godot.Vector2 p, bool apparent=true)
    {
        var jp = apparent ? ApparentPosition : GlobalPosition;
        return jp.DistanceTo(p);
    }

    public void updateOrganicEnemy(Godot.Vector2 p)
    {
        if (!Powers.getPower(PowerNames.EnemyDetecor)) { return; }
        var md = manhattanDistance(p, false);
        if (md < organic_enemy_distance) { organic_enemy_distance = md; }
    }
}
