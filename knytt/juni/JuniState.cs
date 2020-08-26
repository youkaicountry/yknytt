public class IdleState : JuniState
{
    public IdleState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Idle");
    }

    public override void PreProcess(float delta)
    {
        var dir = juni.MoveDirection;
        juni.velocity.x = juni.speed * dir;
        if (dir != 0) { juni.transitionState(new WalkRunState(juni)); } 
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
    public WalkRunState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.GetNode<RawAudioPlayer2D>("Audio/WalkPlayer2D").Play();
        juni.Anim.Play("Walk");
    }

    public override void PreProcess(float delta)
    {
        var dir = juni.MoveDirection;
        juni.velocity.x = juni.speed * dir; // TODO: Get speed from a property
        if (dir == 0) { juni.transitionState(new IdleState(juni)); }
    }

    public override void PostProcess(float delta) 
    { 
        if (juni.DidJump) { juni.transitionState(new JumpState(juni)); }
        else if ( !juni.Grounded ) { juni.transitionState(new FallState(juni)); } // Juni Walks off Ledge / ground falls out from under
    }

    public override void onExit()
    {
        juni.GetNode<RawAudioPlayer2D>("Audio/WalkPlayer2D").Stop();
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
        var dir = juni.MoveDirection;
        juni.velocity.x = juni.speed * dir; // TODO: Get speed from a property
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
        var dir = juni.MoveDirection;
        juni.velocity.x = juni.speed * dir; // TODO: Get speed from a property
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
