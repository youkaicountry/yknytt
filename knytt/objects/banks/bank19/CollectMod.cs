using Godot;

public class CollectMod : Node2D
{
    [Export] NodePath collisionPath = new NodePath("CollisionShape2D");

    private GDKnyttBaseObject parent;

    public override void _Ready()
    {
        parent = GetParent<GDKnyttBaseObject>();
        if (parent.Juni.Powers.getCollectable(parent.ObjectID.y))
        {
            parent.QueueFree();
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
        juni.showEffect(effect: true, sound: "PowerUp");
        parent.QueueFree();
    }
}
