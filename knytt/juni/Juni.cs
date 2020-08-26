using Godot;

public class Juni : KinematicBody2D
{
    [Export] float speed = 150f;
    [Export] float jump_speed = -300f;
    [Export] float gravity = 1000f;

    public Vector2 velocity = Vector2.Zero;

    public void getInput()
    {
        velocity.x = 0f;
        if (Input.IsActionPressed("right")) { velocity.x += speed; }
        if (Input.IsActionPressed("left")) { velocity.x -= speed; }
    }

    public override void _PhysicsProcess(float delta)
    {
        getInput();
        velocity.y += gravity * delta;
        velocity = MoveAndSlide(velocity, Vector2.Up);
        if (Input.IsActionJustPressed("jump") && IsOnFloor()) { velocity.y = jump_speed; }
    }
}
