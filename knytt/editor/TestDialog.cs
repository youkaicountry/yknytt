using Godot;
using System.Collections.Generic;
using YKnyttLib;

// Test profiles: the state Juni starts a test with
public class TestDialog : ConfirmationDialog
{
    [Signal] public delegate void StartPickRequested();

    private static readonly Dictionary<string, string> chosen = new Dictionary<string, string>();
    private static readonly Color ERROR_COLOR = new Color(1, 0.4f, 0.4f);
    private const int POWERS = 13;
    private const int FLAGS = 10;

    public TestProfile Profile { get; private set; }
    public KnyttSave LastTest { get; set; }

    private GDKnyttWorldImpl world;
    private List<string> names;
    private Dictionary<int, KnyttPoint> placed = new Dictionary<int, KnyttPoint>();
    private bool[] collected;
    private bool filling;

    private CollectGrid[] grids;
    private LineEdit name_edit;
    private SpinBox coins_spent;
    private TextEdit more_edit;

    public override void _Ready()
    {
        GetOk().Text = "Test";
        GetCancel().Text = "Close";
        grids = new[] { GetNode<CollectGrid>("%Creatures"), GetNode<CollectGrid>("%Coins"), GetNode<CollectGrid>("%Artifacts") };
        name_edit = GetNode<LineEdit>("%NameEdit");
        coins_spent = GetNode<SpinBox>("%CoinsSpent");
        more_edit = GetNode<TextEdit>("%MoreEdit");
        foreach (var grid in grids)
        {
            grid.Placed = placed;
            grid.Describe = describe;
        }
    }

    public void init(GDKnyttWorldImpl world)
    {
        this.world = world;
        choose(chosen.TryGetValue(world.WorldDirectoryName, out string name) ? name : null);
    }

    public List<string> Names => TestProfile.names(world);

    // No name is the level's own start
    public void choose(string name)
    {
        Profile = name == null ? null : TestProfile.load(world, name);
        chosen[world.WorldDirectoryName] = Profile?.Name;
    }

    public void open()
    {
        findPlaced();
        showProfile();
        PopupCentered();
    }

    public void setStart(KnyttPoint area, KnyttPoint position)
    {
        Profile.setStart(area, position);
        Profile.save();
    }

    private void showProfile()
    {
        filling = true;
        var shown = Profile ?? TestProfile.levelDefault(world);
        bool editable = Profile != null;

        names = Names;
        var list = GetNode<OptionButton>("%Profiles");
        list.Clear();
        list.AddItem("Level default");
        foreach (string name in names) { list.AddItem(name); }
        list.Select(names.IndexOf(Profile?.Name) + 1);

        name_edit.Text = Profile?.Name ?? "";
        name_edit.Editable = editable;
        GetNode<Button>("%DeleteButton").Disabled = !editable;
        GetNode<Button>("%LastTestButton").Disabled = !editable || LastTest == null;
        GetNode<Button>("%PickButton").Disabled = !editable;
        GetNode<Button>("%ClearButton").Disabled = !editable || !Profile.HasStart;

        var area = shown.Save.getArea();
        var position = shown.Save.getAreaPosition();
        GetNode<Label>("%StartLabel").Text = shown.HasStart ? $"x{area.x}y{area.y}  {position.x}, {position.y}" : "Level start";

        for (int i = 0; i < POWERS; i++)
        {
            var power = GetNode<CheckBox>($"%Power{i}");
            power.Pressed = shown.Save.getPower(i);
            power.Disabled = !editable;
        }
        for (int i = 0; i < FLAGS; i++)
        {
            var flag = GetNode<CheckBox>($"%Flag{i}");
            flag.Pressed = shown.Save.getFlag(i);
            flag.Disabled = !editable;
        }

        shown.Save.getCollectables(out collected, out int spent);
        foreach (var grid in grids)
        {
            grid.Collected = collected;
            grid.MouseFilter = editable ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
            grid.Update();
        }
        coins_spent.Value = spent;
        coins_spent.Editable = editable;

        more_edit.Text = shown.More;
        more_edit.Readonly = !editable;
        GetNode<Label>("%MoreLabel").RemoveColorOverride("font_color");
        filling = false;
    }

    // The first of each collectable placed in the level
    private void findPlaced()
    {
        placed.Clear();
        foreach (var area in world.Map.Values)
        {
            if (area.Empty) { continue; }
            foreach (var layer in area.ObjectLayers)
            {
                for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
                {
                    for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                    {
                        var id = layer.getObjectID(x, y);
                        if (id.x == 19 && id.y != 0 && !placed.ContainsKey(id.y)) { placed[id.y] = area.Position; }
                    }
                }
            }
        }
    }

    private string describe(int id)
    {
        string name = id <= 50 ? $"Creature {id}" :
                      id <= 150 ? $"Coin {id - 50}" :
                      $"Artifact {(id - 151) / 7 + 1}, number {(id - 151) % 7 + 1}";
        return placed.TryGetValue(id, out var area) ? $"{name}\n19:{id}, at x{area.x}y{area.y}" : $"{name}\n19:{id}, not in the level";
    }

    private void saveProfile()
    {
        if (filling || Profile == null) { return; }
        Profile.save();
    }

    private void _on_Profiles_item_selected(int index)
    {
        choose(index == 0 ? null : names[index - 1]);
        showProfile();
    }

    private void _on_NewButton_pressed()
    {
        var from = Profile ?? TestProfile.levelDefault(world);
        choose(TestProfile.create(world, Profile?.Name ?? "Test", from.Save).Name);
        showProfile();
        name_edit.GrabFocus();
        name_edit.SelectAll();
    }

    private void _on_DeleteButton_pressed()
    {
        Profile.delete();
        choose(null);
        showProfile();
    }

    private void _on_NameEdit_text_entered(string text)
    {
        _on_NameEdit_focus_exited();
    }

    private void _on_NameEdit_focus_exited()
    {
        if (Profile == null) { return; }
        Profile.rename(name_edit.Text);
        choose(Profile.Name);
        showProfile();
    }

    private void _on_LastTestButton_pressed()
    {
        Profile.takeState(LastTest);
        Profile.save();
        showProfile();
    }

    private void _on_PickButton_pressed()
    {
        Hide();
        EmitSignal(nameof(StartPickRequested));
    }

    private void _on_ClearButton_pressed()
    {
        Profile.clearStart();
        Profile.save();
        showProfile();
    }

    private void _on_Power_toggled(bool pressed, int power)
    {
        if (filling) { return; }
        Profile.Save.setPower(power, pressed);
        saveProfile();
    }

    private void _on_Flag_toggled(bool pressed, int flag)
    {
        if (filling) { return; }
        Profile.Save.setFlag(flag, pressed);
        saveProfile();
    }

    private void _on_Collectables_changed()
    {
        if (filling) { return; }
        Profile.setCollectables(collected, (int)coins_spent.Value);
        saveProfile();
    }

    private void _on_CoinsSpent_value_changed(float value)
    {
        _on_Collectables_changed();
    }

    private void _on_MoreEdit_text_changed()
    {
        if (filling) { return; }
        bool valid = Profile.setMore(more_edit.Text);
        var label = GetNode<Label>("%MoreLabel");
        if (!valid) { label.AddColorOverride("font_color", ERROR_COLOR); return; }
        label.RemoveColorOverride("font_color");
        saveProfile();
    }
}
