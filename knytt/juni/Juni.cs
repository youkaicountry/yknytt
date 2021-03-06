using Godot;
using System;
using YKnyttLib;
using YUtil.Math;
using YUtil.Random;
using static YKnyttLib.JuniValues;

public class Juni : KinematicBody2D
{
    internal const float JUMP_SPEED = -250f,    // Speed of jump
    GRAVITY = 1125f,                            // Gravity exerted on Juni
    JUST_CLIMBED_TIME = .085f,                  // Time after a jump considered (just jumped)
    FREE_JUMP_TIME = .085f,                     // Amount of time after leaving a wall that Juni gets a "free" jump
    MAX_SPEED_WALK = 90f,                       // Max speed while walking
    MAX_SPEED_RUN = 175f,                       // Max speed while running
    PULL_OVER_FORCE = 30f,                      // X Force exerted when reaching the top of a climb
    SLOPE_MAX_ANGLE = .81f,                     // The Maximum angle a floor can be before becoming a wall (TODO: This number is made up, research required)
    UPDRAFT_FORCE = .15f,                       // The base updraft force exerted
    UPDRAFT_FORCE_HOLD = .3f,                   // The updraft force exterted when holding jump
    MAX_UPDRAFT_SPEED = -225f,                  // Maximum Y speed in an updraft
    MAX_UPDRAFT_SPEED_HOLD = -240f,             // Maximum Y speed in an updraft while holding jump
    JUMP_HOLD_POWER = 125f,                     // Y Force exerted while holding jump
    HIGH_JUMP_HOLD_POWER = 550f,                // Y Force exerted while holding jump when Juni has high jump power
    UMBRELLA_JUMP_HOLD_PENALTY = .82f,          // Penalty on jump hold when Juni has the umbrella deployed
    MAX_X_MOVING_DELTA = 2500f,                 // Maximum rate of change of X velocity when moving
    MAX_X_DECAY_DELTA = 1500f,                  // Maximum rate of change of X velocity when stopped
    MAX_X_SPEED_UMBRELLA = 120f,                // Maximum X speed when Juni has the umbrella deployed
    TERM_VEL = 350f,                            // Maximum +Y velocity
    TERM_VEL_UMB = 60f,                         // Maximum +Y velocity when Juni has the umbrella deployed
    TERM_VEL_UP = 20f,                          // Maximum +Y velocity when Juni has the umbrella deployed in an updraft
    CLIMB_SPEED = -125f,                        // Speed Juni climbs up a wall
    SLIDE_SPEED = 25f,                          // Speed Juni slides down a wall
    CLIMB_JUMP_X_SPEED = 130f,                  // Speed Juni jumps away from a wall
    INSIDE_X_SPEED = -22f,                      // Speed at which Juni moves along the x-axis when stuck inside walls
    INSIDE_Y_SPEED = -10f,                      // Speed at which Juni moves along the y-axis when stuck inside walls
    DEBUG_FLY_SPEED = 300f;                     // Speed at which Juni flies while in debug fly mode

    [Signal] public delegate void Jumped();
    [Signal] public delegate void PowerChanged();
    [Signal] public delegate void HologramStopped(Juni juni);
    [Signal] public delegate void DownEvent(Juni juni);

    PackedScene hologram_scene;

    public Godot.Vector2 velocity = Godot.Vector2.Zero;
    public int dir = 0;

    //const float BaseHeightCorrection = 3.4f;
    public Godot.Vector2 BaseCorrection
    {
        get
        {
            //var rect = GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D;
            //return new Godot.Vector2(0f, 12f - rect.Extents.y);
            return new Godot.Vector2(0f, 3.4f);
        }
    }

    PackedScene double_jump_scene;

    public GDKnyttGame Game { get; private set; }
    public GDKnyttArea GDArea { get { return Game.CurrentArea; } }
    public Sprite Detector { get; private set; }
    public KnyttPoint AreaPosition { get { return GDArea.getPosition(GlobalPosition); } }

    public JuniValues Powers { get; }

