using Godot;
using System;

public class MenuCloud : Node2D
{
    public Random R { get; }

    [Export]
    public int seed;

    public MenuCloud()
    {
        R = new Random(seed);
    }
}
