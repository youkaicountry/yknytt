using Godot;
using System;

public class BaseEater : GDKnyttBaseObject
{
    public override void _Ready()
    {
        OrganicEnemy = true;
    }
    
    public void OnArea2DBodyEntered(GDKnyttBaseObject this_obj, object body)
    {
        if (!(body is Juni juni)) { return; }
        this_obj.GetNode<AnimatedSprite>("AnimatedSprite").Play("eat");
        this_obj.GetNode<AudioStreamPlayer2D>("Player").Play();
        juniDie(juni);
    }
        
    private void _on_Area2D_body_entered(object body)
    {
        OnArea2DBodyEntered(this, body);
    }
}
