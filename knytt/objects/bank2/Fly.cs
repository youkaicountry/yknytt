using Godot;
using System;
using YUtil.Random;

public partial class Fly : GDKnyttBaseObject
{
    private AnimatedSprite2D player;
    private Timer flyUpTimer;
    private int triggerDistance;
    private float xBuzz;
    private bool flying = false;
    private bool timerIsOver = false;

    public override void _Ready()
    {
        player = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        flyUpTimer = GetNode<Timer>("FlyUpTimer");
        player.Play($"stop{ObjectID.Y}");
        triggerDistance = ObjectID.Y == 3 ? 50 : 80;
        xBuzz = ObjectID.Y == 3 ? 120 : 66;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!flying && Juni.manhattanDistance(Center) < triggerDistance)
        {
            if (!timerIsOver) { flyUpTimer.Start(); }
            player.Play($"fly{ObjectID.Y}");
            flying = true;
        }

        if (!flying) { return; }

        Vector2 diff = new Vector2(random.NextFloat(-xBuzz, xBuzz), 
            random.NextFloat(-53, 78) - (flyUpTimer.IsStopped() ? 0 : 50)) * delta;

        if (moveAndCollide(diff, testOnly: true) == null)
        {
            moveAndCollide(diff);
        }

        if (moveAndCollide(Vector2.Down, testOnly: true) != null)
        {
            player.Play($"stop{ObjectID.Y}");
            flying = false;
        }
    } // TODO: destroy if touch water

    private void _on_FlyUpTimer_timeout()
    {
        timerIsOver = true;
    }
}
