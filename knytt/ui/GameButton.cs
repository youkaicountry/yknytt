using Godot;
using YKnyttLib;

public class GameButton : Button
{
    public KnyttWorld<string> World { get; private set; }

    public void initialize(Texture icon, KnyttWorld<string> world)
    {
        this.World = world;
        var main_container = GetNode("MainContainer");
        ((TextureRect)(main_container.GetNode("IconTexture"))).Texture = icon;
        var text_container = main_container.GetNode("TextContainer");
        ((Label)(text_container.GetNode("NameLabel"))).Text = string.Format("{0} {1}", world.Info.Name, world.Info.Author);
        ((Label)(text_container.GetNode("NameLabel"))).Text = world.Info.Description;
    }
}
