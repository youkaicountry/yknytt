using Godot;
using System;

public class BouncerGreen : Bouncer
{
    protected float newSpeed = 5;

    protected override float nextSpeed()
    {
        newSpeed = newSpeed >= 5 ? 2 : newSpeed + 1;
        return newSpeed;
    }

}
