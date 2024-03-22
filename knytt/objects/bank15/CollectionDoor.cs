using Godot;

public class CollectionDoor : Door
{
    [Export] int creaturesRequired = 0;

    public override bool checkKey(Juni juni)
    {
        return Juni.Powers.getCreaturesCount() >= creaturesRequired;
    }
}
