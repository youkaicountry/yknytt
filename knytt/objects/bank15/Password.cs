using Godot;

public class Password : GDKnyttBaseObject
{
    private AnimatedSprite sprite;
    protected int current_char = 13;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    private async void _on_Area2D_body_entered(object body)
    {
        if (!(body is Juni)) { return; }

        current_char = current_char == 21 ? 13 :
                       current_char < 21 ? current_char + 1 : current_char;

        if (checkPassword()) { destroyWalls(); }

        sprite.Play("press");
        await ToSignal(sprite, "animation_finished");
        sprite.Play(current_char.ToString());
    }

    // TODO: refactor: make this class abstract, move implementation to derived SymbolPassword
    protected virtual bool checkPassword()
    {
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (var knytt_object in layer.GetChildren())
            {
                if (knytt_object is Password password && password.GetType() == typeof(Password))
                {
                    if (!password.IsCorrect()) { return false; }
                }
            }
        }
        return true;
    }

    protected void destroyWalls()
    {
        bool objects_destroyed = false;
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (var node in layer.GetChildren())
            {
                if (node is LockBlock block)
                {
                    block.QueueFree();
                    objects_destroyed = true;
                }
            }
        }

        if (objects_destroyed) { GetNode<AudioStreamPlayer2D>("OpenPlayer").Play(); }
    }

    public bool IsCorrect()
    {
        return current_char == ObjectID.y;
    }
}
