using Godot;

public class GDAreaLayer : Node2D
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

    public void setTile(int x, int y, int tilenum)
    {
        TileMap umap;
        if (tilenum < 128) { umap = this.B; }
        else
        {
            tilenum -= 128;
            umap = this.A;
        }

        umap.SetCell(x, y, tilenum);
    }

    public void deactivate()
    {
        a_col = A.CollisionLayer;
        b_col = B.CollisionLayer;
        A.CollisionLayer = 0;
        B.CollisionLayer = 0;
    }

    public void activate()
    {
        A.CollisionLayer = a_col;
        B.CollisionLayer = b_col;
    }
}
