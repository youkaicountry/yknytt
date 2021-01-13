public class GroundBlob : Muff
{
    protected override async void changeDirection(int dir)
    {
        base.changeDirection(dir);
        sprite.Play("turn");
        await ToSignal(sprite, "animation_finished");
        sprite.Play("walk");
    }
}
