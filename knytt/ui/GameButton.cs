using Godot;
using YKnyttLib;

public class GameButton : Button
{
    public KnyttWorld<string> World { get; private set; }

    public void initialize(Texture icon, KnyttWorld<string> world)
    {
        this.World = world;
        GetNode<TextureRect>("MainContainer/IconTexture").Texture = icon;
        GetNode<Label>("MainContainer/TextContainer/NameLabel").Text = string.Format("{0} {1}", world.Info.Name, world.Info.Author);
        GetNode<Label>("MainContainer/TextContainer/DescriptionLabel").Text = world.Info.Description;
    }
}
