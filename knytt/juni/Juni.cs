using Godot;
using System;
using YKnyttLib;
using YKnyttLib.Logging;
using YUtil.Math;
using YUtil.Random;
using static YKnyttLib.JuniValues;

public class Juni : KinematicBody2D
{
    /*[Export] public*/internal const float JUMP_SPEED_HIGH = -240f,    // Speed of jump with high jump power (-238.5 in original)
    JUMP_SPEED_LOW = -235.5f,                   // Speed of jump with no high jump power (-230 in original)
    JUMP_SPEED_UMBRELLA = -221f,                // Speed of jump with umbrella (-220 in original)
    GRAVITY = 1125f,                            // Gravity exerted on Juni
    LOW_JUMP_HOLD_POWER = 130f,                 // Y Force exerted while holding jump (125 in original)
    HIGH_JUMP_HOLD_POWER = 555f,                // Y Force exerted while holding jump when Juni has high jump power (550 in original)
    LOW_JUMP_EXTRA_GRAVITY = 100f,              // Y Force added to gravity when Juni has no high jump power (not from the original)
    JUST_CLIMBED_TIME = .085f,                  // Time after a jump considered (just jumped)
    FREE_JUMP_TIME = .102f,                     // Amount of time after leaving a wall that Juni gets a "free" jump (+1 extra frame)
    MAX_SPEED_WALK = 90f,                       // Max speed while walking
    MAX_SPEED_RUN = 175f,                       // Max speed while running
    MAX_X_SPEED_UMBRELLA = 130f,                // Maximum X speed when Juni has the umbrella deployed
    PULL_OVER_FORCE_X = 30f,                    // X Force guranteed when reaching the top of a climb
    PULL_OVER_SPEED_Y = -125f,                  // Y Speed set when reaching the top of a climb
    SLOPE_MAX_ANGLE = 1.11f,                    // The Maximum angle a floor can be before becoming a wall (in original 2-pixel obstacle is a bump, 3-4-pixel obstacle is a stopper, 5-pixel obstacle is a wall)
    UPDRAFT_FORCE = .116f,                      // The base updraft force exerted (.111 in original)
    UPDRAFT_FORCE_HOLD = .4f,                   // The updraft force exterted when holding jump
    MAX_UPDRAFT_SPEED = -225f,                  // Maximum Y speed in an updraft
    MAX_UPDRAFT_SPEED_HOLD = -233f,             // Maximum Y speed in an updraft while holding jump
    MAX_X_MOVING_DELTA = 2500f,                 // Maximum rate of change of X velocity when moving
    MAX_X_DECAY_DELTA = 1500f,                  // Maximum rate of change of X velocity when stopped
    TERM_VEL = 350f,                            // Maximum +Y velocity
    TERM_VEL_UMB = 62.5f,                       // Maximum +Y velocity when Juni has the umbrella deployed
    TERM_VEL_UMB_HIGHJUMP_HOLD = 51.5f,         // Maximum +Y velocity when Juni has the umbrella deployed while holding jump with high jump power
    TERM_VEL_UMB_LOWJUMP_HOLD = 60f,            // Maximum +Y velocity when Juni has the umbrella deployed while holding jump with no high jump power
    TERM_VEL_UP = 21f,                          // Maximum +Y velocity when Juni has the umbrella deployed in an updraft
    CLIMB_SPEED = -125f,                        // Speed Juni climbs up a wall
    SLIDE_SPEED = 25f,                          // Speed Juni slides down a wall
    CLIMB_JUMP_X_SPEED = 130f,                  // Speed Juni jumps away from a wall
    BUMP_Y_SPEED = -245f,                       // Speed Juni goes up when running over a bump. value / fps is max height of obstacle (currently 4px)
    INSIDE_X_SPEED = -22f,                      // Speed at which Juni moves along the x-axis when stuck inside walls
    INSIDE_Y_SPEED = -10f,                      // Speed at which Juni moves along the y-axis when stuck inside walls
    DEBUG_FLY_SPEED = 300f,                     // Speed at which Juni flies while in debug fly mode
    SWIM_JUMP_SPEED = -100f,                    // Replacements for swimming
    SWIM_MAX_SPEED_WALK = 50.7f,
    SWIM_MAX_SPEED_RUN = 100f,
    SWIM_LOW_JUMP_HOLD_POWER = 950f,
    SWIM_HIGH_JUMP_HOLD_POWER = 1025f,
    SWIM_PULL_OVER_SPEED_Y = -80f,
    SWIM_MAX_X_SPEED_UMBRELLA = 55f,
    SWIM_TERM_VEL = 100f,
    SWIM_TERM_VEL_UMB = 22.9f,
    SWIM_TERM_VEL_UMB_HIGHJUMP_HOLD = 9.84f,
    SWIM_TERM_VEL_UMB_LOWJUMP_HOLD = 11.7f,
    SWIM_UPDRAFT_FORCE = .4f,
    SWIM_MAX_UPDRAFT_SPEED = -100f,
    SWIM_EXIT_BOOST = 2.1f;