    public InsideDetector InsideDetector { get; private set; }
    public ClimbCheckers ClimbCheckers { get; private set; }
    public GroundChecker GroundChecker { get; private set; }

    public JuniMotionParticles MotionParticles { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    public float air_time = 0f;

    public bool dead = false;
    public int just_reset = 0;

    public int jumps = 0;

    const float START_SLOPE_TIME = .02f;
    int start_slope_frames;
    int frames_on_slope = 0;

    // Input
    public JuniInput juniInput;

    float _just_climbed = 0f;
    public bool JustClimbed
    {
        get { return _just_climbed > 0f; }
        set { _just_climbed = value ? JUST_CLIMBED_TIME : 0f; }
    }

    float _can_free_jump = 0f;
    public bool CanFreeJump
    {
        get { return _can_free_jump > 0f; }
        set { _can_free_jump = value ? FREE_JUMP_TIME : 0f; }
    }

    public Godot.Vector2 Bottom
    {
        get
        {
            var gp = GlobalPosition;
            return new Godot.Vector2(gp.x, gp.y + 8.6f);
        }
    }

    int _updrafts = 0;
    public bool InUpdraft
    {
        get { return _updrafts > 0; }
        set { _updrafts += (value ? 1 : -1); }
    }

    public Color? JuniClothes
    {
        set
        {
            var mat = GetNode<Sprite>("Sprite").Material as ShaderMaterial;
            mat.SetShaderParam("clothes_color", value);
        }

        get
        {
            var mat = GetNode<Sprite>("Sprite").Material as ShaderMaterial;
            return mat.GetShaderParam("clothes_color") as Color?;
        }
    }

    public Color? JuniSkin
    {
        set
        {
            var mat = GetNode<Sprite>("Sprite").Material as ShaderMaterial;
            mat.SetShaderParam("skin_color", value);
        }

        get
        {
            var mat = GetNode<Sprite>("Sprite").Material as ShaderMaterial;
            return mat.GetShaderParam("skin_color") as Color?;
        }
    }

    public float TerminalVelocity { get { return Umbrella.Deployed ? (InUpdraft ? TERM_VEL_UP : TERM_VEL_UMB) : TERM_VEL; } }

    public int JumpLimit { get { return Powers.getPower(PowerNames.DoubleJump) ? 2 : 1; } }
    public bool CanClimb { get { return Powers.getPower(PowerNames.Climb) && (FacingRight ? ClimbCheckers.RightColliding : ClimbCheckers.LeftColliding); } }
    public bool CanUmbrella { get { return Powers.getPower(PowerNames.Umbrella); } }
    //public bool Grounded { get { return IsOnFloor(); } }
    public bool Grounded { get { return GroundChecker.IsOnGround; } }
    public bool DidJump { get { return juniInput.JumpEdge && Grounded && CanJump; } }
    public bool FacingRight
    {
        set { Sprite.FlipH = !value; Umbrella.FacingRight = value; }
        get { return !Sprite.FlipH; }
    }
    //public bool DidAirJump { get { return JumpEdge && (CanFreeJump || (jumps < JumpLimit)); } }
    public bool DidAirJump { get { return juniInput.JumpEdge && (CanFreeJump || (jumps < JumpLimit)); } }

    // Whether or no Juni is in a NoJump situation
    int no_jumps = 0; // Number of no jump zones conditions covering Juni
    public bool CanJump
    {
        get { return no_jumps == 0; }
        set { no_jumps += (value ? -1 : 1); }
    }

    // Whether or no Juni is in a sticky area
    int stickies = 0; // Number of sticky zones covering Juni
    public bool Sticky
    {
        get { return stickies > 0; }
        set { stickies += (value ? 1 : -1); }
    }

    public Godot.Vector2 ApparentPosition { get { return (Hologram == null) ? GlobalPosition : Hologram.GlobalPosition; } }
    public bool CanDeployHologram { get { return ((CurrentState is IdleState) || (CurrentState is WalkRunState)); } }
    public Node2D Hologram { get; private set; }

    public float detector_reverse_distance = 0;
    public Color detector_color = new Color(0, 0, 0, 0);

    public bool WalkRun
    {
        get
        {
            if (!Powers.getPower(PowerNames.Run)) { return false; }
            return !juniInput.WalkHeld;
        }
    }

    public float MaxSpeed
    {
        get
        {
            return WalkRun ? MAX_SPEED_RUN : MAX_SPEED_WALK;
        }
    }

    public int MoveDirection
    {
        get
        {
            var d = 0;
            if (Sticky) { return 0; }
            if (juniInput.RightHeld) { d = 1; FacingRight = true; }
            else if (juniInput.LeftHeld) { d = -1; FacingRight = false; }
            return d;
        }
    }

    private bool _immune;
    private bool Immune
    {
        get { return _immune; }
        set
        {
            _immune = value;
            CollisionLayer = value ? (uint)0 : 1;
            CollisionMask = value ? 2147483654 : 2147491846;
        }
    }

    public Sprite Sprite { get; private set; }
    public Umbrella Umbrella { get; private set; }
    public AnimationPlayer Anim { get; private set; }

    bool _debug_fly = false;
    public bool DebugFlyMode
    {
        get { return _debug_fly; }
        set 
        { 
            _debug_fly = value;
            CollisionsDisabled = value;
            velocity = new Godot.Vector2();
            if (value) 
            { 
                transitionState(new IdleState(this)); 
                executeStateTransition(); 
            }
        }
    }

    bool _collisions_disabled = false;
    public bool CollisionsDisabled
    {
        get { return _collisions_disabled; }
        set
        {
            if (DebugFlyMode && !value) { return; }
            _collisions_disabled = value;
            GetNode<CollisionShape2D>("InsideDetector/CollisionShape2D").Disabled = value;
            GetNode<GroundChecker>("GroundChecker").Disabled = value;
            GetNode<ClimbCheckers>("ClimbCheckers").Disabled = value;
            enforceCollisionMap();
        }
    }

    // Toggle the collision shapes
    CollisionPolygon2D[] _collision_polygons = {null, null, null};
    bool[] _collision_map = { true, true, true };
    public void setCollisionMap(bool a, bool b, bool c)
    {
        _collision_map[0] = a;
        _collision_map[1] = b;
        _collision_map[2] = c;

        enforceCollisionMap();
    }

    private void enforceCollisionMap()
    {
        if (CollisionsDisabled)
        {
            _collision_polygons[0].Disabled = true;
            _collision_polygons[1].Disabled = true;
            _collision_polygons[2].Disabled = true;
        }
        else
        {
            _collision_polygons[0].Disabled = !_collision_map[0];
            _collision_polygons[1].Disabled = !_collision_map[1];
            _collision_polygons[2].Disabled = !_collision_map[2];
        }
    }

    public Juni()
    {
        juniInput = new JuniInput(this);
        this.Powers = new JuniValues();
        this.double_jump_scene = ResourceLoader.Load("res://knytt/juni/DoubleJump.tscn") as PackedScene;

        this.start_slope_frames = Mathf.FloorToInt(START_SLOPE_TIME / (1f / ((int)ProjectSettings.GetSetting("physics/common/physics_fps"))));
    }

    public override void _Ready()
    {
        _collision_polygons = new CollisionPolygon2D[] { GetNode<CollisionPolygon2D>("CollisionPolygonA"),
                                                         GetNode<CollisionPolygon2D>("CollisionPolygonB"),
                                                         GetNode<CollisionPolygon2D>("CollisionPolygonC") };
        hologram_scene = ResourceLoader.Load("res://knytt/juni/Hologram.tscn") as PackedScene;
        MotionParticles = GetNode<JuniMotionParticles>("JuniMotionParticles");
        Detector = GetNode<Sprite>("Detector");
        Detector.Visible = true;
        InsideDetector = GetNode<InsideDetector>("InsideDetector");
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
        enableAttachment(this.Powers.Attachment);
        GetNode<StandartSoundPlayer>("Audio/StandartSoundPlayer").KWorld = game.GDWorld.KWorld;
        JuniClothes = new Color(KnyttUtil.BGRToRGBA(Game.GDWorld.KWorld.Info.Clothes));
        JuniSkin = new Color(KnyttUtil.BGRToRGBA(Game.GDWorld.KWorld.Info.Skin));
    }

    public void setPower(PowerNames name, bool value)
    {
        if (name == PowerNames.Umbrella && !value && Umbrella.Deployed) { Umbrella.Deployed = false; }
        if (name == PowerNames.Hologram && !value && Hologram != null) { stopHologram(); }
        Powers.setPower(name, value);
        EmitSignal(nameof(PowerChanged), name, value);
    }

    public void setPower(int power, bool value)
    {
        setPower((PowerNames)power, value);
    }

    public void updateCollectables()
    {
        EmitSignal(nameof(PowerChanged), -1, true);
    }

    public void transitionState(JuniState state)
    {
        this.next_state = state;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (dead) { return; }

        juniInput.Update();

        this.checkDebugInput(); // TODO: Check the mode for debug

        if (DebugFlyMode) { processFlyMode(delta); } else { processMotion(delta); }

        if (GDArea.HasAltInput) { juniInput.FinishFrame(); }
    }

    private void checkDebugInput()
    {
        if (Input.IsActionJustPressed("debug_die")) { die(); }
        if (Input.IsActionJustPressed("debug_save")) { Game.saveGame(this, true); }
        if (Input.IsActionJustPressed("debug_iddqd")) { Immune = !Immune; }
        if (Input.IsActionJustPressed("debug_ui")) { Game.UI.Location.toggle(); }
        if (Input.IsActionJustPressed("debug_idclip")) { DebugFlyMode = !DebugFlyMode; }
    }

    public void processMotion(float delta)
    {
        if (just_reset > 0)
        {
            just_reset--;
            if (just_reset == 0) { SetDeferred("CollisionsDisabled", false); }
        }

        if (juniInput.DownPressed) { EmitSignal(nameof(DownEvent), this); }

        // Organic Enemy Distance
        if (detector_reverse_distance > 0)
        {
            Detector.Visible = true;
            var m = detector_color;
            m.a = GDKnyttDataStore.random.NextFloat(.25f, detector_reverse_distance * .65f);
            Detector.Modulate = m;
        }
        else { Detector.Visible = false; }

        detector_reverse_distance = 0;

        // Handle state transitions
        if (next_state != null) { executeStateTransition(); }

        this.CurrentState?.PreProcess(delta);

        // Handle umbrella
        if (CanUmbrella && juniInput.UmbrellaPressed)
        {
            Umbrella.Deployed = !Umbrella.Deployed;
        }

        handleHologram();
        handleXMovement(delta);
        handleGravity(delta);

        this.CurrentState.PostProcess(delta);

        // Pull-over-edge
        if (JustClimbed)
        {
            velocity.x += (FacingRight ? 1f : -1f) * PULL_OVER_FORCE;
            _just_climbed -= delta;
            if (_can_free_jump <= 0f) { JustClimbed = false; }
        }

        // Can-free-jump
        if (CanFreeJump)
        {
            _can_free_jump -= delta;
            if (_can_free_jump <= 0f) { jumps++; CanFreeJump = false; }
        }

        velocity.y = Mathf.Min(TerminalVelocity, velocity.y);

        // Slope check
        // This helps keeps Juni from entering slope mode when jumping onto platforms
        // TODO: Move this frame independence calculation to a utility function somewhere
        if (Mathf.Abs(GetFloorNormal().x) > .00001f && !juniInput.JumpEdge) { frames_on_slope += 1; }
        else { frames_on_slope = 0; }

        if (frames_on_slope > start_slope_frames) { handleSlope(); }
        else if (InsideDetector.IsInside) { Translate(new Godot.Vector2(INSIDE_X_SPEED * MoveDirection * delta, INSIDE_Y_SPEED * delta)); }
        else
        {
            velocity.x = MoveAndSlide(new Godot.Vector2(velocity.x, 0), Godot.Vector2.Up, stopOnSlope: false, floorMaxAngle: SLOPE_MAX_ANGLE).x;
            velocity.y = MoveAndSlide(new Godot.Vector2(0, velocity.y), Godot.Vector2.Up, stopOnSlope: true, floorMaxAngle: SLOPE_MAX_ANGLE).y;
        }

        if (GetSlideCount() > 0 && GetSlideCollision(0).Collider is BaseBullet) { die(); }
    }

    private void processFlyMode(float delta)
    {
        var dir = new Godot.Vector2();
        if (juniInput.UpHeld) { dir.y -= 1f; }
        if (juniInput.DownHeld) { dir.y += 1f; }
        if (juniInput.LeftHeld) { dir.x -= 1f; }
        if (juniInput.RightHeld) { dir.x += 1f; }

        Translate(dir.Normalized() * DEBUG_FLY_SPEED * delta);
    }

    private void handleSlope()
    {
        // Rotate the x component of the velocity to the perpendicular of the floor normal
        var x_move = -(GetFloorNormal().Perpendicular().Normalized()) * velocity.x;

        // Isolate the unrotated y component
        var y_move = -GetFloorNormal();

        x_move = MoveAndSlideWithSnap(x_move, Godot.Vector2.Down, Godot.Vector2.Up, stopOnSlope: false, maxSlides: 2, floorMaxAngle: SLOPE_MAX_ANGLE);

        y_move = MoveAndSlide(y_move, Godot.Vector2.Up, stopOnSlope: true, maxSlides: 2, floorMaxAngle: SLOPE_MAX_ANGLE);

        // Unrotate the x component and set it back to the velocity
        // Currently disabled due to it exaggerating imperfections in the slope
        velocity.y = y_move.Length();
    }

    private void handleGravity(float delta)
    {
        // This particular value needs to be multiplied by delta to ensure
        // framerate independence
        if (InUpdraft && Umbrella.Deployed)
        {
            velocity.y -= GRAVITY * delta * (juniInput.JumpHeld ? UPDRAFT_FORCE_HOLD : UPDRAFT_FORCE);
            velocity.y = Mathf.Max(juniInput.JumpHeld ? MAX_UPDRAFT_SPEED_HOLD : MAX_UPDRAFT_SPEED, velocity.y);
        }
        else
        {
            if (!Grounded) { velocity.y += GRAVITY * delta; }
            else { velocity.y = GRAVITY * delta; }
            if (juniInput.JumpHeld)
            {
                var jump_hold = Powers.getPower(PowerNames.HighJump) ? HIGH_JUMP_HOLD_POWER : JUMP_HOLD_POWER;
                if (Umbrella.Deployed) { jump_hold *= UMBRELLA_JUMP_HOLD_PENALTY; }
                velocity.y -= jump_hold * delta;
            }
        }
    }

    private void handleHologram()
    {
        if (!Powers.getPower(PowerNames.Hologram)) { return; }

        bool double_down = false;
        if (juniInput.DownPressed)
        {
            var dtimer = GetNode<Timer>("DoubleDownTimer");
            if (dtimer.TimeLeft > 0f) { double_down = true; dtimer.Stop(); }
            else { dtimer.Start(); }
        }

        if (double_down || juniInput.HologramPressed)
        {
            if (Hologram == null)
            {
                if (CanDeployHologram) { deployHologram(); }
            }
            else
            {
                stopHologram();
                EmitSignal(nameof(HologramStopped), this);
            }
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
        GetNode<Sprite>("AttachmentSprite").Modulate = new Color(1, 1, 1, 1 / m.a);
    }

    public void stopHologram(bool cleanup = false)
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
        GetNode<Sprite>("AttachmentSprite").Modulate = new Color(1, 1, 1, 1);
    }

    public void doubleJumpEffect()
    {
        var dj_node = double_jump_scene.Instance() as Node2D;
        dj_node.GlobalPosition = GlobalPosition;
        GetParent().AddChild(dj_node);
    }

    public void playSound(string sound)
    {
        GetNode<StandartSoundPlayer>("Audio/StandartSoundPlayer").playSound(sound);
    }

    public void enableAttachment(string attachment)
    {
        var torch_sprite = GetNode<Sprite>("AttachmentSprite");
        switch (attachment?.ToLower())
        {
            case "true":
            case "":
                torch_sprite.Texture = GDKnyttAssetManager.loadInternalTexture("res://knytt/juni/Attach.png");
                torch_sprite.Visible = true;
                Powers.Attachment = "true";
                break;
            case "false":
            case null:
                torch_sprite.Visible = false;
                Powers.Attachment = "false";
                break;
            default:
                torch_sprite.Texture = Game.GDWorld.KWorld.getWorldTexture($"Custom Objects/{attachment}") as Texture;
                torch_sprite.Visible = torch_sprite.Texture != null;
                Powers.Attachment = attachment;
                break;
        }
    }

    // This kills the Juni
    public void die()
    {
        CallDeferred("_die");
    }

    private async void _die()
    {
        if (dead || Immune) { return; }
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
        Game.respawnJuniWithWSOD();
    }

    public void moveToPosition(GDKnyttArea area, KnyttPoint position)
    {
        GlobalPosition = (area.getTileLocation(position) + (velocity.y < 0 ? -1 : 1) * BaseCorrection);
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

        SetDeferred("CollisionsDisabled", true);
        //GetNode<CollisionPolygon2D>("CollisionPolygon2D").SetDeferred("disabled", true);
        this.just_reset = 2;

        dir = 0;
        jumps = 0;
        JustClimbed = false;
        CanFreeJump = false;

        Umbrella.reset();
        stopHologram(cleanup: true);
        enableAttachment(Powers.Attachment);
    }

    private void handleXMovement(float delta)
    {
        // Move, then clamp
        if (dir != 0)
        {
            var uspeed = Umbrella.Deployed ? (Mathf.Min(MaxSpeed, MAX_X_SPEED_UMBRELLA)) : MaxSpeed;
            MathTools.MoveTowards(ref velocity.x, dir * uspeed, MAX_X_MOVING_DELTA * delta);
        }
        else
        {
            MathTools.MoveTowards(ref velocity.x, 0f, MAX_X_DECAY_DELTA * delta); // deceleration
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

    public void executeJump(float jump_speed, bool air_jump = false, bool sound = true, bool reset_jumps = false)
    {
        transitionState(new JumpState(this));
        Anim.Play("Jump");
        if (sound) { GetNode<RawAudioPlayer2D>("Audio/JumpPlayer2D").Play(); }
        velocity.y = jump_speed;

        if (air_jump && jumps > 0) { doubleJumpEffect(); }

        jumps = reset_jumps ? 1 : jumps + 1;
        JustClimbed = false;
        CanFreeJump = false;

        // Do not emit this event if the hologram is out, as it cannot jump
        if (Hologram == null) { EmitSignal(nameof(Jumped), this); }
    }

    public void executeJump(bool air_jump = false, bool sound = true, bool reset_jumps = false)
    {
        executeJump(JUMP_SPEED, air_jump, sound, reset_jumps);
    }

    public void continueFall()
    {
        Anim.Play("Falling");
    }

    public float manhattanDistance(Godot.Vector2 p, bool apparent = true)
    {
        var jp = apparent ? ApparentPosition : GlobalPosition;
        return Math.Abs(p.x - jp.x) + Math.Abs(p.y - jp.y);
    }

    public float distance(Godot.Vector2 p, bool apparent = true)
    {
        var jp = apparent ? ApparentPosition : GlobalPosition;
        return jp.DistanceTo(p);
    }

    public float distanceSquared(Godot.Vector2 p, bool apparent = true)
    {
        var jp = apparent ? ApparentPosition : GlobalPosition;
        return jp.DistanceSquaredTo(p);
    }

    public void updateOrganicEnemy(float rev_distance, Color color)
    {
        if (rev_distance > detector_reverse_distance)
        {
            detector_reverse_distance = rev_distance;
            detector_color = color;
        }
    }
}
