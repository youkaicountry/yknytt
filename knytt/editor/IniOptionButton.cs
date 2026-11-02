using Godot;

// The items are the values, the first one is not set. A value that isn't listed shows in one extra item.
// Values, when given, are what the items stand for: Floor is 1
public class IniOptionButton : OptionButton, IIniField
{
    [Signal] public delegate void Edited();
    [Signal] public delegate void Done();

    [Export] public string Section { get; set; } = "";
    [Export] public string Key { get; set; } = "";
    [Export] public string[] Values { get; set; } = new string[0];

    private int extra = -1;

    public string Value
    {
        get => Selected < Values.Length ? Values[Selected] : GetItemText(Selected);
        set
        {
            for (int i = 0; i < GetItemCount(); i++)
            {
                string item = i < Values.Length ? Values[i] : GetItemText(i);
                if (i != extra && item.ToLower() == value.ToLower()) { Select(i); return; }
            }
            if (extra < 0) { AddItem(value); extra = GetItemCount() - 1; }
            SetItemText(extra, value);
            Select(extra);
        }
    }

    public override void _Ready()
    {
        Connect("item_selected", this, nameof(selected));
    }

    private void selected(int index)
    {
        EmitSignal(nameof(Edited));
        EmitSignal(nameof(Done));
    }
}
