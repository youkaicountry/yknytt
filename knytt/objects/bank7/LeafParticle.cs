using Godot;

public class LeafParticle : SceneCPUParticleInstance
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite>("AnimatedSprite").Animation = Params;
    }

    public void _on_Area2D_entered(object body_or_area)
    {
        if (Params == "default" || Params == "3" || Params == "1")
        {
            stop();
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Disappear");
        }
        else
        {
            parent.returnParticle(this);
        }
    }

    private void _on_AnimationPlayer_animation_finished(string anim_name)
    {
        Modulate = new Color(1, 1, 1, 1);
        parent.returnParticle(this);
    }
}