    [Signal] public delegate void Jumped();
    [Signal] public delegate void PowerChanged();
    [Signal] public delegate void HologramStopped(Juni juni);
    [Signal] public delegate void DownEvent(Juni juni);
    [Signal] public delegate void Died(Juni juni);

    PackedScene hologram_scene;

    public Godot.Vector2 velocity = Godot.Vector2.Zero;
    public int dir = 0;

    public Godot.Vector2 BaseCorrection = new Godot.Vector2(0f, 3.4f);

    PackedScene double_jump_scene;

    public GDKnyttGame Game { get; private set; }
    public GDKnyttArea GDArea => Game.CurrentArea; 
    public Sprite Detector { get; private set; }
    public KnyttPoint AreaPosition => GDArea.getPosition(GlobalPosition); 

    public JuniValues Powers { get; }

    public Checkers Checkers { get; private set; }

    public JuniMotionParticles MotionParticles { get; private set; }

    // States
    public JuniState CurrentState { get; private set; }
    private JuniState next_state = null;

    public bool dead = false;
    public int just_reset = 0;

    public int jumps = 0;

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

    public Color JuniClothes     { set { material.SetShaderParam("clothes_color", value); } }
    private bool JuniClothesSkip { set { material.SetShaderParam("clothes_skip", value); } }
    public Color JuniSkin        { set { material.SetShaderParam("skin_color", value); } }
    private bool JuniSkinSkip    { set { material.SetShaderParam("skin_skip", value); } }

    public float TerminalVelocity
    {
        get
        {
            if (Swim)
            {
                return !Umbrella.Deployed ? SWIM_TERM_VEL :
                    InUpdraft ? TERM_VEL_UP :
                    !juniInput.JumpHeld && !juniInput.UmbrellaHeld ? SWIM_TERM_VEL_UMB :
                    Powers.getPower(PowerNames.HighJump) ? SWIM_TERM_VEL_UMB_HIGHJUMP_HOLD : SWIM_TERM_VEL_UMB_LOWJUMP_HOLD;
            }
            else
            {
                return !Umbrella.Deployed ? TERM_VEL :
                    InUpdraft ? TERM_VEL_UP :
                    !juniInput.JumpHeld && !juniInput.UmbrellaHeld ? TERM_VEL_UMB :
                    Powers.getPower(PowerNames.HighJump) ? TERM_VEL_UMB_HIGHJUMP_HOLD : TERM_VEL_UMB_LOWJUMP_HOLD;
            }
            // TODO: different terminal velocities when Juni has no run power. Can reach unreachable areas, but almost impossible situation
        }
    }

    public int JumpLimit => Powers.getPower(PowerNames.DoubleJump) ? 2 : 1; 
    public bool CanClimb => Powers.getPower(PowerNames.Climb) && Checkers.Colliding; 
    public bool CanUmbrella => Powers.getPower(PowerNames.Umbrella); 
    public bool Grounded { get; private set; }
    public bool DidJump => juniInput.JumpEdge && Grounded && CanJump; 
    public bool FacingRight
    {
        set { if (FacingRight != value) { Rotation = 0; Scale = new Godot.Vector2(value ? 1 : -1, 1); } }
        get { return Scale.x > 0 && Scale.y > 0; }
    }
    public bool ApparentFacingRight => Hologram != null ? !(Hologram as Sprite).FlipH : FacingRight; 
    public bool DidAirJump => juniInput.JumpEdge && (CanFreeJump || (jumps < JumpLimit)); 
    public bool CanAnyJump => Grounded ? CanJump : (CanFreeJump || jumps < JumpLimit);

