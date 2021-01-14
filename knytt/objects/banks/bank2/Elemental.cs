using Godot;

public class Elemental : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!Juni.dead && Juni.manhattanDistance(Center) < 67 && GDArea.isIn(Juni.GlobalPosition, 18 + 14, 18 + 14))
        {
            sprite.Play("launch");
            GetNode<AudioStreamPlayer2D>("ExplodePlayer").Play();
            juniDie(Juni);
        }
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
