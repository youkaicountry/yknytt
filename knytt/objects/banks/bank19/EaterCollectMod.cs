using Godot;

public partial class EaterCollectMod : CollectMod
{
    public override async void _body_entered(Node body)
    {
        GetNode<AudioStreamPlayer2D>("../Player").Play();
        var sprite = GetNode<AnimatedSprite2D>("../AnimatedSprite2D");
        sprite.Play("eat");
        await ToSignal(sprite, "animation_finished");
        base._body_entered(body);
    }
}
