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
        juni.InHologramPlace = true;
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.InHologramPlace = false;
    }
}
