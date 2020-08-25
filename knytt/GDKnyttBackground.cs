using Godot;
using YKnyttLib;

public class GDKnyttBackground : Sprite
{
    public void initialize(Texture t)
    {
        this.Texture = t;

        var size = t.GetSize();

        var px = KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH;
        var py = KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT;

        this.RegionRect = new Rect2(0f, 0f, px, py);

        //this.Scale = new Vector2(px/size.x, py/size.y);
    }
}
