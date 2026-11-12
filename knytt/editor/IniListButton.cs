using Godot;

// Clicks on the arrow its field draws: values from the level to pick from. Typing still works
public class IniListButton : MenuButton
{
    // What LevelEditor.choices lists: character, cutscene, flag, warpflag, artifact, image, attach
    [Export] public string List { get; set; } = "";
    [Export] public NodePath Field { get; set; } = new NodePath();

    private LineEdit field;

    public override void _Ready()
    {
        field = GetNode<LineEdit>(Field);
        GetPopup().Connect("about_to_show", this, nameof(fill));
        GetPopup().Connect("index_pressed", this, nameof(chosen));
    }

    private void fill()
    {
        var popup = GetPopup();
        popup.Clear();
        foreach (var choice in ((LevelEditor)Owner).choices(List))
        {
            if (choice.Value == null) { popup.AddSeparator(choice.Text); continue; }
            popup.AddRadioCheckItem(choice.Text);
            int index = popup.GetItemCount() - 1;
            popup.SetItemMetadata(index, choice.Value);
            popup.SetItemChecked(index, choice.Value.ToLower() == field.Text.Trim().ToLower());
        }
        if (popup.GetItemCount() == 0)
        {
            popup.AddItem("None in this level");
            popup.SetItemDisabled(0, true);
        }
    }

    private void chosen(int index)
    {
        string value = (string)GetPopup().GetItemMetadata(index);
        ((LevelEditor)Owner).listChosen(List, value);
        if (field is IniLineEdit edit) { edit.choose(value); return; }
        field.Text = value;
        field.EmitSignal("text_changed", value);
    }
}
