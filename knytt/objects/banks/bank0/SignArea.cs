using Godot;

public class SignArea : GDKnyttBaseObject
{
    protected Sign attachedSign = null;

    protected Sign findSign()
    {
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (var knytt_object in layer.GetChildren())
            {
                if (knytt_object is Sign sign && sign.ObjectID.y + 12 == ObjectID.y)
                {
                    return sign;
                }
            }
        }
        return null;
    }

    private void _on_Area2D_body_entered(Node body)
    {
        if (attachedSign == null) { attachedSign = findSign(); }
        attachedSign.OnArea2DBodyEntered(body);
    }

    private void _on_Area2D_body_exited(Node body)
    {
        attachedSign.OnArea2DBodyExited(body);
    }
}
