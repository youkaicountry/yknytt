using Godot;
using YKnyttLib;

public partial class SignArea : GDKnyttBaseObject
{
    protected Sign attachedSign = null;

    private void _on_Area2D_body_entered(Node body)
    {
        attachedSign = attachedSign ?? GDArea.Objects.findObject(new KnyttPoint(0, ObjectID.y - 12)) as Sign;
        attachedSign?.OnArea2DBodyEntered(body);
    }

    private void _on_Area2D_body_exited(Node body)
    {
        attachedSign?.OnArea2DBodyExited(body);
    }
}
