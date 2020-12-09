using Godot;

public class WallNinja : Muff
{
    [Export] Vector2 shotPosition;
    [Export] int[] shotDirections;

    public override void _Ready()
    {
        base._Ready();
        GDArea.Bullets.RegisterEmitter(this, "NinjaStar",
            (p, i) => 
            {
                p.Translate(shotPosition);
                p.VelocityMMF2 = 80;
                p.DirectionMMF2 = random.NextElement(shotDirections);
                p.GravityMMF2 = 18;
            });
    }

    private async void _on_ShotTimer_timeout_ext()
    {
        var old_speed = speed;
        speed = 0;

        GetNode<AudioStreamPlayer2D>("ShotPlayer").Play();
        sprite.Play("shoot1");
        await ToSignal(sprite, "animation_finished");
        
        GDArea.Bullets.Emit(this);
        
        sprite.Play("shoot2");
        await ToSignal(sprite, "animation_finished");
        
        sprite.Play("walk");
        speed = old_speed;
        _on_DirectionTimer_timeout();
    }

    protected override void changeDirection(int dir)
    {
        base.changeDirection(dir);
        sprite.Play(dir < 0 ? "walk" : "slide");
    }
}
