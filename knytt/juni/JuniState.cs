using Godot;

public partial class IdleState : JuniState
{
    public override bool StickToGround { get { return true; } }

    public IdleState(Juni juni) : base(juni)
    {

    }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true);
        juni.Anim.Play("Idle");
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir != 0 && !juni.Sticky) { juni.transitionState(new WalkRunState(juni, juni.WalkRun)); }
    }

    public override void PostProcess(float delta)
    {
        // Do climb check first so jump will override it.
        if (juni.juniInput.UpHeld && juni.CanClimb)
        {
            juni.velocity.Y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.DidJump) { juni.executeJump(reset_jumps: true); }
        else if (!juni.Grounded) // Ground falls out from under Juni
        {
            juni.CanFreeJump = false;
            juni.jumps = 1;
            juni.transitionState(new FallState(juni));
        }
    }
}

// TODO: Have a juni function that checks if walk or run
public partial class WalkRunState : JuniState
{
    const float FALL_LEEWAY = .085f;

    bool walk_run;
    int fall_frames;
    int current_fall;

    string WalkRunString { get { return walk_run ? "Run" : "Walk"; } }

    public override bool StickToGround { get { return true; } }

    public WalkRunState(Juni juni, bool walk_run) : base(juni)
    {
        this.walk_run = walk_run;
        juni.MotionParticles.CurrentMotion = walk_run ? JuniMotionParticles.JuniMotion.RUN : JuniMotionParticles.JuniMotion.WALK;

        // Calculate the leeway frames to fall in a frame independent manner
        fall_frames = Mathf.FloorToInt(FALL_LEEWAY / (1f / ((int)ProjectSettings.GetSetting("physics/common/physics_fps"))));
        current_fall = fall_frames;
    }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true);
        juni.GetNode<AudioStreamPlayer2D>($"Audio/{WalkRunString}Player2D").Play();
        juni.Anim.Play(WalkRunString);
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir == 0 || juni.Sticky) { juni.transitionState(new IdleState(juni)); }
        if (juni.WalkRun != walk_run) { juni.transitionState(new WalkRunState(juni, !walk_run)); }
    }

    public override void PostProcess(float delta)
    {
        // Do climb check first so jump will override it.
        if (juni.juniInput.UpHeld && juni.CanClimb)
        {
            juni.velocity.Y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.juniInput.JumpEdge && juni.CanJump && current_fall > 0) { juni.executeJump(reset_jumps: true); }
        else if (!juni.Grounded) // Juni Walks off Ledge / ground falls out from under
        {
            if (current_fall <= 0)
            {
                juni.CanFreeJump = false;
                juni.jumps = 1; // Transition as if Juni has jumped once
                juni.transitionState(new FallState(juni));
            }
            current_fall--;
        }
        else { current_fall = fall_frames; }
    }

    public override void onExit()
    {
        juni.GetNode<AudioStreamPlayer2D>($"Audio/{WalkRunString}Player2D").Stop();
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public partial class ClimbState : JuniState
{
    public ClimbState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        calcCollisionMap();
        juni.Anim.Play("Climbing");
        juni.GetNode<AudioStreamPlayer2D>("Audio/ClimbPlayer2D").Play();
        juni.jumps = 0;
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.CLIMB;
    }

    public override void PreProcess(float delta)
    {
        calcDir();
        if (!juni.CanClimb)
        {
            juni.transitionState(new FallState(juni));
            juni.JustClimbed = true;
            juni.CanFreeJump = true;
        }
    }

    protected void calcDir()
    {
        juni.dir = juni.MoveDirection;

        calcCollisionMap();
    }

    protected void calcCollisionMap()
    {
        if (!juni.FacingRight) { juni.setCollisionMap( true, true, false ); }
        else { juni.setCollisionMap(false, true, true); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.juniInput.UpHeld) { juni.velocity.Y = Juni.CLIMB_SPEED; }
        else if (juni.velocity.Y > 0f) { juni.transitionState(new SlideState(juni)); }

        if (juni.juniInput.JumpEdge)
        {
            juni.velocity.X = juni.FacingRight ? -Juni.CLIMB_JUMP_X_SPEED : Juni.CLIMB_JUMP_X_SPEED;
            juni.executeJump();
        }
    }

    public override void onExit()
    {
        juni.GetNode<AudioStreamPlayer2D>("Audio/ClimbPlayer2D").Stop();
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public partial class SlideState : ClimbState
{
    private AudioStreamPlayer2D slide_sound;

    public SlideState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        calcCollisionMap();
        juni.Anim.Play("Sliding");
        slide_sound = juni.GetNode<AudioStreamPlayer2D>("Audio/SlidePlayer2D");
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        calcDir();
        if (!juni.CanClimb)
        {
            juni.transitionState(new FallState(juni));
            juni.JustClimbed = true;
            juni.CanFreeJump = true;
        }
        if (juni.Grounded) { juni.transitionState(new IdleState(juni)); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.juniInput.UpHeld)
        {
            juni.velocity.Y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }
        else if (!juni.juniInput.DownHeld)
        {
            juni.velocity.Y = Juni.SLIDE_SPEED;
            if (slide_sound.Playing) { slide_sound.Stop(); }
            juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
        }
        else
        {
            // Holding down, do slide sound. Gravity controls motion
            if (!slide_sound.Playing) { slide_sound.Play(); }
            juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.CLIMB;
        }

        if (juni.juniInput.JumpEdge)
        {
            juni.velocity.X = juni.FacingRight ? -Juni.CLIMB_JUMP_X_SPEED : Juni.CLIMB_JUMP_X_SPEED;
            juni.executeJump();
        }
    }

    public override void onExit()
    {
        if (slide_sound.Playing) { slide_sound.Stop(); }
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public partial class JumpState : JuniState
{
    public JumpState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true);
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.executeJump(air_jump: true, sound: true); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        else if (juni.velocity.Y >= 0) { juni.transitionState(new FallState(juni)); }
    }
}

public partial class FallState : JuniState
{
    public FallState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true);
        juni.Anim.Play("StartFall");
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.executeJump(air_jump: true, sound: true); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        if (juni.Grounded)
        {
            juni.transitionState(new IdleState(juni));
            juni.GetNode<AudioStreamPlayer2D>("Audio/LandPlayer2D").Play();
        }
    }
}

public partial class JuniState
{
    protected Juni juni;
    public virtual bool StickToGround { get { return false; } }

    public JuniState(Juni juni)
    {
        this.juni = juni;
    }

    public virtual void onEnter() { }
    public virtual void PreProcess(float delta) { }
    public virtual void PostProcess(float delta) { }
    public virtual void onExit() { }
}
