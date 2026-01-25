using Godot;

public partial class GDAreaLayer : Node2D
{
    public TileMap A { get; private set; }
    public TileMap B { get; private set; }

    uint a_col;
    uint b_col;

    public void initLayer(TileSet A, TileSet B)
    {
        this.A = this.GetNode("A") as TileMap;
        this.B = this.GetNode("B") as TileMap;

        this.A.TileSet = A;
        this.B.TileSet = B;
    }

    public void setTile(int x, int y, int tilenum, bool collisions)
    {
        TileMap umap;
        if (tilenum < 128) { umap = this.B; }
        else
        {
            tilenum -= 128;
            umap = this.A;
        }

        // Godot 4 TileMap API: SetCell(layer, coords, sourceId, atlasCoords, alternativeTile)
        // The collision handling needs to be done differently in Godot 4 via the TileSet
        umap.SetCell(0, new Vector2I(x, y), tilenum + (collisions ? 0 : 128));
    }

    public void deactivate()
    {
        // TODO: Godot 4 - TileMap collision is now controlled via TileSet physics layers
        // For now, we disable collision by setting the collision visibility mode
        // A proper solution would involve manipulating the TileSet's physics layers
        A.CollisionVisibilityMode = TileMap.VisibilityMode.ForceHide;
        B.CollisionVisibilityMode = TileMap.VisibilityMode.ForceHide;
        A.SetLayerEnabled(0, false);
        B.SetLayerEnabled(0, false);
    }

    public void activate()
    {
        A.CollisionVisibilityMode = TileMap.VisibilityMode.Default;
        B.CollisionVisibilityMode = TileMap.VisibilityMode.Default;
        A.SetLayerEnabled(0, true);
        B.SetLayerEnabled(0, true);
    }
}
