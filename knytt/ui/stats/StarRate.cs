using Godot;

public class StarRate : HBoxContainer
{
    [Signal] public delegate void RateEvent(int n);
    [Signal] public delegate void HoverEvent(string hint);

    private int _rate;
    public int Rate { set { _rate = value; update(-2); } }

    private void _on_Star_mouse_entered(int n)
    {
        update(n);
    }

    private void _on_Star_mouse_exited(int n)
    {
        update(-1);
    }

    private void update(int n)
    {
        for (int i = 1; i <= 10; i++)
        {
            GetNode<TextureRect>($"Star{i}").Modulate = 
                i <= n ? new Color("#EE0000") : 
                i <= _rate ? new Color("#005C00") : new Color("#c0c080");
        }
        GetNode<TextureRect>($"Remove").Modulate = n == 0 && _rate > 0 ? new Color("#EE0000") : 
                _rate > 0 ? new Color("#EECC22") : new Color("#c0c080");
        if (n != -2)
        {
            EmitSignal(nameof(HoverEvent), 
                n == 0 && _rate > 0 ? "Remove your rating for this level" : 
                n > 0 ? $"Rate this level as {n / 2f:0.0} (out of 5)" : null);
        }
    }

    private void _on_Star_gui_input(InputEvent @event, int n)
    {
        // TODO: don't know how to recieve mouse events when left mouse button is pressed
        // if (@event is InputEventMouseButton st && st.Pressed) { GD.Print("workaround"); }
        bool released = @event is InputEventScreenTouch && !(@event as InputEventScreenTouch).Pressed;
        if (released) { EmitSignal(nameof(RateEvent), n); }
    }
}
