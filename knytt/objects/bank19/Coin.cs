using Godot;

public class Coin : CustomObject
{
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        if (!GDArea.GDWorld.KWorld.INIData["World"].ContainsKey("Coin")) { sprite.Play("default"); return; }

        // Enable custom animation
        info = new CustomObject.CustomObjectInfo()
        {
            image = GDArea.GDWorld.KWorld.INIData["World"]["Coin"],
            anim_to = 8,
            anim_speed = 250
        };
        if (fillAnimation($"{GDArea.GDWorld.KWorld.WorldDirectoryName} custom")) { sprite.Play(); }
        else { sprite.Play("default"); }
    }
}
