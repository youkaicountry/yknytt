using Godot;

public partial class LiquidPoolDeadly : LiquidPool
{
    private void _on_Area2D_body_entered(Node2D body)
    {
        switch (body)
        {
            case Juni juni: juniDie(juni); break;
            case GhostSlimeBullet slime: slime.disappear(true); break;
            case BaseBullet bullet: bullet.disappear(false); break;
        }
    }
}

