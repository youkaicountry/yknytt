using Godot;

public class IdleState : JuniState
{
    public override bool StickToGround => true; 

    public IdleState(Juni juni) : base(juni)
    {

    }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true, false);
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
            juni.velocity.y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.DidJump) { juni.executeJump(reset_jumps: true); }
        else if (!juni.Grounded) // Ground falls out from under Juni
        {
            juni.CanFreeJump = true;
            juni.transitionState(new FallState(juni));
        }
    }
}

// TODO: Have a juni function that checks if walk or run
public class WalkRunState : JuniState
{
    bool walk_run;

    string WalkRunString => walk_run ? "Run" : "Walk"; 

    public override bool StickToGround => true; 

    public WalkRunState(Juni juni, bool walk_run) : base(juni)
    {
        this.walk_run = walk_run;
        juni.MotionParticles.CurrentMotion = walk_run ? JuniMotionParticles.JuniMotion.RUN : JuniMotionParticles.JuniMotion.WALK;
    }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true, false);
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
            juni.velocity.y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.Grounded) { juni.CanFreeJump = true; }

        if (juni.juniInput.JumpEdge && juni.CanJump && juni.CanFreeJump)
        {
            juni.executeJump(reset_jumps: true); // juni.CanFreeJump = false inside
        }
        else if (!juni.Grounded && juni.DidAirJump)
        {
            juni.executeJump(air_jump: true, sound: true);
        }
        else if (!juni.CanFreeJump) // Juni Walks off Ledge / ground falls out from under
        {
            juni.jumps = 1; // Transition as if Juni has jumped once
            juni.transitionState(new FallState(juni));
        }
    }

    public override void onExit()
    {
        juni.GetNode<AudioStreamPlayer2D>($"Audio/{WalkRunString}Player2D").Stop();
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public class ClimbState : JuniState
{
    public ClimbState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        calcCollisionMap();
        juni.Anim.Play("Climbing");
        juni.GetNode<AudioStreamPlayer2D>("Audio/ClimbPlayer2D").Play();
        juni.jumps = 0;
        juni.CanFreeJump = false;
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
        juni.setCollisionMap(true, false, false, true);
    }

    public override void PostProcess(float delta)
    {
        if (juni.juniInput.UpHeld) { juni.velocity.y = Juni.CLIMB_SPEED; }
        else if (juni.velocity.y > 0f) { juni.transitionState(new SlideState(juni)); }

        if (juni.juniInput.JumpEdge)
        {
            juni.velocity.x = juni.FacingRight ? -Juni.CLIMB_JUMP_X_SPEED : Juni.CLIMB_JUMP_X_SPEED;
            juni.executeJump(check_stow: false);
        }
    }

    public override void onExit()
    {
        juni.GetNode<AudioStreamPlayer2D>("Audio/ClimbPlayer2D").Stop();
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public class SlideState : ClimbState
{
    private AudioStreamPlayer2D slide_sound;

    public SlideState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        calcCollisionMap();
        juni.Anim.Play("Sliding");
        slide_sound = juni.GetNode<AudioStreamPlayer2D>("Audio/SlidePlayer2D");
        juni.jumps = 0;
        juni.CanFreeJump = false;
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
            juni.velocity.y = Juni.CLIMB_SPEED;
            juni.transitionState(new ClimbState(juni));
        }
        else if (!juni.juniInput.DownHeld)
        {
            juni.velocity.y = Juni.SLIDE_SPEED;
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
            juni.velocity.x = juni.FacingRight ? -Juni.CLIMB_JUMP_X_SPEED : Juni.CLIMB_JUMP_X_SPEED;
            juni.executeJump(check_stow: false);
        }
    }

    public override void onExit()
    {
        if (slide_sound.Playing) { slide_sound.Stop(); }
        juni.MotionParticles.CurrentMotion = JuniMotionParticles.JuniMotion.NONE;
    }
}

public class JumpState : JuniState
{
    public JumpState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true, false);
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.executeJump(air_jump: true, sound: true); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        else if (juni.velocity.y >= 0) { juni.transitionState(new FallState(juni)); }

        if ((juni.CanClimb || juni.velocity.y >= 0) && juni.Umbrella.DeployOnFall)
        {
            juni.Umbrella.Deployed = true;
            juni.Umbrella.DeployOnFall = false;
        }
    }
}

public class FallState : JuniState
{
    public FallState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.setCollisionMap(true, true, true, false);
        juni.Anim.Play("StartFall");
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.executeJump(air_jump: true, sound: true, check_stow: !juni.CanFreeJump); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        if (juni.Grounded)
        {
            if (juni.MoveDirection != 0) { juni.transitionState(new WalkRunState(juni, juni.WalkRun)); }
            else { juni.transitionState(new IdleState(juni)); }
            juni.GetNode<AudioStreamPlayer2D>("Audio/LandPlayer2D").Play();
        }
    }
}

public class JuniState
{
    protected Juni juni;
    public virtual bool StickToGround => false; 

    public JuniState(Juni juni)
    {
        this.juni = juni;
    }

    public virtual void onEnter() { }
    public virtual void PreProcess(float delta) { }
    public virtual void PostProcess(float delta) { }
    public virtual void onExit() { }
}
