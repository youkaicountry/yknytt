using System;
using Godot;

public class ParticleCircle : Node2D
{
    [Export]
    public Texture particleTexture;


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

    private float rot_speed;

    public override void _Ready()
    {
        float radius = getRangeValue(radiusRange);
        float start_angle = getRangeValue(startAngle);
        float da = getRangeValue(angleDifference);

        this.rot_speed = getRangeValue(rotationSpeedRange);

        for (int i = 0; i < particleNumber; i++)
        {
            Node2D arm = new Node2D();
            Sprite s = new Sprite();
            s.Texture = particleTexture;
            s.Position = new Vector2(radius, 0f);
            s.Scale = new Vector2(spriteScale, spriteScale);
            arm.AddChild(s);
            arm.Rotate(start_angle + (da*i));
            AddChild(arm);
        }
        
    }

    public override void _Process(float delta)
    {
        
    }

    public void circleProcess(float delta)
    {
        Rotate(delta*rot_speed);
    }

    public void cloudProcess(float delta)
    {

    }

    private float getRangeValue(Vector2 range)
    {
        return ((float)R.NextDouble())*(range.y-range.x) + range.x;
    }
}
