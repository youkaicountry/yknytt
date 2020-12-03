using Godot;

public class Coin : CustomObject
{
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        if (!GDArea.GDWorld.KWorld.INIData["World"].ContainsKey("Coin")) { playDefault(); return; }

        // Enable custom animation
        info = new CustomObject.CustomObjectInfo();
        info.image = GDArea.GDWorld.KWorld.INIData["World"]["Coin"];
        info.anim_to = 8;
        info.anim_speed = 250;
        if (fillAnimation("custom")) { sprite.Play(); } else { playDefault(); }
    }

    private void playDefault()
    {
        sprite.Offset = new Vector2(4, 4);
        sprite.Play("default");
    }
}
