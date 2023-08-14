using Godot;

public class CollectionDoor : Door
{
    [Export] int creaturesRequired = 0;

    protected override bool checkKey(Juni juni)
    {
        return Juni.Powers.getCreaturesCount() >= creaturesRequired;
    }
}
