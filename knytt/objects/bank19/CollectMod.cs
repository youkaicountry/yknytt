using System.Collections.Generic;
using Godot;
using YKnyttLib;

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
        checkDoors(juni);
        foreach (var obj in parent.GDArea.Objects.findObjects(parent.ObjectID))
        {
            obj.QueueFree();
            obj.Deleted = true;
        }
    }

    private void checkDoors(Juni juni)
    {
        var doors = juni.Game.CurrentArea.Objects.findObjects(new List<KnyttPoint>() {
            new KnyttPoint(15, 31), new KnyttPoint(15, 32), new KnyttPoint(15, 33), 
            new KnyttPoint(15, 34), new KnyttPoint(15, 35), new KnyttPoint(15, 36) });
        foreach (Door door in doors)
        {
            door.gotKey();
        }
    }
}
