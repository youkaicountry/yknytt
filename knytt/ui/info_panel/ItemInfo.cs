using Godot;
using YKnyttLib;

public class ItemInfo : Control
{
    [Export] public int ItemID;

    public override void _Ready()
    {
        var anim = GetNode<AnimatedSprite>("ItemInfo");
        anim.Animation = $"Power{ItemID}";
        anim.Play();
    }

    public void updateItem(JuniValues values)
    {
        var m = Modulate;
        m.a = values.getPower(ItemID) ? 1f : .18f;
        Modulate = m;
    }
}
