public class NoWall : GhostObject
{
    protected override void _InvReady()
    {
        Flickering = false;
    }

    protected override void _impl_initialize() { }

    protected override void _InvEnable()
    {
        // Copy values of the cells
        int a = GDArea.Tiles.Layers[3].A.GetCell(Coords.x, Coords.y);
        int b = GDArea.Tiles.Layers[3].B.GetCell(Coords.x, Coords.y);

        // Check if there is nothing to transfer. This also prevents Retransfering when an area is woken back up
        if (a == 0 && b == 0) { return; }

        // Set the values of the Ghost Terrain Cells
        GDArea.Tiles.Layers[4].A.SetCell(Coords.x, Coords.y, a);
        GDArea.Tiles.Layers[4].B.SetCell(Coords.x, Coords.y, b);

        // Clear the original cells
        GDArea.Tiles.Layers[3].A.SetCell(Coords.x, Coords.y, 0);
        GDArea.Tiles.Layers[3].B.SetCell(Coords.x, Coords.y, 0);
    }
}
