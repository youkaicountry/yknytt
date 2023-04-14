using Godot;
using YKnyttLib;

public partial class ItemInfo : Control
{
    [Export] public int ItemID;

    public override void _Ready()
    {
        var anim = GetNode<AnimatedSprite2D>("ItemInfo");
        anim.Animation = $"Power{ItemID}";
        anim.Play();
    }

    public virtual void updateItem(JuniValues values)
    {
        var m = Modulate;
        m.A = values.getPower(ItemID) ? 1f : .18f;
        Modulate = m;
    }
}
