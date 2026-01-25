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

        umap.SetCell(x, y, tilenum + (collisions ? 0 : 128));
    }

    public void deactivate()
    {
        // TODO: Godot 4 TileMap no longer has CollisionLayer property
        // Need to migrate to TileMapLayer or handle via TileSet physics layers
        // For now, use CollisionEnabled property on the tilemap
        A.CollisionEnabled = false;
        B.CollisionEnabled = false;
    }

    public void activate()
    {
        A.CollisionEnabled = true;
        B.CollisionEnabled = true;
    }
}
