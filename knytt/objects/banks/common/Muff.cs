using Godot;
using YUtil.Random;

public class Muff : GDKnyttBaseObject
{
    protected enum DirectionChange { Random, TowardsJuni, AwayFromJuni }

    [Export] protected DirectionChange directionChange = DirectionChange.Random;
    [Export] protected float directionChangeTime = 0;
    [Export] protected float speedChangeTime = 0;
    [Export] protected int initialSpeed = 0;
    [Export] protected int[] speedValues = null;
    [Export] protected float deceleration = 0;

    protected const float SPEED_SCALE = 50f / 8;

    protected float speed;
    protected int direction;
    protected float _deceleration;

    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        if (speedChangeTime > 0) { GetNode<Timer>("SpeedTimer").Start(speedChangeTime); }
        if (directionChangeTime > 0) { GetNode<Timer>("DirectionTimer").Start(directionChangeTime); }
       
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        init();
    }

    protected virtual void init()
    {
        changeSpeed(initialSpeed);
        _on_DirectionTimer_timeout();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        var x_diff = speed * direction * SPEED_SCALE * delta;
        var new_x = Position.x + 12 + x_diff;
        if (Call("move_and_collide", new Vector2(x_diff, 0)) != null || new_x < 0 || new_x > 600)
        {
            collide();
        }
        var new_speed = speed - _deceleration * delta;
        if (speed > 0 && new_speed <= 0) { changeSpeed(0); } else { speed = new_speed; }
    }

    protected virtual void _on_SpeedTimer_timeout()
    {
        changeSpeed(speedValues[GDKnyttDataStore.random.Next(speedValues.Length)]);
    }

    protected virtual void _on_DirectionTimer_timeout()
    {
        changeDirection(getDirection(directionChange));
    }

    protected int getDirection(DirectionChange dir_change)
    {
        return dir_change == DirectionChange.Random ? (GDKnyttDataStore.random.NextBoolean() ? -1 : 1) :
               dir_change == DirectionChange.TowardsJuni ? (Juni.ApparentPosition.x > GlobalPosition.x + 12 ? 1 : -1) :
                    (Juni.ApparentPosition.x > GlobalPosition.x + 12 ? -1 : 1);

    }

    protected virtual void collide()
    {
        changeDirection(-direction);
    }

    protected virtual void changeDirection(int dir)
    {
        direction = dir;
        sprite.FlipH = direction < 0;
    }

    protected virtual void changeSpeed(float s)
    {
        speed = s;
        // TODO: more accurate value for deceleration. Make speed *= 1.2, deceleration /= 1.2 , like in DS remake?
        _deceleration = speed * deceleration / 20f;
        sprite.Play(speed == 0 ? "default" : "walk");
    }
}
