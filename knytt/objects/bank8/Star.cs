using Godot;
using YUtil.Random;

public partial class Star : GesturesObject
{
    public override void _Ready()
    {
        base._Ready();
        GDArea.Selector.Register(this);
    }

    protected override void nextAnimation()
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            sprite.Offset = new Vector2(random.NextFloat(24f), random.NextFloat(24f));
            base.nextAnimation();
        }
        else
        {
            idle();
        }
    }
}
