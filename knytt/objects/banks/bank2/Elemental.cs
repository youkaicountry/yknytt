using Godot;

public class Elemental : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Play($"{ObjectID.y}_stopped");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        // TODO: don't know what "Flag 0" is, ignoring it
        if (!Juni.dead && Juni.manhattanDistance(Center) < 67 && !(
            Juni.ApparentPosition.x - GDArea.GlobalPosition.x < 18 + 16 ||
            Juni.ApparentPosition.y - GDArea.GlobalPosition.y < 18 + 16 ||
            GDArea.GlobalPosition.x + GDKnyttArea.Width - Juni.ApparentPosition.x < 18 + 16 ||
            GDArea.GlobalPosition.y + GDKnyttArea.Height - Juni.ApparentPosition.y < 18 + 16))
        {
            sprite.Play($"{ObjectID.y}_launching");
            GetNode<AudioStreamPlayer2D>($"{ObjectID.y}_ExplodePlayer").Play();
            Juni.die();
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
