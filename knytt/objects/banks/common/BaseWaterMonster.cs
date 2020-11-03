using Godot;

public class BaseWaterMonster : GDKnyttBaseObject
{
    [Export]public Texture Texture;
    [Export]public int AttackNum;

    public override void _Ready()
    {
        GetNode<Sprite>("Sprite").Texture = Texture;
        GetNode<AnimationPlayer>("Sprite/AnimationPlayer").Play("Shoot");
    }

    public void shoot()
    {
        GetNode<SceneCPUParticles>($"Attack{AttackNum}").spawnParticles();
        GetNode<AudioStreamPlayer2D>("RawAudioPlayer2D").Play();
    }
}
