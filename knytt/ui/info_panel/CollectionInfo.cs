using Godot;

public abstract partial class CollectionInfo : ItemInfo
{
    public abstract string IconFilename { get; }

    public override void _Ready()
    {
        var world = (FindParent("GKnyttGame") as GDKnyttGame).GDWorld.KWorld;
        if (world.worldFileExists(IconFilename))
        {
            GetNode<Sprite2D>("Sprite2D").Offset = new Vector2(0, 2);
            GetNode<Sprite2D>("Sprite2D").Texture = world.getWorldTexture(IconFilename) as Texture2D;
        }
    }
}
