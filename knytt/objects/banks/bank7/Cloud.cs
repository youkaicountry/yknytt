using Godot;

public class Cloud : GDKnyttBaseObject, IParticleFetch
{
    public int ParticleType { get { return 0; } }

    protected override void _impl_initialize() { }

    protected override void _impl_process(float delta) { }
}