    // Whether or not Juni is in a NoJump situation
    int no_jumps = 0; // Number of no jump zones conditions covering Juni
    public bool CanJump
    {
        get { return no_jumps == 0; }
        set { no_jumps += (value ? -1 : 1); }
    }

    // Whether or not Juni is in a sticky area
    int stickies = 0; // Number of sticky zones covering Juni
    public bool Sticky
    {
        get { return stickies > 0; }
        set { stickies += (value ? 1 : -1); }
    }

    public void doMuffle(bool on)
    {
        string[] audio_nodes = { "Walk", "Climb", "Slide", "Run", "Jump", "Land" };
        foreach (var audio_node in audio_nodes)
        {
            GetNode<AudioStreamPlayer2D>($"Audio/{audio_node}Player2D").VolumeDb = on ? -10 : 5;
        }
    }

    public bool InHologramPlace { get; set; }

    int swim_zones = 0;
    public bool Swim
    {
        get { return swim_zones > 0 || GDArea.Swim; }
        set
        {
            swim_zones += (value ? 1 : -1);
            if (!value && swim_zones == 0 && !GDArea.Swim && velocity.y < 0 && CurrentState is JumpState)
            {
                float speed_limit = Powers.getPower(PowerNames.HighJump) ? JUMP_SPEED_HIGH : JUMP_SPEED_LOW;
                velocity.y = Mathf.Max(speed_limit, velocity.y * SWIM_EXIT_BOOST);
            }
        }
    }

    public Godot.Vector2 ApparentPosition => (Hologram == null) ? GlobalPosition : Hologram.GlobalPosition; 
    public bool CanDeployHologram
    {
        get
        {
            return ((CurrentState is IdleState) || (CurrentState is WalkRunState)) &&
                   !(GDArea.BlockHologram && !InHologramPlace);
        }
    }
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
            return Swim ? (WalkRun ? SWIM_MAX_SPEED_RUN : SWIM_MAX_SPEED_WALK)
                        : (WalkRun ? MAX_SPEED_RUN : MAX_SPEED_WALK);
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
    public bool Immune
    {
        get { return _immune; }
        set
        {
            _immune = value;
            CollisionLayer = value ? (uint)0 : 1;
            if (value) { Game.sendCheat(); }
        }
    }

