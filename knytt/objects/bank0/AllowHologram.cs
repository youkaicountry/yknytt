using Godot;

public class AllowHologram : GDKnyttBaseObject
{
    public override void _Ready()
    {
        GDArea.BlockHologram = true;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        if (GDArea.Selector.GetSize(this) == 0) { juni.InHologramPlace = true; }
        GDArea.Selector.Register(this);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        GDArea.Selector.Unregister(this);
        if (GDArea.Selector.GetSize(this) == 0) { juni.InHologramPlace = false; }
    }
}
