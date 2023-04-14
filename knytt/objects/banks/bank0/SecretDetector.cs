using Godot;

public partial class SecretDetector : GDKnyttBaseObject
{
    public override void _Ready()
    {
        organic_enemy_max_distance = 26f;
        discrete_detector = true;
        enemy_detector_color = ObjectID.y == 27 ? new Color(0.2f, 0.6f, 0.4f) : new Color(0.4f, 1, 0.8f);
    }
}
