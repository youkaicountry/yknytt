using Godot;
using System.Text;
using System.Text.RegularExpressions;
using YKnyttLib;

public class NewLevelScreen : BasicScreen
{
    private const string TEMPLATE = "res://knytt/editor/templates/Template.knytt.bin";
    private const string TEMPLATE_PLUS = "res://knytt/editor/templates/Template+.knytt.bin";
    private const string INVALID_CHARACTERS = "\\/:*?\"<>|";

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
        string path = LevelSelection.InstallDirectory.PlusFile($"{author} - {name}.knytt.bin");

        if (name == "" || author == "") { showError("Enter a level name and author"); return; }
        if ((name + author).IndexOfAny(INVALID_CHARACTERS.ToCharArray()) != -1) { showError($"Names can't contain {INVALID_CHARACTERS}"); return; }
        if (new File().FileExists(path)) { showError("This level already exists"); return; }

        var bin = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(plus ? TEMPLATE_PLUS : TEMPLATE));
        bin.RootDirectory = $"{author} - {name}";
        string ini = GDKnyttAssetManager.loadTextFile(bin.GetFile("World.ini"));
        ini = setIniValue(ini, "Name", name);
        ini = setIniValue(ini, "Author", author);
        ini = setIniValue(ini, "Format", plus ? "4" : "2");
        bin.SetFile("World.ini", Encoding.GetEncoding(1252).GetBytes(ini));

        GDKnyttAssetManager.ensureDirExists(path.GetBaseDir());
        var f = new File();
        if (f.Open(path, File.ModeFlags.Write) != Error.Ok) { showError($"Can't write {path}"); return; }
        f.StoreBuffer(bin.Pack());
        f.Close();

        var world = new GDKnyttWorldImpl();
        world.setBinMode(bin);
        world.setDirectory(path, bin.RootDirectory);
        world.loadWorldConfig(GDKnyttAssetManager.loadTextFileRaw(world.getWorldData("World.ini")));
        LevelEditor.open(GetTree(), world);
    }

    private static string setIniValue(string ini, string key, string value)
    {
        return Regex.Replace(ini, $@"^{key}=[^\r\n]*", m => $"{key}={value}", RegexOptions.Multiline);
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
