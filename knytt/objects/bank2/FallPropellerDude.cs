using Godot;
using System.Linq;

public partial class FallPropellerDude : BuzzFlyer
{
    private static readonly Vector2[] directionsOverride = Directions.Where(d => d.Y <= 0 && d != Vector2.Zero).ToArray();

    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        base._Ready();
    }

    protected override void changeDirection()
    {
        if (sprite.Animation != "default") { sprite.Play("default"); }
        currentDirection = random.NextElement(directionsOverride);
    }

    private void _on_AttackTimer_timeout()
    {
        if (Mathf.Abs(Juni.ApparentPosition.X - Center.X) < 55 && Center.Y < Juni.ApparentPosition.Y - 24)
        {
            if (sprite.Animation != "attack") { sprite.Play("attack"); }
            currentDirection = new Vector2(Juni.ApparentPosition.X < Center.X ? -2 : 2, 3);
        }
    }
}
