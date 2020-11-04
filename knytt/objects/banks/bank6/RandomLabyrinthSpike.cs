using Godot;
using System;

public class RandomLabyrinthSpike : LabyrinthSpike
{
    protected override void onCollide()
    {
        speed = 50 * (GDKnyttDataStore.random.Next(2) + 1);
    }
}
