using Godot;

public partial class NoWall : GDKnyttBaseObject
{
    public override void _Ready()
    {
        base._Ready();
        if (IsQueuedForDeletion()) { return; }

        // Copy values of the cells
        Vector2I c = new Vector2I(Coords.x, Coords.y);

        int a_src = GDArea.Tiles.tileMapA.GetCellSourceId(3, c);
        int b_src = GDArea.Tiles.tileMapB.GetCellSourceId(3, c);

        // Check if there is nothing to transfer. This also prevents Retransfering when an area is woken back up
        if (a_src != 1 && b_src != 1) { return; }

        var a = GDArea.Tiles.tileMapA.GetCellAtlasCoords(3, c);
        var b = GDArea.Tiles.tileMapB.GetCellAtlasCoords(3, c);

        // Clear the original cells
        if (a_src != -1) { GDArea.Tiles.tileMapA.EraseCell(3, c); }
        if (b_src != -1) { GDArea.Tiles.tileMapB.EraseCell(3, c); }
        
        // Set the values of the Ghost Terrain Cells
        if (a_src != -1) { GDArea.Tiles.tileMapA.SetCell(3, c, 0, a); }
        if (b_src != -1) { GDArea.Tiles.tileMapB.SetCell(3, c, 0, b); }

        /* Uncomment this if you find the bug
        // Replace solid tiles with clear ones (by changing source from 1 to 0)
        if (a_src != -1) { GDArea.Tiles.tileMapA.SetCell(3, c, 0, GDArea.Tiles.tileMapA.GetCellAtlasCoords(3, c)); }
        if (b_src != -1) { GDArea.Tiles.tileMapB.SetCell(3, c, 0, GDArea.Tiles.tileMapB.GetCellAtlasCoords(3, c)); }*/
    }
}
