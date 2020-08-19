using Godot;
using YKnyttLib;

public class GKnyttBackground : Sprite
{
    public void initialize(Texture t)
    {
        this.Texture = t;

        var size = t.GetSize();

        var px = KnyttArea<string>.AREA_WIDTH * GKnyttAssetBuilder.TILE_WIDTH;
        var py = KnyttArea<string>.AREA_HEIGHT * GKnyttAssetBuilder.TILE_HEIGHT;

        this.RegionRect = new Rect2(0f, 0f, px, py);

        //this.Scale = new Vector2(px/size.x, py/size.y);
    }
}
