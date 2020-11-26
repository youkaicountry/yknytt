using Godot;
using System;

public class RunThing : Muff
{
    [Export] float firstDelay = 0;
    [Export] float timer = 0;
    [Export] float safeDistance = 0;
    [Export] int attackSpeed = 0;
    [Export] float normalDeceleration = 0;
    [Export] float attackDeceleration = 0;

    protected override void init()
    {
        GetNode<Timer>("FirstDelayTimer").Start(firstDelay);
    }

    private void _on_FirstDelayTimer_timeout()
    {
        GetNode<Timer>("SpeedTimer").Start(timer);
        _on_SpeedTimer_timeout();
    }

    protected override void _on_SpeedTimer_timeout()
    {
        var juni_distance = Juni.ApparentPosition.x - (GlobalPosition.x + 12);

        var direction = 
            juni_distance >= safeDistance ? 1 :
            juni_distance <= -safeDistance ? -1 :
            Juni.FacingRight && juni_distance < 0 ? 1 :
            !Juni.FacingRight && juni_distance < 0 ? -1 :
            Juni.FacingRight && juni_distance > 0 ? 1 : -1;

        var any_speed = speedValues[GDKnyttDataStore.random.Next(speedValues.Length)];
        var speed = 
            Mathf.Abs(juni_distance) >= safeDistance ? any_speed :
            Juni.FacingRight && juni_distance < 0 ? any_speed :
            !Juni.FacingRight && juni_distance < 0 ? attackSpeed :
            Juni.FacingRight && juni_distance > 0 ? attackSpeed : any_speed;

        deceleration = speed == attackSpeed ? attackDeceleration : normalDeceleration;

        changeDirection(direction);
        changeSpeed(speed);
    }
}
