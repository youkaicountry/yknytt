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
        string world_name = GetNodeOrNull<GDKnyttGame>("/root/GKnyttGame")?.GDWorld?.KWorld?.WorldDirectoryName;
        if (world_name != null) { checkCustomIcon(world_name); }
    }

    public virtual void updateItem(JuniValues values)
    {
        var m = Modulate;
        m.a = values.getPower(ItemID) ? 1f : .18f;
        Modulate = m;
    }

    public void checkCustomIcon(string prefix)
    {
        var anim = GetNode<AnimatedSprite>("ItemInfo");
        var new_anim = $"{prefix} {ItemID} icon";
        if (anim.Frames.HasAnimation(new_anim)) { anim.Animation = new_anim; }
    }
}
