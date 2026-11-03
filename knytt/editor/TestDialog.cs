using Godot;
using System.Collections.Generic;
using YKnyttLib;

// What Juni starts with: the level's own start (DefaultSavegame.ini), or a test profile laid over it
public class TestDialog : ConfirmationDialog
{
    [Signal] public delegate void StartPickRequested();
    [Signal] public delegate void LevelChanged(string text, bool done);

    private static readonly Dictionary<string, string> chosen = new Dictionary<string, string>();
    private static readonly Color ERROR_COLOR = new Color(1, 0.4f, 0.4f);
    private const int POWERS = 13;
    private const int FLAGS = 10;

    public TestProfile Profile { get; private set; }
    public KnyttSave LastTest { get; set; }

    // What the form edits: Profile, or the level's start
    private TestProfile shown;
    public bool LevelShown => shown?.IsLevel ?? true;

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

    // The chosen profile, or the level's start
    public void open(bool level = false)
    {
        shown = level ? null : Profile;
        reopen();
    }

    // Shows the same one again, as it is now
    public void reopen()
    {
        findPlaced();
        showProfile();
        PopupCentered();
    }

    public void refresh()
    {
        if (Visible) { showProfile(); }
    }

    // Testing tests what's shown
    public void chooseShown()
    {
        choose(LevelShown ? null : shown.Name);
    }

    public void useAsLevelStart(TestProfile profile)
    {
        EmitSignal(nameof(LevelChanged), TestProfile.levelTextWith(world, profile), true);
    }

    public void setStart(KnyttPoint area, KnyttPoint position)
    {
        shown.setStart(area, position);
        shown.save();
    }

    private void showProfile()
    {
        filling = true;
        if (LevelShown) { shown = TestProfile.levelDefault(world); }
        bool profile = !shown.IsLevel;
        WindowTitle = profile ? "Test profiles" : "Level start";

        names = Names;
        var list = GetNode<OptionButton>("%Profiles");
        list.Clear();
        list.AddItem("Level default");
        foreach (string name in names) { list.AddItem(name); }
        list.Select(profile ? names.IndexOf(shown.Name) + 1 : 0);

        name_edit.Text = profile ? shown.Name : "";
        name_edit.Editable = profile;
        GetNode<Button>("%DeleteButton").Disabled = !profile;
        GetNode<Button>("%LastTestButton").Disabled = !profile || LastTest == null;
        GetNode<Button>("%UseButton").Disabled = !profile;
        GetNode<Button>("%ClearButton").Disabled = !profile || !shown.HasStart;

        var area = shown.Save.getArea();
        var position = shown.Save.getAreaPosition();
        GetNode<Label>("%StartLabel").Text = shown.HasStart ? $"x{area.x}y{area.y}  {position.x}, {position.y}" : "Level start";

        for (int i = 0; i < POWERS; i++) { GetNode<CheckBox>($"%Power{i}").Pressed = shown.Save.getPower(i); }
        for (int i = 0; i < FLAGS; i++) { GetNode<CheckBox>($"%Flag{i}").Pressed = shown.Save.getFlag(i); }

        shown.Save.getCollectables(out collected, out int spent);
        foreach (var grid in grids)
        {
            grid.Collected = collected;
            grid.Update();
        }
        coins_spent.Value = spent;

        // Keep the caret when the text is the same (an undo while typing)
        if (more_edit.Text != shown.More) { more_edit.Text = shown.More; }
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

    // The level's start goes to the editor, which keeps it undoable. Typing is one step until done
    private void saveProfile(bool done = true)
    {
        if (filling) { return; }
        if (shown.IsLevel) { EmitSignal(nameof(LevelChanged), shown.LevelText, done); }
        else { shown.save(); }
    }

    private void _on_Profiles_item_selected(int index)
    {
        choose(index == 0 ? null : names[index - 1]);
        shown = Profile;
        showProfile();
    }

    private void _on_NewButton_pressed()
    {
        choose(TestProfile.create(world, shown.IsLevel ? "Test" : shown.Name, shown.Save).Name);
        shown = Profile;
        showProfile();
        name_edit.GrabFocus();
        name_edit.SelectAll();
    }

    private void _on_DeleteButton_pressed()
    {
        shown.delete();
        choose(null);
        shown = null;
        showProfile();
    }

    private void _on_NameEdit_text_entered(string text)
    {
        _on_NameEdit_focus_exited();
    }

    private void _on_NameEdit_focus_exited()
    {
        if (shown.IsLevel) { return; }
        shown.rename(name_edit.Text);
        choose(shown.Name);
        shown = Profile;
        showProfile();
    }

    // Shows the level's start after, so the change is in view
    private void _on_UseButton_pressed()
    {
        useAsLevelStart(shown);
        shown = null;
        showProfile();
    }

    private void _on_LastTestButton_pressed()
    {
        shown.takeState(LastTest);
        shown.save();
        showProfile();
    }

    private void _on_PickButton_pressed()
    {
        Hide();
        EmitSignal(nameof(StartPickRequested));
    }

    private void _on_ClearButton_pressed()
    {
        shown.clearStart();
        shown.save();
        showProfile();
    }

    private void _on_Power_toggled(bool pressed, int power)
    {
        if (filling) { return; }
        shown.Save.setPower(power, pressed);
        saveProfile();
    }

    private void _on_Flag_toggled(bool pressed, int flag)
    {
        if (filling) { return; }
        shown.Save.setFlag(flag, pressed);
        saveProfile();
    }

    private void _on_Collectables_changed()
    {
        if (filling) { return; }
        shown.setCollectables(collected, (int)coins_spent.Value);
        saveProfile();
    }

    private void _on_CoinsSpent_value_changed(float value)
    {
        _on_Collectables_changed();
    }

    private void _on_MoreEdit_text_changed()
    {
        if (filling) { return; }
        bool valid = shown.setMore(more_edit.Text);
        var label = GetNode<Label>("%MoreLabel");
        if (!valid) { label.AddColorOverride("font_color", ERROR_COLOR); return; }
        label.RemoveColorOverride("font_color");
        saveProfile(done: false);
    }

    private void _on_MoreEdit_focus_exited()
    {
        saveProfile();
    }

    private void _on_TestDialog_visibility_changed()
    {
        if (!Visible && (shown?.IsLevel ?? false)) { saveProfile(); }
    }
}
