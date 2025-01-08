using Godot;

public class CollectMod : Node2D
{
    [Export] NodePath collisionPath = new NodePath("CollisionShape2D");

    private GDKnyttBaseObject parent;

    public override void _Ready()
    {
        Node candidate = GetParent();
        while (!(candidate is GDKnyttBaseObject))
        {
            candidate = candidate.GetParent();
        }
        parent = candidate as GDKnyttBaseObject;

        if (parent.Juni.Powers.getCollectable(parent.ObjectID.y))
        {
            parent.QueueFree();
            parent.Deleted = true;
            return;
        }
        var area = GetNode<Area2D>("Area2D");
        area.AddChild(GetNode<Node2D>(collisionPath).Duplicate());
    }

    public virtual void _body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }
        juni.Powers.setCollectable(parent.ObjectID.y, true);
        juni.updateCollectables();
        parent.GDArea.playEffect(offset: GlobalPosition - parent.GDArea.GlobalPosition);
        juni.playSound("powerup");
        foreach (var obj in parent.GDArea.Objects.findObjects(parent.ObjectID))
        {
            obj.QueueFree();
            obj.Deleted = true;
        }
    }
}
