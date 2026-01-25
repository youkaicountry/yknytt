using Godot;

public partial class NoWall : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        if (IsQueuedForDeletion()) { return; }

        // Godot 4: TileMap API changed - GetCell → GetCellSourceId, SetCell takes Vector2I
        var coords = new Vector2I(Coords.X, Coords.Y);

        // Copy values of the cells
        int a = GDArea.Tiles.Layers[3].A.GetCellSourceId(0, coords);
        int b = GDArea.Tiles.Layers[3].B.GetCellSourceId(0, coords);

        // Check if there is nothing to transfer. This also prevents Retransfering when an area is woken back up
        if (a == -1 && b == -1) { return; }

        // Set the values of the Ghost Terrain Cells
        GDArea.Tiles.Layers[4].A.SetCell(0, coords, a);
        GDArea.Tiles.Layers[4].B.SetCell(0, coords, b);

        // Clear the original cells (-1 erases the cell)
        GDArea.Tiles.Layers[3].A.SetCell(0, coords, -1);
        GDArea.Tiles.Layers[3].B.SetCell(0, coords, -1);
    }
}
