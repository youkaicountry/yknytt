using Godot;

public abstract class CollectionInfo : ItemInfo
{
    public abstract string IconFilename { get; }

    public override void _Ready()
    {
        var world = (FindParent("GKnyttGame") as GDKnyttGame).GDWorld.KWorld;
        if (world.worldFileExists(IconFilename))
        {
            GetNode<Sprite>("Sprite").Offset = new Vector2(0, 2);
            GetNode<Sprite>("Sprite").Texture = world.getWorldTexture(IconFilename) as Texture;
        }
    }
}
