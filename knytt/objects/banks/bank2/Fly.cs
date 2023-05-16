using Godot;
using System;
using YUtil.Random;

public class Fly : GDKnyttBaseObject
{
    private AnimatedSprite player;
    private Timer flyUpTimer;
    private int triggerDistance;
    private float xBuzz;
    private bool flying = false;
    private bool timerIsOver = false;

    public override void _Ready()
    {
        player = GetNode<AnimatedSprite>("AnimatedSprite");
        flyUpTimer = GetNode<Timer>("FlyUpTimer");
        player.Play($"stop{ObjectID.y}");
        triggerDistance = ObjectID.y == 3 ? 50 : 80;
        xBuzz = ObjectID.y == 3 ? 120 : 66;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!flying && Juni.manhattanDistance(Center) < triggerDistance)
        {
            if (!timerIsOver) { flyUpTimer.Start(); }
            player.Play($"fly{ObjectID.y}");
            flying = true;
        }

        if (!flying) { return; }

        Vector2 diff = new Vector2(random.NextFloat(-xBuzz, xBuzz), 
            random.NextFloat(-53, 78) - (flyUpTimer.IsStopped() ? 0 : 50)) * delta;

        if (moveAndCollide(diff, testOnly: true) == null)
        {
            moveAndCollide(diff);
        }

        if (moveAndCollide(new Vector2(0, 1), testOnly: true) != null)
        {
            player.Play($"stop{ObjectID.y}");
            flying = false;
        }
    } // TODO: destroy if touch water

    private void _on_FlyUpTimer_timeout()
    {
        timerIsOver = true;
    }
}
