using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BulletLayer : Node2D
{
    public delegate BaseBullet CreateBulletEvent();
    public delegate void InitBulletEvent(BaseBullet bullet, int i);

    private Dictionary<Type, List<Node2D>> enemies = new Dictionary<Type, List<Node2D>>();
    private Dictionary<Type, Queue<BaseBullet>> bullets = new Dictionary<Type, Queue<BaseBullet>>();
    private Dictionary<Type, int> bullets_limit = new Dictionary<Type, int>();
    private Dictionary<Type, CreateBulletEvent> create_events = new Dictionary<Type, CreateBulletEvent>();
    private Dictionary<Node2D, InitBulletEvent> init_events = new Dictionary<Node2D, InitBulletEvent>();
    private Dictionary<Type, int> shots_for_timer = new Dictionary<Type, int>();
    private Dictionary<Type, int> counters_for_timer = new Dictionary<Type, int>();

    public void RegisterEmitter(Node2D enemy_object, int max_bullets,
                                CreateBulletEvent on_create, InitBulletEvent on_init)
    {
        if (!bullets.ContainsKey(enemy_object.GetType()))
        {
            bullets.Add(enemy_object.GetType(), new Queue<BaseBullet>());
            bullets_limit.Add(enemy_object.GetType(), max_bullets);
            create_events.Add(enemy_object.GetType(), on_create);
            shots_for_timer.Add(enemy_object.GetType(), 0);
            counters_for_timer.Add(enemy_object.GetType(), 0);
        }
        if (!enemies.ContainsKey(enemy_object.GetType()))
        {
            enemies.Add(enemy_object.GetType(), new List<Node2D>());
        }
        enemies[enemy_object.GetType()].Add(enemy_object);
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

    public void EmitPickOne(Type enemy_type, int n = 0)
    {
        var object_list = enemies[enemy_type];
        var enemy_object = object_list[GDKnyttDataStore.random.Next(object_list.Count)];
        Emit(enemy_object, n);
    }

    public void EmitOnTimer(Type enemy_type, int times)
    {
        shots_for_timer[enemy_type] = times;
        counters_for_timer[enemy_type] = 0;
    }

    private void _on_TickTimer_timeout()
    {
        foreach(var entry in enemies)
        {
            var type = entry.Key;
            var object_list = entry.Value;
            if (counters_for_timer[type] >= shots_for_timer[type]) { continue; }
            EmitPickOne(type, counters_for_timer[type]++);
            //foreach (var enemy_object in object_list)
            //Emit(enemy_object, counters_for_timer[type]);
            //counters_for_timer[type]++;
        }
    }

    public void Reset()
    {
        foreach (var entry in bullets)
        {
            var type = entry.Key;
            var bullet_list = entry.Value;

            foreach (var bullet in bullet_list)
            {
                if (bullet.Enabled)
                {
                    bullet.Enabled = false;
                }
            }

            shots_for_timer[type] = 0;
            counters_for_timer[type] = 0;
        }
        // Only GDKnyttBaseObjects resets with an area
        init_events = init_events.Where(kv => !(kv.Key is GDKnyttBaseObject)).ToDictionary(kv => kv.Key, kv => kv.Value);
        enemies = enemies.Where(kv => !kv.Key.IsSubclassOf(typeof(GDKnyttBaseObject))).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
