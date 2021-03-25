using Godot;
using System.Linq;

public class FallPropellerDude : BuzzFlyer
{
    private static readonly Vector2[] directionsOverride = Directions.Where(d => d.y <= 0 && d != Vector2.Zero).ToArray();

    private AnimatedSprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        base._Ready();
    }

    protected override void changeDirection()
    {
        if (sprite.Animation != "default") { sprite.Play("default"); }
        currentDirection = random.NextElement(directionsOverride);
    }

    private void _on_AttackTimer_timeout()
    {
        if (Mathf.Abs(Juni.ApparentPosition.x - Center.x) < 55 && Center.y < Juni.ApparentPosition.y - 24)
        {
            if (sprite.Animation != "attack") { sprite.Play("attack"); }
            currentDirection = new Vector2(Juni.ApparentPosition.x < Center.x ? -2 : 2, 3);
        }
    }
}
