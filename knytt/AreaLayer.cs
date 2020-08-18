using Godot;

public class AreaLayer : Node2D
{
    public TileMap A { get; private set; }
    public TileMap B { get; private set; }

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
}
