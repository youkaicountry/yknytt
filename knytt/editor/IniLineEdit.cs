using Godot;

// Edited as it's typed, done on Enter or when it loses focus
public class IniLineEdit : LineEdit, IIniField
{
    [Signal] public delegate void Edited();
    [Signal] public delegate void Done();

    [Export] public string Section { get; set; } = "";
    [Export] public string Key { get; set; } = "";

    public string Value
    {
        get => Text.Trim();
        set { if (value != Value) { Text = value; } }
    }

    public override void _Ready()
    {
        Connect("text_changed", this, nameof(changed));
        Connect("text_entered", this, nameof(entered));
        Connect("focus_exited", this, nameof(done));
    }

    private void changed(string text)
    {
        EmitSignal(nameof(Edited));
    }

    private void entered(string text)
    {
        done();
    }

    private void done()
    {
        EmitSignal(nameof(Done));
    }
}
