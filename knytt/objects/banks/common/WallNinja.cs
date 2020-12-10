using Godot;

public class WallNinja : Muff
{
    [Export] string bulletScene;
    [Export] int bulletVelocity;
    [Export] int[] shotDirections;
    [Export] bool randomDirection;
    [Export] int bulletGravity;
    [Export] Vector2 shotPosition;

    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, bulletScene,
            (p, i) => 
            {
                p.Translate(shotPosition);
                p.VelocityMMF2 = bulletVelocity;
                p.DirectionMMF2 = randomDirection ? random.NextElement(shotDirections) : shotDirections[i];
                p.GravityMMF2 = bulletGravity;
            });
    }

    private async void _on_ShotTimer_timeout_ext()
    {
        var old_speed = speed;
        speed = 0;

        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        sprite.Play("prepare");
        await ToSignal(sprite, "animation_finished");
        
        for (int i = 0; i < (randomDirection ? 1 : shotDirections.Length); i++)
        {
            GDArea.Bullets.Emit(this, i);
        }
        
        sprite.Play("aftershot");
        await ToSignal(sprite, "animation_finished");
        
        speed = old_speed;
        _on_DirectionTimer_timeout();
    }

    protected override void changeDirection(int dir)
    {
        base.changeDirection(dir);
        sprite.Play(dir < 0 ? "climb" : "slide");
    }
}
