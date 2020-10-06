using System.Collections.Generic;
using Godot;

public class Door : GDKnyttBaseObject
{
    struct DoorInfo
    {
        public string Anim;
        public YKnyttLib.JuniValues.PowerNames Power;

        public DoorInfo(string anim, YKnyttLib.JuniValues.PowerNames power)
        {
            Anim = anim;
            Power = power;
        }
    }

    static Dictionary<int, DoorInfo> id2info;

    static Door()
    {
        id2info = new Dictionary<int, DoorInfo>();
        id2info[27] = new DoorInfo("RedDoor", YKnyttLib.JuniValues.PowerNames.RedKey);
        id2info[28] = new DoorInfo("YellowDoor", YKnyttLib.JuniValues.PowerNames.YellowKey);
        id2info[29] = new DoorInfo("BlueDoor", YKnyttLib.JuniValues.PowerNames.BlueKey);
        id2info[30] = new DoorInfo("PurpleDoor", YKnyttLib.JuniValues.PowerNames.PurpleKey);
    }

    DoorInfo door_info;
    Area2D open_area;
    bool opening = false;

    public override void _Ready()
    {
        open_area = GetNode<Area2D>("OpenArea");
    }

    protected override void _impl_initialize()
    {
        door_info = id2info[ObjectID.y];
        GetNode<AnimatedSprite>("AnimatedSprite").Animation = door_info.Anim;
        if (Juni.Powers.getPower(door_info.Power)) { GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").Disabled = true; }
    }

    protected override void _impl_process(float delta)
    {
        foreach(var body in open_area.GetOverlappingBodies())
        {
            if (opening || !(body is Juni)) { continue; }
            if (Juni.Powers.getPower(door_info.Power)) { open(); }
        }
    }

    private async void open()
    {
        opening = true;
        GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").Disabled = true;
        GetNode<AudioStreamPlayer2D>("DoorPlayer2D").Play();
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.Play();
        await ToSignal(player, "animation_finished");
        QueueFree();
    }
}
