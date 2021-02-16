using Godot;

public class AttackBlock : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    private CollisionShape2D shape;

    private readonly float[] scales = { 1, 1.5f, 2, 3, 4, 6, 8, 10, 12, 15, 18, 21, 25, 30, 35, 36, 35 };
    private readonly float[] positions = { 71, 70.5f, 70, 69, 68, 66, 64, 62, 60, 57, 54, 51, 47, 42, 37, 36, 37 };

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        shape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

    private void _on_DistanceMod_EnterEvent()
    {
        // TODO: rock particles
        shape.SetDeferred("disabled", false);
        GetNode<RawAudioPlayer2D>("OpenPlayer").Play();
    }

    private void _on_AnimatedSprite_frame_changed()
    {
        shape.Position = new Vector2(13, positions[sprite.Frame]);
        shape.Scale = new Vector2(1, scales[sprite.Frame]);
    }
}
