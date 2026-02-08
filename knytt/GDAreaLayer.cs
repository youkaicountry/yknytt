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
        // Source 0 = tiles with collision, Source 1 = tiles without collision
        int sourceId = collisions ? 0 : 1;
        var atlasCoords = new Vector2I(tilenum % GDKnyttAssetManager.TILESET_WIDTH,
                                        tilenum / GDKnyttAssetManager.TILESET_WIDTH);
        umap.SetCell(0, new Vector2I(x, y), sourceId, atlasCoords);
    }

    public void deactivate()
    {
        A.SetLayerEnabled(0, false);
        B.SetLayerEnabled(0, false);
    }

    public void activate()
    {
        A.SetLayerEnabled(0, true);
        B.SetLayerEnabled(0, true);
    }
}
