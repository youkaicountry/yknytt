using Godot;
using YKnyttLib;

public class GDKnyttBackground : Sprite
{
    public void initialize(Texture t)
    {
        Texture = t;

        var px = KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH;
        var py = KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT;

        RegionRect = new Rect2(0f, 0f, px, py);
    }
}
