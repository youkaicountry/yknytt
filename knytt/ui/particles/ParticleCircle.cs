using Godot;
using System;
using System.Collections.Generic;

public partial class ParticleCircle : Node2D
{
    [Export]
    public Texture2D particleTexture;

    [Export]
    public bool cloud;

    [Export]
    public Vector2 rotationSpeedRange;

    [Export]
    public Vector2 radiusRange;

    [Export]
    public Vector2 startAngle;

    [Export]
    public int particleNumber;

    [Export]
    public float spriteScale;

    [Export]
    public Vector2 angleDifference;

    private Random R { get { return GetParent<MenuCloud>().R; } }

    List<float> cloud_speeds;

    private float rot_speed;

    public override void _Ready()
    {
        cloud_speeds = new List<float>();

        if (this.cloud) { setupCloud(); } else { setupCircle(); }
    }

    private void setupCircle()
    {
        float radius = getRangeValue(radiusRange);
        float start_angle = getRangeValue(startAngle);
        float da = getRangeValue(angleDifference);

        this.rot_speed = getRangeValue(rotationSpeedRange);

        for (int i = 0; i < particleNumber; i++)
        {
            addParticle(radius, start_angle + (da * i));
        }
    }

    private void setupCloud()
    {
        float angle = getRangeValue(startAngle);

        for (int i = 0; i < particleNumber; i++)
        {
            addParticle(getRangeValue(radiusRange), angle);
            angle += getRangeValue(angleDifference);
            cloud_speeds.Add(getRangeValue(rotationSpeedRange));
        }
    }

    private void addParticle(float radius, float angle)
    {
        Node2D arm = new Node2D();
        Sprite2D s = new Sprite2D();
        s.Texture = particleTexture;
        s.Position = new Vector2(radius, 0f);
        s.Scale = new Vector2(spriteScale, spriteScale);
        arm.AddChild(s);
        arm.Rotate(angle);
        AddChild(arm);
    }

    public override void _Process(double delta)
    {
        if (this.cloud) { cloudProcess((float)delta); } else { circleProcess((float)delta); }
    }

    public void circleProcess(float delta)
    {
        Rotate(delta * rot_speed);
    }

    public void cloudProcess(float delta)
    {
        var children = this.GetChildren();
        for (int i = 0; i < GetChildCount(); i++)
        {
            ((Node2D)children[i]).Rotate(delta * cloud_speeds[i]);
        }
    }

    private float getRangeValue(Vector2 range)
    {
        return ((float)R.NextDouble()) * (range.Y - range.X) + range.X;
    }
}
