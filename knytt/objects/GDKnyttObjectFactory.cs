using System.Collections.Generic;
using Godot;
using YKnyttLib;

public static class GDKnyttObjectFactory
{
    private static Dictionary<KnyttPoint, string> ObjectLookup;

    static GDKnyttObjectFactory()
    {
        ObjectLookup = new Dictionary<KnyttPoint, string>();
        ObjectLookup[new KnyttPoint(0, 1)]  =  "SavePoint";
        ObjectLookup[new KnyttPoint(0, 2)]  =  "Win";
        ObjectLookup[new KnyttPoint(0, 3)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 4)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 5)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 6)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 7)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 8)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 9)]  =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 10)] =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 11)] =  "NoClimb";
        ObjectLookup[new KnyttPoint(0, 14)] =  "Shift";
        ObjectLookup[new KnyttPoint(0, 15)] =  "Shift";
        ObjectLookup[new KnyttPoint(0, 16)] =  "Shift";
        ObjectLookup[new KnyttPoint(0, 17)] =  "Sign";
        ObjectLookup[new KnyttPoint(0, 18)] =  "Sign";
        ObjectLookup[new KnyttPoint(0, 19)] =  "Sign";
        ObjectLookup[new KnyttPoint(0, 20)] =  "Warp";
        ObjectLookup[new KnyttPoint(0, 21)] =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 22)] =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 23)] =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 24)] =  "PowerItem";
        ObjectLookup[new KnyttPoint(0, 25)] =  "NoJump";
        ObjectLookup[new KnyttPoint(1, 1)] =   "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 2)] =   "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 3)] =   "Waterfall";
        ObjectLookup[new KnyttPoint(1, 4)] =   "Waterfall";
        ObjectLookup[new KnyttPoint(1, 5)] =   "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 6)] =   "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 7)] =   "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 8)] =   "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 9)] =   "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 10)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 11)] =  "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 12)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 13)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 14)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 15)] =  "Waterfall";
        ObjectLookup[new KnyttPoint(1, 16)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 17)] =  "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 18)] =  "Bubble";
        ObjectLookup[new KnyttPoint(1, 19)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 20)] =  "Waterfall";
        ObjectLookup[new KnyttPoint(1, 21)] =  "WaterBlock";
        ObjectLookup[new KnyttPoint(1, 22)] =  "LiquidPool";
        ObjectLookup[new KnyttPoint(1, 23)] =  "Waterfall";
        ObjectLookup[new KnyttPoint(1, 24)] =  "WaterBlock";
        ObjectLookup[new KnyttPoint(2, 15)] =  "BuzzFlyer";
        ObjectLookup[new KnyttPoint(2, 16)] =  "BuzzFlyer";
        ObjectLookup[new KnyttPoint(2, 17)] =  "BuzzFlyer";
        ObjectLookup[new KnyttPoint(2, 18)] =  "Elemental";
        ObjectLookup[new KnyttPoint(2, 19)] =  "Elemental";
        ObjectLookup[new KnyttPoint(2, 20)] =  "FlySpike";
        ObjectLookup[new KnyttPoint(2, 21)] =  "FlySpike";
        ObjectLookup[new KnyttPoint(2, 30)] =  "CircleBird";
        ObjectLookup[new KnyttPoint(3, 5)] =   "FollowPeaceful";
        ObjectLookup[new KnyttPoint(3, 12)] =  "Eskimo1";
        ObjectLookup[new KnyttPoint(3, 13)] =  "Eskimo2";
        ObjectLookup[new KnyttPoint(3, 21)] =  "BeardGuy";
        ObjectLookup[new KnyttPoint(3, 27)] =  "ForestDude";
        ObjectLookup[new KnyttPoint(3, 29)] =  "SkyGirl";
        ObjectLookup[new KnyttPoint(3, 32)] =  "FlyGirl";
        ObjectLookup[new KnyttPoint(3, 39)] =  "CaveDude";
        ObjectLookup[new KnyttPoint(3, 42)] =  "BigCat";
        ObjectLookup[new KnyttPoint(3, 43)] =  "SmallCat";
        ObjectLookup[new KnyttPoint(4, 1)] =   "FollowMonster";
        ObjectLookup[new KnyttPoint(4, 18)] =  "ToastMonster";
        ObjectLookup[new KnyttPoint(6, 1)] =   "TrapCeiling";
        ObjectLookup[new KnyttPoint(6, 2)] =   "LabyrinthSpike";
        ObjectLookup[new KnyttPoint(6, 3)] =   "RandomLabyrinthSpike";
        ObjectLookup[new KnyttPoint(6, 4)] =   "FastLabyrinthSpike";
        ObjectLookup[new KnyttPoint(6, 5)] =   "Eater";
        ObjectLookup[new KnyttPoint(6, 7)] =   "DownStuffShooter";
        ObjectLookup[new KnyttPoint(6, 8)] =   "SelfDropper";
        ObjectLookup[new KnyttPoint(6, 9)] =   "UpStuffShooter";
        ObjectLookup[new KnyttPoint(6, 10)] =  "UpSpikes";
        ObjectLookup[new KnyttPoint(6, 11)] =  "DownSpikes";
        ObjectLookup[new KnyttPoint(6, 12)] =  "LeftSpikes";
        ObjectLookup[new KnyttPoint(6, 13)] =  "RightSpikes";
        ObjectLookup[new KnyttPoint(7, 1)] =   "Leaf";
        ObjectLookup[new KnyttPoint(7, 5)] =   "SunRay";
        ObjectLookup[new KnyttPoint(7, 6)] =   "Leaf";
        ObjectLookup[new KnyttPoint(7, 8)] =   "Rain";
        ObjectLookup[new KnyttPoint(7, 9)] =   "RaindropObject";
        ObjectLookup[new KnyttPoint(7, 10)] =  "Leaf";
        ObjectLookup[new KnyttPoint(7, 12)] =  "Leaf";
        ObjectLookup[new KnyttPoint(7, 16)] =  "Cloud";
        ObjectLookup[new KnyttPoint(8, 6)] =   "SimpleDecoration";
        ObjectLookup[new KnyttPoint(8, 7)] =   "SimpleDecoration";
        ObjectLookup[new KnyttPoint(8, 9)] =   "SimpleDecoration";
        ObjectLookup[new KnyttPoint(8, 10)] =  "Star";
        ObjectLookup[new KnyttPoint(8, 12)] =  "SimpleDecoration";
        ObjectLookup[new KnyttPoint(10, 1)] =  "Bouncer1";
        ObjectLookup[new KnyttPoint(10, 2)] =  "Bouncer2";
        ObjectLookup[new KnyttPoint(10, 5)] =  "BouncerEnemy";
        ObjectLookup[new KnyttPoint(10, 8)] =  "BouncerGreen";
        ObjectLookup[new KnyttPoint(10, 9)] = "BigFluff";
        ObjectLookup[new KnyttPoint(10, 10)] = "HappyFluff";
        ObjectLookup[new KnyttPoint(10, 11)] = "OldFluff";
        ObjectLookup[new KnyttPoint(10, 12)] = "RegularFluff";
        ObjectLookup[new KnyttPoint(10, 13)] = "TeenageFluff";
        ObjectLookup[new KnyttPoint(12, 5)] =  "GhostBlock";
        ObjectLookup[new KnyttPoint(12, 10)] = "GhostEater";
        ObjectLookup[new KnyttPoint(12, 11)] = "Ghost";
        ObjectLookup[new KnyttPoint(12, 17)] = "NoWall";
        ObjectLookup[new KnyttPoint(12, 18)] = "GhostMarker";
        ObjectLookup[new KnyttPoint(12, 19)] = "GhostMarker";
        ObjectLookup[new KnyttPoint(12, 20)] = "GhostMarker";
        ObjectLookup[new KnyttPoint(12, 21)] = "GhostMarker";
        ObjectLookup[new KnyttPoint(13, 1)] =  "RobotCannon";
        ObjectLookup[new KnyttPoint(13, 2)] =  "HomingCannon";
        ObjectLookup[new KnyttPoint(13, 6)] =  "RollerGenerator";
        ObjectLookup[new KnyttPoint(13, 7)] =  "Laser7";
        ObjectLookup[new KnyttPoint(13, 8)] =  "Laser8";
        ObjectLookup[new KnyttPoint(13, 9)] =  "Laser9";
        ObjectLookup[new KnyttPoint(13, 10)] = "Laser10";
        ObjectLookup[new KnyttPoint(13, 11)] = "Laser11";
        ObjectLookup[new KnyttPoint(13, 12)] = "Laser12";
        ObjectLookup[new KnyttPoint(13, 13)] = "FlyBot";
        ObjectLookup[new KnyttPoint(13, 14)] = "DarkHomingCannon";
        ObjectLookup[new KnyttPoint(14, 5)] =  "PassiveWalker";
        ObjectLookup[new KnyttPoint(14, 6)] =  "PassiveWalker";
        ObjectLookup[new KnyttPoint(14, 7)] =  "PassiveWalker";
        ObjectLookup[new KnyttPoint(14, 8)] =  "PassiveWalker";
        ObjectLookup[new KnyttPoint(14, 10)] = "StationaryRock";
        ObjectLookup[new KnyttPoint(15, 1)] =  "Block1";
        ObjectLookup[new KnyttPoint(15, 2)] =  "Block2";
        ObjectLookup[new KnyttPoint(15, 3)] =  "Block3";
        ObjectLookup[new KnyttPoint(15, 4)] =  "Block4";
        ObjectLookup[new KnyttPoint(15, 5)] =  "Updraft";
        ObjectLookup[new KnyttPoint(15, 6)] =  "ProximityBlock";
        ObjectLookup[new KnyttPoint(15, 7)] =  "ProximityBlock";
        ObjectLookup[new KnyttPoint(15, 8)] =  "FalseBlock";
        ObjectLookup[new KnyttPoint(15, 9)] =  "FalseBlock9";
        ObjectLookup[new KnyttPoint(15, 10)] =  "FalseBlock10";
        ObjectLookup[new KnyttPoint(15, 11)] =  "FalseBlock11";
        ObjectLookup[new KnyttPoint(15, 12)] =  "LockBlock";
        ObjectLookup[new KnyttPoint(15, 13)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 14)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 15)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 16)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 17)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 18)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 19)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 20)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 21)] =  "Password";
        ObjectLookup[new KnyttPoint(15, 22)] =  "ClearBlock";
        ObjectLookup[new KnyttPoint(15, 23)] =  "LockBlockHorizontal";
        ObjectLookup[new KnyttPoint(15, 24)] =  "LockBlockVertical";
        ObjectLookup[new KnyttPoint(15, 26)] = "HoldButton";
        ObjectLookup[new KnyttPoint(15, 27)] = "Door";
        ObjectLookup[new KnyttPoint(15, 28)] = "Door";
        ObjectLookup[new KnyttPoint(15, 29)] = "Door";
        ObjectLookup[new KnyttPoint(15, 30)] = "Door";
        ObjectLookup[new KnyttPoint(16, 1)] =  "Spring";
        ObjectLookup[new KnyttPoint(16, 13)] = "InvisibleBarrier";
        ObjectLookup[new KnyttPoint(16, 14)] = "InvisibleBlock";
        ObjectLookup[new KnyttPoint(17, 3)] =  "CreepyWaterMonster";
        ObjectLookup[new KnyttPoint(18, 1)] =  "Fish";
        ObjectLookup[new KnyttPoint(18, 2)] =  "Fish";
        ObjectLookup[new KnyttPoint(18, 3)] =  "Fish";
        ObjectLookup[new KnyttPoint(18, 4)] =  "Fish";
        ObjectLookup[new KnyttPoint(18, 5)] =  "Fish";
        ObjectLookup[new KnyttPoint(18, 6)] = "WaterMonster";
    }

    public static GDKnyttObjectBundle buildKnyttObject(KnyttPoint object_id)
    {
        if (!ObjectLookup.ContainsKey(object_id))
        {
            GD.Print($"Object {object_id.x}:{object_id.y} unimplemented.");
            return null;
        }
        
        string fname = $"res://knytt/objects/banks/bank{object_id.x}/{ObjectLookup[object_id]}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(fname);
        return new GDKnyttObjectBundle(object_id, scene);
    }
}

public class GDKnyttObjectBundle
{
    public KnyttPoint object_id;
    PackedScene scene;
    public Texture icon;

    public GDKnyttObjectBundle(KnyttPoint object_id, PackedScene scene, Texture icon=null)
    {
        this.object_id = object_id;
        this.scene = scene;
        this.icon = icon;
    }
    
    public GDKnyttBaseObject getNode(GDKnyttObjectLayer layer, KnyttPoint coords)
    {
        var node = scene.Instance() as GDKnyttBaseObject;
        node.initialize(object_id, layer, coords);
        return node;
    }
}
