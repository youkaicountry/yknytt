public class IdleState : JuniState
{
    public IdleState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Idle");
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir != 0) { juni.transitionState(new WalkRunState(juni, juni.WalkRun)); }
    }

    public override void PostProcess(float delta) 
    {
        if (juni.DidJump) { juni.transitionState(new JumpState(juni)); }
        else if ( !juni.Grounded ) { juni.transitionState(new FallState(juni)); } // Ground falls out from under Juni
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
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
        if (juni.dir == 0) { juni.transitionState(new IdleState(juni)); }
        if (juni.WalkRun != walk_run) { juni.transitionState(new WalkRunState(juni, !walk_run)); }
    }

    public override void PostProcess(float delta) 
    { 
        if (juni.DidJump) { juni.transitionState(new JumpState(juni)); }
        else if ( !juni.Grounded ) { juni.transitionState(new FallState(juni)); } // Juni Walks off Ledge / ground falls out from under
    }

    public override void onExit()
    {
        juni.GetNode<RawAudioPlayer2D>(string.Format("Audio/{0}Player2D", walk_run ? "Run":"Walk")).Stop();
    }
}

public class JumpState : JuniState
{
    public JumpState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Jump");
        juni.GetNode<RawAudioPlayer2D>("Audio/JumpPlayer2D").Play();
        juni.velocity.y = juni.jump_speed;
    }

    public override void PreProcess(float delta)
    {
        juni.dir = juni.MoveDirection;
    }

    public override void PostProcess(float delta)
    { 
        //if (juni) // Check if falling
        if (juni.Grounded) 
        { 
            juni.transitionState(new IdleState(juni)); 
            juni.GetNode<RawAudioPlayer2D>("Audio/LandPlayer2D").Play();
        }
        // If 
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
    }

    public override void PostProcess(float delta)
    { 
        //if (juni) // Check if falling
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
