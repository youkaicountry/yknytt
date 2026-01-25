public partial class NoWall : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        if (IsQueuedForDeletion()) { return; }

        // Copy values of the cells
        int a = GDArea.Tiles.Layers[3].A.GetCell(Coords.X, Coords.Y);
        int b = GDArea.Tiles.Layers[3].B.GetCell(Coords.X, Coords.Y);

        // Check if there is nothing to transfer. This also prevents Retransfering when an area is woken back up
        if (a == -1 && b == -1) { return; }

        // Set the values of the Ghost Terrain Cells
        GDArea.Tiles.Layers[4].A.SetCell(Coords.X, Coords.Y, a);
        GDArea.Tiles.Layers[4].B.SetCell(Coords.X, Coords.Y, b);

        // Clear the original cells
        GDArea.Tiles.Layers[3].A.SetCell(Coords.X, Coords.Y, -1);
        GDArea.Tiles.Layers[3].B.SetCell(Coords.X, Coords.Y, -1);
    }
}
