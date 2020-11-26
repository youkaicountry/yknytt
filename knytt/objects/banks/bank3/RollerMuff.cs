using Godot;
using YUtil.Random;

public class RollerMuff : Muff
{
    [Export] float rollInterval = 0;

    protected bool roll = false;

    public override void _Ready()
    {
        base._Ready();
        GDArea.Selector.Register(this);
        GetNode<Timer>("FirstDelayTimer").Start(GDKnyttDataStore.random.NextFloat(rollInterval));
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("RollTimer").Start(rollInterval);
        _on_RollTimer_timeout();
    }

    private void _on_RollTimer_timeout()
    {
        if (!GDArea.Selector.IsObjectSelected(this)) { return; }
        roll = true;
        speed = 24;
        changeDirection(Juni.ApparentPosition.x < Center.x ? -1 : 1);
        sprite.Play("roll");
    }

    protected override void _on_SpeedTimer_timeout()
    {
        if (!roll) { base._on_SpeedTimer_timeout(); }
    }

    protected override void _on_DirectionTimer_timeout()
    {
        if (!roll) { base._on_DirectionTimer_timeout(); }
    }

    protected override void collide()
    {
        if (roll)
        {
            roll = false;
            speed = 0;
            sprite.Play("default");
        }
        base.collide();
    }
}
