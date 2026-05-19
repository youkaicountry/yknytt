using Godot;

public class AttackBlock : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    private CollisionPolygon2D shape;

    private readonly float[] scales = { 1, 1.5f, 2, 3, 4, 6, 8, 10, 12, 15, 18, 21, 25, 30, 35, 36, 35 };

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        shape = GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D");
    }

    private void _on_DistanceMod_EnterEvent()
    {
        // TODO: rock particles
        shape.SetDeferred("disabled", false);
        GetNode<AudioStreamPlayer2D>("OpenPlayer").Play();
    }

    private void _on_AnimatedSprite_frame_changed()
    {
        shape.Scale = new Vector2(1, scales[sprite.Frame]);
    }
}
