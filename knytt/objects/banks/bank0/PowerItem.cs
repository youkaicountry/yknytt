using Godot;

public class PowerItem : GDKnyttBaseObject
{
    public static int[] Object2Power = new int[] { -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 
                   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, 9, 10, 11 };


    int power;

    public override void _Ready()
    {
        this.power = Object2Power[ObjectID.y];
        // Check if Juni has the powerup, hide if it is so.
        //GD.Print(Layer.ObjectLayers.GDArea.GDWorld.Game.Juni);
        if (Juni.Powers.getPower(power)) { this.Visible = false; }
        GetNode<AnimatedSprite>("AnimatedSprite").Animation = $"Power{power}";
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        Juni juni = (Juni)body;
        if (juni.Powers.getPower(power)) { return; }
        juni.setPower(power, true);

        GetNode<AnimatedSprite>("AnimatedSprite").Visible = false;
        GetNode<AnimationPlayer>("PickupAnimation").Play("Pickup");
    }
}
