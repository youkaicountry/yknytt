using Godot;

public class Elemental : GDKnyttBaseObject
{
    bool exploded;
    Vector2 prev_juni_pos = Vector2.Zero;
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!exploded && Juni.manhattanDistance(Center) < 66 && GDArea.isIn(Juni.GlobalPosition, 18 + 14, 18 + 14))
        {
            exploded = true;
            sprite.Play("launch");
            GetNode<AudioStreamPlayer2D>("ExplodePlayer").Play();
            if (Juni.GlobalPosition.DistanceTo(prev_juni_pos) < 1000 * delta) { juniDie(Juni); } // can teleport into elemental with no harm
        }
        prev_juni_pos = Juni.GlobalPosition;
    }

    private void _on_AnimatedSprite_animation_finished()
    {
        if (!sprite.Animation.StartsWith("idle")) { sprite.Play($"idle{random.Next(4)}"); }
    }

    private void _on_Timer_timeout()
    {
        if (sprite.Animation.StartsWith("idle")) { sprite.Play("change"); }
    }
}
