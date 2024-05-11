using Godot;

public class Win : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        string ending = GDArea.Area.getExtraData("Ending") ?? "Ending";

        var save = GDArea.GDWorld.KWorld.CurrentSave;
        var endings = save.Endings;
        endings.Add(ending);
        save.Endings = endings; // to write to ini
        GDArea.GDWorld.Game.saveGame(save);

        string ini_cache_name = "user://Cache".PlusFile(juni.Game.GDWorld.KWorld.WorldDirectory.GetFile()).PlusFile("World.ini");
        string ini = GDKnyttAssetManager.loadTextFile(GDKnyttAssetManager.loadFile(ini_cache_name));
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.loadWorldConfig(ini);
        if (world.INIData["World"].ContainsKey("Endings"))
        {
            var all_endings = world.INIData["World"]["Endings"].Split('/');
            if (all_endings.Length > 0 ? endings.Count >= all_endings.Length : endings.Count > 0)
            {
                InfoScreen.setIniValue(juni.Game.GDWorld.KWorld, "Completed", "1");
            }
        }

        juni.win(ending);
    }
}