    public Sprite Sprite { get; private set; }
    public Umbrella Umbrella { get; private set; }
    public AnimationPlayer Anim { get; private set; }
    private ShaderMaterial material;

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
                Game.sendCheat();
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
            Checkers.Disabled = value;
            _collisions_disabled = value;
            enforceCollisionMap();
            if (!value && !Checkers.IsInside) { Grounded = MoveAndCollide(Godot.Vector2.Down, testOnly: true) != null; }
        }
    }

    // Toggle the collision shapes
    CollisionShape2D[] _collision_polygons = { null, null, null, null };
    bool[] _collision_map = { true, true, true, false };
    public void setCollisionMap(bool main, bool back, bool small_main, bool small_climb)
    {
        _collision_map = new bool[] { main, back, small_main, small_climb };
        enforceCollisionMap();
    }

    private void enforceCollisionMap()
    {
        for (int i = 0; i < 4; i++)
        {
            _collision_polygons[i].Disabled = CollisionsDisabled || !_collision_map[i];
        }
    }

    public Juni()
    {
        juniInput = new JuniInput(this);
        this.Powers = new JuniValues();
        this.double_jump_scene = ResourceLoader.Load("res://knytt/juni/DoubleJump.tscn") as PackedScene;
    }

    public override void _Ready()
    {
        GetNode("/root/Console").Connect("ConsoleOpen", this, nameof(OnConsoleOpen));
        GetNode("/root/Console").Connect("ConsoleClosed", this, nameof(OnConsoleClosed));

        _collision_polygons = new CollisionShape2D[]
        {
            GetNode<CollisionShape2D>("MainShape"),
            GetNode<CollisionShape2D>("BackShape"),
            GetNode<CollisionShape2D>("Hitbox/UsualShape"),
            GetNode<CollisionShape2D>("Hitbox/ClimbShape"),
        };
        hologram_scene = ResourceLoader.Load("res://knytt/juni/Hologram.tscn") as PackedScene;
        MotionParticles = GetNode<JuniMotionParticles>("JuniMotionParticles");
        Detector = GetNode<Sprite>("Detector");
        Detector.Visible = true;
        Checkers = GetNode<Checkers>("Checkers");
        Sprite = GetNode<Sprite>("Sprite");
        material = Sprite.Material as ShaderMaterial;
        Umbrella = GetNode<Umbrella>("Umbrella");
        Umbrella.reset();
        Anim = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
        just_reset = 1; // skip first frame just to update Grounded
        transitionState(new IdleState(this));
    }

    public void OnConsoleOpen()
    {
        juniInput.Enabled = false;
    }

    public void OnConsoleClosed()
    {
        juniInput.Enabled = true;
    }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
        this.Powers.readFromSave(Game.GDWorld.KWorld.CurrentSave);
        enableAttachment(this.Powers.Attachment);
        changeCharacter(this.Powers.Character ?? Game.GDWorld.KWorld.Info.Character);
        game.applyTint(this.Powers.Tint.Item1, this.Powers.Tint.Item2, this.Powers.Tint.Item3);
        GetNode<StandartSoundPlayer>("Audio/StandartSoundPlayer").KWorld = game.GDWorld.KWorld;

        int clothes = Game.GDWorld.KWorld.Info.Clothes;
        JuniClothesSkip = clothes == -1;
        if (clothes != -1) { JuniClothes = new Color(KnyttUtil.BGRToRGBA(clothes)); }
        int skin = Game.GDWorld.KWorld.Info.Skin;
        JuniSkinSkip = skin == -1;
        if (skin != -1) { JuniSkin = new Color(KnyttUtil.BGRToRGBA(skin)); }
    }

    public void setPower(PowerNames name, bool value)
    {
        if (Game.GDWorld.KWorld.INIData["World"]["Format"] == "4")
        {
            if (name == PowerNames.Umbrella && !value && Umbrella.Deployed) { Umbrella.Deployed = false; }
            if (name == PowerNames.Hologram && !value && Hologram != null) { stopHologram(); }
        }
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
        if (Input.IsActionJustPressed("debug_slow")) { GDKnyttDataStore.CurrentSpeed /= 1.2f; }
        if (Input.IsActionJustPressed("debug_fast")) { GDKnyttDataStore.CurrentSpeed *= 1.2f; }
        if (Input.IsActionJustPressed("main_menu")) { Game.quit(); }
    }

    public void processMotion(float delta)
    {
        if (just_reset > 0)
        {
            if (--just_reset == 0) { SetDeferred("CollisionsDisabled", false); }
            return;
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
        handleBumps(delta);

        this.CurrentState.PostProcess(delta);

        // Pull-over-edge
        if (JustClimbed)
        {
            if (velocity.y < 0)
            {
                velocity.x = FacingRight ? Mathf.Max(velocity.x, PULL_OVER_FORCE_X) : Mathf.Min(velocity.x, -PULL_OVER_FORCE_X);
                if (_just_climbed == JUST_CLIMBED_TIME)
                {
                    velocity.y = Mathf.Max(Swim ? SWIM_PULL_OVER_SPEED_Y : PULL_OVER_SPEED_Y, velocity.y);
                }
            }
            _just_climbed -= delta;
            if (_can_free_jump <= 0f) { JustClimbed = false; }
        }

        // Can-free-jump
        if (CanFreeJump)
        {
            _can_free_jump -= delta;
            if (_can_free_jump <= 0f) { jumps++; CanFreeJump = false; }
        }

        // Limit falling speed to terminal velocity
        velocity.y = Mathf.Min(TerminalVelocity, velocity.y);

        if (Checkers.IsInside) { Translate(new Godot.Vector2(INSIDE_X_SPEED * MoveDirection * delta, INSIDE_Y_SPEED * delta)); }
        else
        {
            // Do the movement in two steps to avoid hanging up on tile seams
            //var snap = IsOnFloor() && !juniInput.JumpEdge && !CanClimb ? 10 * Godot.Vector2.Down : Godot.Vector2.Zero; // previous version
            velocity.x = MoveAndSlideWithSnap(new Godot.Vector2(velocity.x, 0), 5 * Godot.Vector2.Down, Godot.Vector2.Up,
                                              stopOnSlope: true, floorMaxAngle: SLOPE_MAX_ANGLE).x;
            velocity.y = MoveAndSlide(new Godot.Vector2(0, velocity.y), Godot.Vector2.Up,
                                      stopOnSlope: true, floorMaxAngle: SLOPE_MAX_ANGLE).y;
            Grounded = IsOnFloor() || MoveAndCollide(Godot.Vector2.Down, testOnly: true) != null;
        }
    }

    private void processFlyMode(float delta)
    {
        var dir = new Godot.Vector2();
        if (Input.IsActionPressed("up")) { dir.y -= 1f; }
        if (Input.IsActionPressed("down")) { dir.y += 1f; }
        if (Input.IsActionPressed("left")) { dir.x -= 1f; }
        if (Input.IsActionPressed("right")) { dir.x += 1f; }

        Translate(dir.Normalized() * DEBUG_FLY_SPEED * delta / GDKnyttDataStore.CurrentSpeed);
    }

    private void handleBumps(float delta)
    {
        if (Checkers.Bump && CurrentState is WalkRunState && (juniInput.LeftHeld || juniInput.RightHeld))
        {
            Translate(new Godot.Vector2(0, BUMP_Y_SPEED * delta));
        }
    }

    private void handleGravity(float delta)
    {
        // This particular value needs to be multiplied by delta to ensure
        // framerate independence
        bool jump_held = juniInput.JumpHeld || (Umbrella.Deployed && juniInput.UmbrellaHeld);
        if (InUpdraft && Umbrella.Deployed)
        {
            velocity.y -= GRAVITY * delta * (Swim ? SWIM_UPDRAFT_FORCE :
                                             jump_held ? UPDRAFT_FORCE_HOLD : UPDRAFT_FORCE);
            velocity.y = Mathf.Max(Swim ? SWIM_MAX_UPDRAFT_SPEED :
                                   jump_held ? MAX_UPDRAFT_SPEED_HOLD : MAX_UPDRAFT_SPEED, velocity.y);
        }
        else
        {
            if (!Grounded) { velocity.y += GRAVITY * delta; }
            else { velocity.y = GRAVITY * delta; }
            if (jump_held)
            {
                var jump_hold = Powers.getPower(PowerNames.HighJump) ?
                    (Swim ? SWIM_HIGH_JUMP_HOLD_POWER : HIGH_JUMP_HOLD_POWER) :
                    (Swim ? SWIM_LOW_JUMP_HOLD_POWER : LOW_JUMP_HOLD_POWER);
                velocity.y -= jump_hold * delta;
            }
            else if (!Powers.getPower(PowerNames.HighJump))
            {
                velocity.y += LOW_JUMP_EXTRA_GRAVITY * delta;
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
        var node = hologram_scene.Instance() as Sprite;
        GDArea.GDWorld.Game.AddChild(node);
        node.GlobalPosition += GlobalPosition;
        node.FlipH = !FacingRight;
        node.Texture = Sprite.Texture;
        node.Hframes = Sprite.Hframes;
        node.Vframes = Sprite.Vframes;
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

    private string character = "juni";

    public void changeCharacter(string name)
    {
        if (name == null || name == "" || name.ToLower() == "juni")
        {
            if (character != "juni")
            {
                Sprite.Texture = GDKnyttAssetManager.loadInternalTexture("res://knytt/juni/juni.png");
                Umbrella.Texture = GDKnyttAssetManager.loadInternalTexture("res://knytt/juni/umbrella_item.png");
                Umbrella.Custom = false;
                JuniClothesSkip = Game.GDWorld.KWorld.Info.Clothes != -1;
                JuniSkinSkip = Game.GDWorld.KWorld.Info.Skin != -1;
                Powers.Character = character = "juni";
            }
        }
        else
        {
            if (character != name)
            {
                Sprite.Texture = Game.GDWorld.KWorld.getWorldTexture($"Custom Objects/{name}.png") as Texture;
                Umbrella.Texture = Game.GDWorld.KWorld.getWorldTexture($"Custom Objects/u{name}.png") as Texture;
                Umbrella.Custom = Umbrella.Texture != null;
                if (Umbrella.Texture == null) { Umbrella.Texture = GDKnyttAssetManager.loadInternalTexture("res://knytt/juni/umbrella_item.png"); }
                JuniClothesSkip = true;
                JuniSkinSkip = true;
                Powers.Character = character = name;
            }
        }
        var frames = Sprite.Texture.GetSize() / 24;
        Sprite.Hframes = (int)frames.x;
        Sprite.Vframes = (int)frames.y;

        var uframes = Umbrella.Texture.GetSize() / 24;
        Umbrella.Hframes = (int)uframes.x;
        Umbrella.Vframes = (int)uframes.y;
    }

    private void _on_Hitbox_body_entered(BaseBullet bullet)
    {
        if (!Immune && !DebugFlyMode) { die(); }
    }

    // This kills the Juni
    public void die()
    {
        CallDeferred("_die");
    }

    private async void _die()
    {
        if (dead)
        {
            KnyttLogger.Debug("Juni is already dead");
            return;
        }

        if (Immune || DebugFlyMode)
        {
            KnyttLogger.Debug("Juni is immune to death");
            return;
        }

        GetNode<DeathParticles>("DeathParticles").Play();
        GetNode<AudioStreamPlayer2D>("Audio/DiePlayer2D").Play();
        this.next_state = null;
        this.clearState();
        Sprite.Visible = false;
        Umbrella.Visible = false;
        Detector.Visible = false;
        this.dead = true;

        EmitSignal(nameof(Died), this);

        var timer = GetNode<Timer>("RespawnTimer");
        timer.Start();

        KnyttLogger.Debug("Juni has died");

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
        Sprite.Visible = true;
        this.dead = false;
        this.velocity = Godot.Vector2.Zero;
        this.transitionState(new IdleState(this));
        this.executeStateTransition();

        SetDeferred("CollisionsDisabled", true);
        this.just_reset = 2;

        dir = 0;
        jumps = 0;
        JustClimbed = false;
        CanFreeJump = false;

        Umbrella.reset();
        stopHologram(cleanup: true);
        enableAttachment(Powers.Attachment);
        changeCharacter(Powers.Character);
        Game.applyTint(Powers.Tint.Item1, Powers.Tint.Item2, Powers.Tint.Item3);
    }

    private void handleXMovement(float delta)
    {
        // Move, then clamp
        if (dir != 0)
        {
            var uspeed = Umbrella.Deployed ?
                (Mathf.Min(MaxSpeed, (Swim ? SWIM_MAX_X_SPEED_UMBRELLA : MAX_X_SPEED_UMBRELLA))) : MaxSpeed;
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
        if (sound) { GetNode<AudioStreamPlayer2D>("Audio/JumpPlayer2D").Play(); }
        velocity.y = jump_speed;

        if (air_jump && jumps > 0) { doubleJumpEffect(); }

        jumps = reset_jumps ? 1 : jumps + 1;
        JustClimbed = false;
        CanFreeJump = false;

        // Do not emit this event if the hologram is out, as it cannot jump
        if (Hologram == null) { EmitSignal(nameof(Jumped), this); }
    }

    public void executeJump(bool air_jump = false, bool sound = true, bool reset_jumps = false, bool check_stow = true)
    {
        if (TouchSettings.UmbrellaCheat && check_stow && Umbrella.Deployed)
        {
            Umbrella.Deployed = false;
            Umbrella.DeployOnFall = true;
        }

        float jump_speed = Swim ? SWIM_JUMP_SPEED :
                            Umbrella.Deployed ? JUMP_SPEED_UMBRELLA :
                            Powers.getPower(PowerNames.HighJump) ? JUMP_SPEED_HIGH : JUMP_SPEED_LOW;
        executeJump(jump_speed, air_jump, sound, reset_jumps);
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
