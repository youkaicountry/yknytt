using Godot;

public class KeyDoor : Door
{
    [Export] YKnyttLib.JuniValues.PowerNames power = 0;

    public override bool checkKey(Juni juni)
    {
        return Juni.Powers.getPower(power);
    }
}
