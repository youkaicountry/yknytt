using Godot;

public class IdleState : JuniState
{
    public IdleState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Idle");
        juni.jumps = 0; 
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir != 0) { juni.transitionState(new WalkRunState(juni, juni.WalkRun)); }
    }

    public override void PostProcess(float delta) 
    {
        // Do climb check first so jump will override it.
        if (juni.UpHeld && juni.CanClimb)
        {
            juni.velocity.y = -125f;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.DidJump) { juni.transitionState(new JumpState(juni)); }
        else if ( !juni.Grounded ) { juni.jumps++; juni.transitionState(new FallState(juni)); } // Ground falls out from under Juni
    }
}

// TODO: Have a juni function that checks if walk or run
public class WalkRunState : JuniState
{
    bool walk_run;

    public WalkRunState(Juni juni, bool walk_run) : base(juni)
    { 
        this.walk_run = walk_run;
    }

    public override void onEnter()
    {
        juni.GetNode<RawAudioPlayer2D>(string.Format("Audio/{0}Player2D", walk_run ? "Run":"Walk")).Play();
        juni.Anim.Play(walk_run ? "Run" : "Walk");
        juni.max_speed = walk_run ? 175f : 90f;
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir == 0) { juni.transitionState(new IdleState(juni)); }
        if (juni.WalkRun != walk_run) { juni.transitionState(new WalkRunState(juni, !walk_run)); }
    }

    public override void PostProcess(float delta)
    {
        // Do climb check first so jump will override it.
        if (juni.UpHeld && juni.CanClimb) 
        {
            juni.velocity.y = -125f;
            juni.transitionState(new ClimbState(juni));
        }

        if (juni.DidJump) { juni.transitionState(new JumpState(juni)); }
        else if ( !juni.Grounded ) { juni.jumps++; juni.transitionState(new FallState(juni)); } // Juni Walks off Ledge / ground falls out from under
    }

    public override void onExit()
    {
        juni.GetNode<RawAudioPlayer2D>(string.Format("Audio/{0}Player2D", walk_run ? "Run":"Walk")).Stop();
    }
}

public class ClimbState : JuniState
{
    public ClimbState(Juni juni) : base(juni) { }
    
    public override void onEnter()
    {
        juni.Anim.Play("Climbing");
        juni.GetNode<RawAudioPlayer2D>("Audio/ClimbPlayer2D").Play();
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (!juni.CanClimb)
        { 
            juni.transitionState(new FallState(juni));
            juni.just_climbed = .08f;
        }
    }

    public override void PostProcess(float delta)
    {
        if (juni.UpHeld) { juni.velocity.y = -125f; }
        else if (juni.velocity.y > 0f) { juni.transitionState(new SlideState(juni)); }

        if (juni.JumpEdge) 
        {
            juni.velocity.x = juni.FacingRight ? -130f : 130f;
            juni.transitionState(new JumpState(juni));
        }
    }

    public override void onExit()
    {
        juni.GetNode<RawAudioPlayer2D>("Audio/ClimbPlayer2D").Stop();
    }
}

public class SlideState : JuniState
{
    private AudioStreamPlayer2D slide_sound;

    public SlideState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Sliding");
        slide_sound = juni.GetNode<RawAudioPlayer2D>("Audio/SlidePlayer2D");
        juni.jumps = 0;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (!juni.CanClimb) 
        { 
            juni.transitionState(new FallState(juni));
            juni.just_climbed = .08f;
        }
        if (juni.Grounded) { juni.transitionState(new IdleState(juni)); }
    }

    public override void PostProcess(float delta)
    {
        if (juni.UpHeld) 
        { 
            juni.velocity.y = -125f; 
            juni.transitionState(new ClimbState(juni));
        }
        else if (!juni.DownHeld)
        {
            juni.velocity.y = 25f;
            if (slide_sound.Playing) { slide_sound.Stop(); }
        }
        else
        {
            // Holding down, do slide sound. Gravity controls motion
            if (!slide_sound.Playing) { slide_sound.Play(); }
        }

        if (juni.JumpEdge) 
        {
            juni.velocity.x = juni.FacingRight ? -130f : 130f;
            juni.transitionState(new JumpState(juni));
        }
    }

    public override void onExit()
    {
        if (slide_sound.Playing) { slide_sound.Stop(); }
    }
}

public class JumpState : JuniState
{
    public JumpState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.executeJump();
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.transitionState(new JumpState(juni)); }
    }

    public override void PostProcess(float delta)
    { 
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        else if (juni.velocity.y >= 0) { juni.transitionState(new FallState(juni)); }
    }
}

public class FallState : JuniState
{
    public FallState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("StartFall");
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.DidAirJump) { juni.transitionState(new JumpState(juni)); }
    }

    public override void PostProcess(float delta)
    { 
        if (juni.CanClimb) { juni.transitionState(new ClimbState(juni)); }
        if (juni.Grounded) 
        { 
            juni.transitionState(new IdleState(juni)); 
            juni.GetNode<RawAudioPlayer2D>("Audio/LandPlayer2D").Play();
        }
    }
}

public class JuniState
{
    protected Juni juni;

    public JuniState(Juni juni)
    {
        this.juni = juni;       
    }

    public virtual void onEnter() { }
    public virtual void PreProcess(float delta) { }
    public virtual void PostProcess(float delta) { }
    public virtual void onExit() { }
}
