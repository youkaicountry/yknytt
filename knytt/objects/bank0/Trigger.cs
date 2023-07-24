using Godot;
using YKnyttLib;

public class Trigger : Switch
{
    KnyttTrigger trigger;

    public override void _Ready()
    {
        @switch = trigger = GDArea.Area.ExtraData != null ? 
            new KnyttTrigger(GDArea.Area, Coords, (KnyttSwitch.SwitchID)(ObjectID.y - 32)) :
            new KnyttTrigger(GDArea.Area.Position, (KnyttSwitch.SwitchID)(ObjectID.y - 32));
        base._Ready();
    }

    protected override void _execute(Juni juni)
    {
        juni.juniInput.SwitchHeld = true;
        
        if (sound != null) { juni.playSound(sound); }

        if (!trigger.ObjectID.Equals(new KnyttPoint(0, 0)))
        {
            var spawn_points = GDArea.Objects.findObjects(new KnyttPoint(0, ObjectID.y + 10));
            if (spawn_points.Count > 0)
            {
                foreach (var spawn_point in spawn_points) { addObject(spawn_point.Coords); }
            }
            else
            {
                addObject(trigger.AbsolutePosition);

                if (trigger.Effect)
                {
                    var offset = new Vector2(trigger.EffectOffset.x, trigger.EffectOffset.y);
                    GDArea.playEffect(trigger.AbsolutePosition, offset);
                }
            }
        }

        var delete_points = GDArea.Objects.findObjects(new KnyttPoint(0, ObjectID.y + 13));
        foreach (var delete_point in delete_points)
        {
            (delete_point as DeletePoint).activate();
        }
    }

    private void addObject(KnyttPoint coords)
    {
        var bundle = GDKnyttObjectFactory.buildKnyttObject(trigger.ObjectID);
        if (bundle != null) { Layer.addObject(coords, bundle); }
    }
}
