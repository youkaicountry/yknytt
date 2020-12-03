using Godot;

public class Artifact : CustomObject
{
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Enable custom animation
        int artifact_number = (ObjectID.y - 151) / 7 + 1;
        var artifact_key = $"Artifact{artifact_number}";
        if (!GDArea.GDWorld.KWorld.INIData["World"].ContainsKey(artifact_key)) { playDefault(); return; }

        info = new CustomObject.CustomObjectInfo();
        info.image = GDArea.GDWorld.KWorld.INIData["World"][artifact_key];
        info.anim_loopback = 1;
        info.anim_to = 14;
        info.anim_speed = 250;
        if (fillAnimation($"custom{artifact_number}")) { sprite.Play(); } else { playDefault(); }
    }

    private void playDefault()
    {
        sprite.Offset = new Vector2(4, 4);
        sprite.Play("default");
    }
}
