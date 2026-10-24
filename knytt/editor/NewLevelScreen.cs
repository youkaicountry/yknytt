using Godot;
using YKnyttLib;

public class NewLevelScreen : BasicScreen
{
    private const string TEMPLATE = "res://knytt/editor/templates/Template.knytt.bin";
    private const string TEMPLATE_PLUS = "res://knytt/editor/templates/Template+.knytt.bin";

    public override void _Ready()
    {
        base._Ready();
        initFocus();
    }

    public override void initFocus()
    {
        GetNode<LineEdit>("%NameEdit").GrabFocus();
    }

    private void create()
    {
        string name = GetNode<LineEdit>("%NameEdit").Text.Trim();
        string author = GetNode<LineEdit>("%AuthorEdit").Text.Trim();
        bool plus = GetNode<CheckBox>("%PlusCheckBox").Pressed;

        string error = WorldName.check(name, author);
        if (error != null) { showError(error); return; }

        var world = new GDKnyttWorldImpl();
        world.setBinMode(new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(plus ? TEMPLATE_PLUS : TEMPLATE)));
        world.setWorldValue("Format", plus ? "4" : "2");
        world.loadWorldMap(new System.IO.MemoryStream(world.getWorldData("Map.bin")));

        var result = world.saveAs(WorldName.path(name, author), name, author);
        if (result != Error.Ok) { showError($"Can't save the level ({result})"); return; }
        LevelEditor.open(GetTree(), world, created: true);
    }

    private void showError(string message)
    {
        GetNode<Label>("%ErrorLabel").Text = message;
    }

    private void _on_CreateButton_pressed()
    {
        ClickPlayer.Play();
        create();
    }

    private void _on_Edit_text_entered(string text)
    {
        create();
    }
}
