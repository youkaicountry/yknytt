using Godot;

// Checked writes On, unchecked removes the key. On is False for keys that default to True
public class IniCheckBox : CheckBox, IIniField
{
    [Signal] public delegate void Edited();
    [Signal] public delegate void Done();

    [Export] public string Section { get; set; } = "";
    [Export] public string Key { get; set; } = "";
    [Export] public string On { get; set; } = "True";

    public string Value
    {
        get => Pressed ? On : "";
        set => SetPressedNoSignal(value.ToLower() == On.ToLower());
    }

    public override void _Ready()
    {
        Connect("toggled", this, nameof(toggled));
    }

    private void toggled(bool pressed)
    {
        EmitSignal(nameof(Edited));
        EmitSignal(nameof(Done));
    }
}
