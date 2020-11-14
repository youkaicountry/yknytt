using Godot;

public class Sign : GDKnyttBaseObject
{
    public int junis = 0;

    public override void _Ready()
    {
        string letter = "";
        switch (ObjectID.y)
        {
            case 17: letter = "A"; break;
            case 18: letter = "B"; break;
            case 19: letter = "C"; break;
        }

        string text = GDArea.Area.getExtraData($"Sign({letter})");
        if (text == null) { text = "<SIGN TEXT MISSING>"; }
        GetNode<Label>("SignRect/Label").Text = text;
        adjustSign();
    }

    protected void adjustSign()
    {
        var sign_rect = GetNode<Control>("SignRect");
        var x_pos = sign_rect.RectPosition.x;
        var y_pos = sign_rect.RectPosition.y;
        var size = sign_rect.RectSize;

        // TODO: original game doesn't overlap object area when showing sign
        if (Position.x + x_pos < 0) { x_pos = -Position.x; }
        if (Position.y + y_pos < 0) { y_pos = -Position.y; }
        if (Position.x + x_pos + size.x > 600) { x_pos = 600 - Position.x - size.x; }
        if (Position.y + y_pos + size.y > 240) { y_pos = 240 - Position.y - size.y; }

        sign_rect.RectPosition = new Vector2(x_pos, y_pos);
    }

    public void OnArea2DBodyEntered(Node body)
    {
        if (!(body is Juni)) { return; }
        if (junis == 0) { GetNode<AnimationPlayer>("AnimationPlayer").Play("FadeIn"); }
        junis++;
    }

    public void OnArea2DBodyExited(Node body)
    {
        if (!(body is Juni)) { return; }
        junis--;
        if (junis > 0) { return; }
        GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("FadeIn");
        junis = 0;
    }

    public void _on_Area2D_body_entered(Node body)
    {
        OnArea2DBodyEntered(body);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        OnArea2DBodyExited(body);
    }
}
