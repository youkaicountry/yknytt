using Godot;

public partial class Artifact : CustomObject
{
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Enable custom animation
        int artifact_number = (ObjectID.y - 151) / 7 + 1;
        var artifact_key = $"Artifact{artifact_number}";
        if (!GDArea.GDWorld.KWorld.INIData["World"].ContainsKey(artifact_key)) { sprite.Play($"default{artifact_number}"); return; }

        info = new CustomObject.CustomObjectInfo();
        info.image = GDArea.GDWorld.KWorld.INIData["World"][artifact_key];
        info.anim_loopback = 1;
        info.anim_to = 14;
        info.anim_speed = 250;
        if (fillAnimation($"custom{artifact_number}")) { sprite.Play(); } else { sprite.Play($"default{artifact_number}"); }
    }
}
