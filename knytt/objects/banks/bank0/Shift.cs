using System.Collections.Generic;
using Godot;
using YKnyttLib;
using static YKnyttLib.KnyttShift;

public class Shift : GDKnyttBaseObject
{
    HashSet<Juni> junis;
    KnyttShift shift;

    public override void _Ready()
    {
        junis = new HashSet<Juni>();
        shift = new KnyttShift(GDArea.Area, Coords, (ShiftID)(ObjectID.y-14));
        setupFromShift();
    }

    private void setupFromShift()
    {
        var shape = GetNode<Node>("Shapes").GetChild<Node2D>((int)shift.Shape);
        shape.Visible = shift.Visible;
        shape.GetNode<Area2D>("Area2D").SetDeferred("monitoring", true);
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Add((Juni)body);   
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni)) { return; }
        junis.Remove((Juni)body);
    }

    protected override void _impl_initialize() { }

    protected override void _impl_process(float delta)
    {
        if (junis.Count == 0) { return; }
        foreach (var juni in junis)
        {
            if (shift.DenyHologram && juni.Hologram != null) { continue; }
            if (shift.OnTouch || juni.DownPressed) { executeShift(juni); }
        }
    }

    private void executeShift(Juni juni)
    {
        var relative_area = shift.RelativeArea;
        GDArea.GDWorld.Game.changeAreaDelta(relative_area, true);
        var jgp = Juni.GlobalPosition;

        // Move Juni to the same spot in the new area
        jgp.x += relative_area.x * GDKnyttArea.Width;
        jgp.y += relative_area.y * GDKnyttArea.Height;

        // Move Juni to the correct location in the area
        var relative_pos = shift.RelativePosition;
        jgp.x += relative_pos.x * GDKnyttAssetManager.TILE_WIDTH;
        jgp.y += relative_pos.y * GDKnyttAssetManager.TILE_HEIGHT;
        Juni.GlobalPosition = jgp;
    }
}
