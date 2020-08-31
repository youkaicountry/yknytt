using System.Collections.Generic;
using Godot;
using YUtil.Random;

public class LiquidPool : GDKnyttBaseObject
{
    bool ping = true;
    string animation;

    static Dictionary<int, PoolInfo> Info;

    static LiquidPool()
    {
        Info = new Dictionary<int, PoolInfo>();
        Info.Add(1, new PoolInfo(null, true));
        Info.Add(2, new PoolInfo(null, true));
        Info.Add(5, new PoolInfo("Halo0", true));
        Info.Add(6, new PoolInfo(null, false));
        Info.Add(10, new PoolInfo("Halo1", true));
        Info.Add(12, new PoolInfo("Halo1", true));
        Info.Add(13, new PoolInfo(null, false));
        Info.Add(14, new PoolInfo(null, true));
        Info.Add(16, new PoolInfo(null, false));
        Info.Add(19, new PoolInfo(null, true));
        Info.Add(22, new PoolInfo("Halo2", true));
    }

    struct PoolInfo
    {
        public string halo;
        public bool deadly;

        public PoolInfo(string halo, bool deadly)
        {
            this.halo = halo;
            this.deadly = deadly;
        }
    }

    bool reset = false;

    public void _on_AnimatedSprite_animation_finished()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        player.Stop();
        player.Play(animation, ping);
        ping = !ping;
    }

    protected override void _impl_initialize()
    {
        var player = GetNode<AnimatedSprite>("AnimatedSprite");
        
        animation = string.Format("Pool{0}", ObjectID.y);
        var pool_info = Info[ObjectID.y];
        if (pool_info.halo != null)
        {
            var halo = GetNode<AnimatedSprite>("Halo");
            halo.Play(pool_info.halo);
            halo.Visible = true;
        }
        if (!pool_info.deadly) { GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Disabled = true; }

        player.SpeedScale = GDKnyttDataStore.random.NextFloat(.6f, 1f);
        player.Play(animation);
        player.Frame = GDKnyttDataStore.random.Next(player.Frames.GetFrameCount(animation));
    }

    protected override void _impl_process(float delta) { }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni)) { return; }
        ((Juni)(body)).die();
    }
}
