using Godot;

public class InsideDetector : Area2D
{
    int inside = 0; // Number of sticky zones covering Juni
    public bool IsInside
    {
        get { return inside > 0; }
        set { inside += (value ? 1 : -1); }
    }

    public void _on_InsideDetector_body_entered(Node body)
    {
        IsInside = true;
        GD.Print($"INSIDE: {inside}");
    }

    public void _on_InsideDetector_body_exited(Node body)
    {
        IsInside = false;
        GD.Print($"OUT: {inside}");
    }
}
