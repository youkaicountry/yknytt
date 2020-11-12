using Godot;
using System;
using YUtil.Random;

public class Bouncer2 : Bouncer
{
    protected override float nextSpeed()
    {
        return 2 + GDKnyttDataStore.random.Next(5);
    }
}
