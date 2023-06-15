using Godot;

public class RunThing : Muff
{
    [Export] float safeDistance = 0;
    [Export] int attackSpeed = 0;
    [Export] float normalDeceleration = 0;
    [Export] float attackDeceleration = 0;

    private void _on_RunTimer_timeout_ext()
    {
        var juni_distance = Juni.ApparentPosition.x - Center.x;

        var direction =
            juni_distance >= safeDistance ? 1 :
            juni_distance <= -safeDistance ? -1 :
            Juni.FacingRight && juni_distance < 0 ? 1 :
            !Juni.FacingRight && juni_distance < 0 ? -1 :
            Juni.FacingRight && juni_distance > 0 ? 1 : -1;

        var any_speed = random.NextElement(speedValues);
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
