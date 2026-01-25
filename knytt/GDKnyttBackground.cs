using Godot;
using YKnyttLib;

public partial class GDKnyttBackground : Sprite2D
{
    public void initialize(Texture2D t)
    {
        Texture = t;

        var px = KnyttArea.AREA_WIDTH * GDKnyttAssetManager.TILE_WIDTH;
        var py = KnyttArea.AREA_HEIGHT * GDKnyttAssetManager.TILE_HEIGHT;

        RegionRect = new Rect2(0f, 0f, px, py);
    }
}
