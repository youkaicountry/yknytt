using Godot;

public class GhostMarker : GhostObject
{
    protected override void _InvReady()
    {
        flickerMax = .2f;
        flickerMin = .8f;
        flip_time = .03f;
        change_fraction = 16f;

        GetNode<AnimatedSprite>("AnimatedSprite").Frame = ObjectID.y - 18;
    }
}
