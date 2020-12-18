using Godot;

public class DeletePoint : GDKnyttBaseObject
{
    public void activate()
    {
        var rect = new Rect2(GlobalPosition, GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
        foreach (var layer in GDArea.Objects.Layers)
        {
            foreach (GDKnyttBaseObject knytt_object in layer.GetChildren())
            {
                if (knytt_object != this && rect.HasPoint(knytt_object.Center))
                {
                    knytt_object.QueueFree();
                }
            }
        }
    }
}
