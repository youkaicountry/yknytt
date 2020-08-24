using Godot;
using YKnyttLib;

public class InfoScreen : CanvasLayer
{
    public KnyttWorld<string> World { get; private set; }

    public void initialize(KnyttWorld<string> world)
    {
        this.World = world;
        var info = GDKnyttAssetManager.loadTexture(world.WorldDirectory + "/Info.png");
        GetNode<TextureRect>("InfoRect").Texture = info;
    }

    public void _on_BackButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        this.QueueFree();
    }
}
