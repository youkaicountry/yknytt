using Godot;
using System.Collections.Generic;
using System.Linq;

public class BulletLayer : Node2D
{
    public delegate void InitBulletEvent(BaseBullet bullet, int i);

    private Dictionary<string, PackedScene> bulletScenes = new Dictionary<string, PackedScene>();
    private Dictionary<string, Queue<BaseBullet>> bullets = new Dictionary<string, Queue<BaseBullet>>();
    private Dictionary<string, int> bulletsLimit = new Dictionary<string, int>();
    private Dictionary<Node2D, string> enemyScenes = new Dictionary<Node2D, string>();
    private Dictionary<Node2D, InitBulletEvent> initEvents = new Dictionary<Node2D, InitBulletEvent>();

    public void RegisterEmitter(Node2D enemy_object, string bullet_scene, int max_bullets, InitBulletEvent on_init)
    {
        if (!bulletScenes.ContainsKey(bullet_scene))
        {
            bulletScenes.Add(bullet_scene, ResourceLoader.Load<PackedScene>($"res://knytt/objects/bullets/{bullet_scene}.tscn"));
            bullets.Add(bullet_scene, new Queue<BaseBullet>());
            bulletsLimit.Add(bullet_scene, max_bullets);
        }
        enemyScenes.Add(enemy_object, bullet_scene);
        initEvents.Add(enemy_object, on_init);
    }

    private void _emit(Node2D enemy_object, int n = 0)
    {
        if (!enemyScenes.ContainsKey(enemy_object)) { return; }
        var bullet_scene = enemyScenes[enemy_object];
        var queue = bullets[bullet_scene];
        int limit = bulletsLimit[bullet_scene];
        //GD.Print($"emit {enemy_object.Position} {n} {queue.Count} {OS.GetTicksMsec()}");

        BaseBullet bullet;
        if (queue.Count >= limit)
        {
            bullet = queue.Dequeue();
        }
        else if (queue.Count == 0 || queue.Peek().Visible)
        {
            bullet = bulletScenes[bullet_scene].Instance() as BaseBullet;
            bullet.GDArea = GetParent<GDKnyttArea>();
            AddChild(bullet);
        }
        else
        {
            bullet = queue.Dequeue();
        }

        // TODO: set ZIndex of bullet
        bullet.GlobalPosition = enemy_object.GlobalPosition;
        bullet.Enabled = true;
        bullet.Visible = true;
        initEvents[enemy_object](bullet, n);
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
                    bullet.Visible = false;
                }
            }
        }
        // Only GDKnyttBaseObjects reset with an area
        enemyScenes = enemyScenes.Where(kv => !(kv.Key is GDKnyttBaseObject)).ToDictionary(kv => kv.Key, kv => kv.Value);
        initEvents = initEvents.Where(kv => !(kv.Key is GDKnyttBaseObject)).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
