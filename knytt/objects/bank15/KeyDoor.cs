using Godot;

public class KeyDoor : Door
{
    [Export] YKnyttLib.JuniValues.PowerNames power = 0;

    protected override bool checkKey(Juni juni)
    {
        return Juni.Powers.getPower(power);
    }
}
