using System.Collections.Generic;
using Godot;
using YUtil.Random;

public class SceneCPUParticles : Node2D
{
    [Export] public int Particles = 1;

    [Export] public float Lifetime = 1f;
    [Export] public float LifetimeVariation = 0f;

    [Export] PackedScene ParticleScene = null;

    [Export] public float Direction = 0f;
    [Export] public float DirectionVariation = 0f;

    [Export] public float Velocity = 0f;
    [Export] public float VelocityVariation = 0f;

    [Export] public float Mass = 1f;
    [Export] public float MassVariation = 0f;

    [Export] public float GravityDirection = 1.571f;
    [Export] public float GravityDirectionVariation = 0f;

    [Export] public float Gravity = 1f;
    [Export] public float GravityVariation = 0f;

    [Export] public float Drag = 0f;
    [Export] public float DragVariation = 0f;

    [Export] public string ParticleParams = "";

    // Brownian motion
    [Export] public bool BrownianMotion = false;

    [Export] public float BrownianX = 0f;
    [Export] public float BrownianXVariation = 0f;
    [Export] public float BrownianXSpeed = 1f;
    [Export] public float BrownianXSpeedVariation = 0f;

    [Export] public float BrownianY = 0f;
    [Export] public float BrownianYVariation = 0f;
    [Export] public float BrownianYSpeed = 1f;
    [Export] public float BrownianYSpeedVariation = 0f;

    [Export] public float BrownianExponent = 1.5f;

    private Stack<SceneCPUParticleInstance> pcache = new Stack<SceneCPUParticleInstance>();

    public void spawnParticles()
    {
        for (int i = 0; i < Particles; i++)
        {
            spawnParticle();
        }
    }

    private void spawnParticle()
    {
        SceneCPUParticleInstance p;
        if (pcache.Count > 0)
        {
            p = pcache.Pop();
            p.renew();
        }
        else
        {
            p = ParticleScene.Instance() as SceneCPUParticleInstance;
            p.parent = this;
            p.Params = this.ParticleParams;
            AddChild(p);
        }

        p.Lifetime = CalcVariation(Lifetime, LifetimeVariation);
        p.Velocity = MagnitudeVector(Direction, DirectionVariation, Velocity, VelocityVariation);
        p.Gravity = MagnitudeVector(GravityDirection, GravityDirectionVariation, Gravity, GravityVariation);
        p.Mass = CalcVariation(Mass, MassVariation);
        p.Drag = CalcVariation(Drag, DragVariation);

        p.BrownianMotion = BrownianMotion;
        p.BrownianForce = new Vector2(CalcVariation(BrownianX, BrownianXVariation), CalcVariation(BrownianY, BrownianYVariation));
        p.BrownianSpeed = new Vector2(CalcVariation(BrownianXSpeed, BrownianXSpeedVariation), CalcVariation(BrownianYSpeed, BrownianYSpeedVariation));
        p.BrownianExponent = BrownianExponent;
    }

    public void returnParticle(SceneCPUParticleInstance p)
    {
        p.hide();
        pcache.Push(p);
    }

    private Vector2 MagnitudeVector(float angle, float angle_var, float mag, float mag_var)
    {
        return InputToVector(angle, angle_var) * CalcVariation(mag, mag_var);
    }

    private Vector2 InputToVector(float angle, float variation)
    {
        float ua = CalcVariation(angle, variation);
        return new Vector2(Mathf.Cos(ua), Mathf.Sin(ua));
    }

    private float CalcVariation(float value, float variation)
    {
        return value + (GDKnyttDataStore.random.NextFloat(2 * variation) - variation);
    }
}
