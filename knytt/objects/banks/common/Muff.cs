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
    [Export] protected bool vertical = false;

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

        changeSpeed(initialSpeed);
        changeDirection(getDirection(DirectionChange.Random));
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        // Check for collisions with ObjectsCollide and area bounds
        var diff = speed * direction * SPEED_SCALE * delta;
        var diff_vec = vertical ? new Vector2(0, diff) : new Vector2(diff, 0);
        // Sometimes collision can be detected with zero movement! Muff got stuck after this.
        if (diff != 0 && (!GDArea.isIn(Center + diff_vec, x_border: 10) || moveAndCollide(diff_vec) != null))
        {
            collide();
        }

        var new_speed = speed - _deceleration * delta;
        if (speed > 0 && new_speed <= 0) { changeSpeed(0); } else { speed = new_speed; }
    }

    protected virtual void _on_SpeedTimer_timeout()
    {
        changeSpeed(random.NextElement(speedValues));
    }

    protected virtual void _on_DirectionTimer_timeout()
    {
        changeDirection(getDirection(directionChange));
    }

    protected int getDirection(DirectionChange dir_change)
    {
        return dir_change == DirectionChange.Random ? (random.NextBoolean() ? -1 : 1) :
               dir_change == DirectionChange.TowardsJuni ? (Juni.ApparentPosition.x > Center.x ? 1 : -1) :
                    (Juni.ApparentPosition.x > Center.x ? -1 : 1);
    }

    protected virtual void collide()
    {
        changeDirection(-direction);
    }

    protected virtual void changeDirection(int dir)
    {
        direction = dir;
        if (!vertical) { sprite.FlipH = direction < 0; }
    }

    protected virtual void changeSpeed(float s)
    {
        speed = s;
        // TODO: more accurate value for deceleration. Make speed *= 1.2, deceleration /= 1.2 , like in DS remake?
        _deceleration = speed * deceleration / 20f;
        sprite.Play(speed == 0 ? "default" : "walk");
    }
}
