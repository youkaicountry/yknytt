using Godot;

public class Elemental : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    
    public override void _Ready()
    {
        OrganicEnemy = true;
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play($"{ObjectID.y}_stopped");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        // TODO: don't know what "Flag 0" is, ignoring it
        if (!Juni.dead && Juni.manhattanDistance(Center) < 67 && !(
            Juni.GlobalPosition.x - GDArea.GlobalPosition.x < 18 + 16 ||
            Juni.GlobalPosition.y - GDArea.GlobalPosition.y < 18 + 16 ||
            GDArea.GlobalPosition.x + GDKnyttArea.Width - Juni.GlobalPosition.x < 18 + 16 ||
            GDArea.GlobalPosition.y + GDKnyttArea.Height - Juni.GlobalPosition.y < 18 + 16))
        {
            sprite.Play($"{ObjectID.y}_launching");
            GetNode<AudioStreamPlayer2D>($"{ObjectID.y}_ExplodePlayer").Play();
            juniDie(Juni);
        }
    }
    
    private void _on_AnimatedSprite_animation_finished()
    {
        if (!sprite.Animation.EndsWith("stopped")) { sprite.Play($"{ObjectID.y}_stopped"); }
    }

    private void _on_Timer_timeout()
    {
        if (sprite.Animation.EndsWith("stopped")) { sprite.Play($"{ObjectID.y}_walking"); }
    }
}
