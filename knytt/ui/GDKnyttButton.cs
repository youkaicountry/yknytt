using Godot;

public class GDKnyttButton : Button
{
    [Signal] public delegate void ShowHint();

    [Export] public string hint;

    protected void _on_GDKnyttButton_focus_entered()
    {
        if (hint != null) { EmitSignal(nameof(ShowHint), hint); }
    }

    protected void _on_GDKnyttButton_focus_exited()
    {
        if (hint != null) { EmitSignal(nameof(ShowHint), (string)null); }
    }

    protected void _on_GDKnyttButton_mouse_entered()
    {
        if (hint != null) { EmitSignal(nameof(ShowHint), hint); }
    }

    protected void _on_GDKnyttButton_mouse_exited()
    {
        if (hint != null && !HasFocus()) { EmitSignal(nameof(ShowHint), (string)null); }
    }
}
