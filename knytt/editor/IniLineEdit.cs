using Godot;
using System;

// Edited as it's typed, done on Enter or when it loses focus
public class IniLineEdit : LineEdit, IIniField
{
    [Signal] public delegate void Edited();
    [Signal] public delegate void Done();

    [Export] public string Section { get; set; } = "";
    [Export] public string Key { get; set; } = "";

    // The text, whenever it changes: a swatch beside it shows it
    public event Action<string> Shown;

    public string Value
    {
        get => Text.Trim();
        set
        {
            if (value != Value) { Text = value; }
            Shown?.Invoke(Value);
        }
    }

    // A value from a list or a swatch, as if typed. Not done yet while a colour is still being dragged
    public void choose(string value, bool done = true)
    {
        Text = value;
        changed(value);
        if (done) { this.done(); }
    }

    public override void _Ready()
    {
        Connect("text_changed", this, nameof(changed));
        Connect("text_entered", this, nameof(entered));
        Connect("focus_exited", this, nameof(done));
    }

    private void changed(string text)
    {
        Shown?.Invoke(Value);
        EmitSignal(nameof(Edited));
    }

    private void entered(string text)
    {
        done();
    }

    public void done()
    {
        EmitSignal(nameof(Done));
    }
}
