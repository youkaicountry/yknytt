using Godot;
using YUtil.Random;

public class Muff : GDKnyttBaseObject
{
    enum DirectionChange { Random, TowardsJuni, AwayFromJuni }

    [Export] DirectionChange directionChange = DirectionChange.Random;
    [Export] float directionChangeTime = 0;
    [Export] float speedChangeTime = 0;
    [Export] int initialSpeed = 0;
    [Export] int[] speedValues = null;

    protected const float SPEED_SCALE = 50f / 8;

    protected float speed;
    protected int direction;

    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        GetNode<Timer>("SpeedTimer").Start(speedChangeTime);
        GetNode<Timer>("DirectionTimer").Start(directionChangeTime);
        
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        changeSpeed(initialSpeed);
        _on_DirectionTimer_timeout();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Call("move_and_collide", new Vector2(speed * direction * SPEED_SCALE * delta, 0)) != null)
        {
            collide();
        }
    }

    protected virtual void _on_SpeedTimer_timeout()
    {
        changeSpeed(speedValues[GDKnyttDataStore.random.Next(speedValues.Length)]);
    }

    protected virtual void _on_DirectionTimer_timeout()
    {
        var new_direction = 
            directionChange == DirectionChange.Random ? (GDKnyttDataStore.random.NextBoolean() ? -1 : 1) :
            directionChange == DirectionChange.TowardsJuni ? (Juni.ApparentPosition.x > GlobalPosition.x ? 1 : -1) :
                (Juni.ApparentPosition.x > GlobalPosition.x ? -1 : 1);
        changeDirection(new_direction);
    }

    protected virtual void collide()
    {
        changeDirection(-direction);
    }

    protected void changeDirection(int dir)
    {
        direction = dir;
        sprite.FlipH = direction < 0;
    }

    protected void changeSpeed(float s)
    {
        speed = s;
        sprite.Play(speed == 0 ? "default" : "walk");
    }
}
