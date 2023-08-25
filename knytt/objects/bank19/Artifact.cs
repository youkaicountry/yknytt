using Godot;

public class Artifact : CustomObject
{
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Enable custom animation
        int artifact_number = (ObjectID.y - 151) / 7 + 1;
        var artifact_key = $"Artifact{artifact_number}";
        if (!GDArea.GDWorld.KWorld.INIData["World"].ContainsKey(artifact_key)) { sprite.Play($"default{artifact_number}"); return; }

        info = new CustomObject.CustomObjectInfo()
        {
            image = GDArea.GDWorld.KWorld.INIData["World"][artifact_key],
            anim_loopback = 1,
            anim_to = 14,
            anim_speed = 250
        };
        if (fillAnimation($"{GDArea.GDWorld.KWorld.WorldDirectoryName} custom{artifact_number}")) { sprite.Play(); }
        else { sprite.Play($"default{artifact_number}"); }
    }
}
