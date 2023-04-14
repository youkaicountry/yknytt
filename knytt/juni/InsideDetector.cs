using Godot;

public partial class InsideDetector : Area2D
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
    }

    public void _on_InsideDetector_body_exited(Node body)
    {
        IsInside = false;
    }
}
