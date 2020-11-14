using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BulletLayer : Node2D
{
    public delegate BaseBullet CreateBulletEvent();
    public delegate void InitBulletEvent(BaseBullet bullet, int i);

    private Dictionary<Type, Queue<BaseBullet>> bullets = new Dictionary<Type, Queue<BaseBullet>>();
    private Dictionary<Type, int> bullets_limit = new Dictionary<Type, int>();
    private Dictionary<Type, CreateBulletEvent> create_events = new Dictionary<Type, CreateBulletEvent>();
    private Dictionary<Node2D, InitBulletEvent> init_events = new Dictionary<Node2D, InitBulletEvent>();

    public void RegisterEmitter(Node2D enemy_object, int max_bullets,
                                CreateBulletEvent on_create, InitBulletEvent on_init)
    {
        if (!bullets.ContainsKey(enemy_object.GetType()))
        {
            bullets.Add(enemy_object.GetType(), new Queue<BaseBullet>());
            bullets_limit.Add(enemy_object.GetType(), max_bullets);
            create_events.Add(enemy_object.GetType(), on_create);
        }
        init_events.Add(enemy_object, on_init);
    }

    private void _emit(Node2D enemy_object, int n = 0)
    {
        var queue = bullets[enemy_object.GetType()];
        int limit = bullets_limit[enemy_object.GetType()];
        //GD.Print($"emit {enemy_object.Position} {n} {queue.Count} {OS.GetTicksMsec()}");

        BaseBullet bullet;
        if (queue.Count >= limit)
        {
            bullet = queue.Dequeue();
        }
        else if (queue.Count == 0 || queue.Peek().Enabled)
        {
            bullet = create_events[enemy_object.GetType()]();
            bullet.GDArea = GetParent<GDKnyttArea>();
            AddChild(bullet);
        }
        else
        {
            bullet = queue.Dequeue();
        }

        bullet.GlobalPosition = enemy_object.GlobalPosition;
        bullet.Enabled = true;
        init_events[enemy_object](bullet, n);
        queue.Enqueue(bullet);
    }

    public void Emit(Node2D enemy_object, int n = 0)
    {
        CallDeferred("_emit", enemy_object, n);
    }

    public void EmitMany(Node2D enemy_object, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Emit(enemy_object, i);
        }
    }

    public void Reset()
    {
        foreach (var bullet_list in bullets.Values)
        {
            foreach (var bullet in bullet_list)
            {
                if (bullet.Enabled)
                {
                    bullet.Enabled = false;
                }
            }
        }
        // Only GDKnyttBaseObjects resets with an area
        init_events = init_events.Where(kv => !(kv.Key is GDKnyttBaseObject)).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
