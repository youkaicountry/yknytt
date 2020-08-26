using Godot;
using System;

public class IdleState : JuniState
{
    public IdleState(Juni juni) : base(juni) { }

    public override void onEnter()
    {
        juni.Anim.Play("Idle");
    }

    public override void PreProcess(float delta)
    {
        if (juni.MoveDirection != 0) { juni.transitionState(new WalkRunState(juni)); } 
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
        juni.velocity.x = juni.speed * dir;
        if (dir == 0) { juni.transitionState(new IdleState(juni)); }
    }

    public override void onExit()
    {
        juni.GetNode<RawAudioPlayer2D>("Audio/WalkPlayer2D").Stop();
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
