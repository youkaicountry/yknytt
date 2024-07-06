using System.Text;
using Godot;
using IniParser.Parser;

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
        GDArea.GDWorld.Game.saveGame(save, save_map: false); // workaround: different thread + exit causes crash sometimes

        const string INI_PATH = "user://worlds.ini";
        var ini_text = GDKnyttAssetManager.loadTextFile(INI_PATH);
        var parser = new IniDataParser();
        var worlds_cache_ini = parser.Parse(ini_text);
        var section = worlds_cache_ini[juni.Game.GDWorld.KWorld.WorldDirectory];

        if (section.ContainsKey("Endings"))
        {
            var all_endings = section["Endings"].Split('/');
            if (all_endings.Length > 0 ? endings.Count >= all_endings.Length : endings.Count > 0)
            {
                section["Completed"] = "1";
                var f = new File();
                f.Open(INI_PATH, File.ModeFlags.Write);
                f.StoreBuffer(Encoding.GetEncoding(1252).GetBytes(worlds_cache_ini.ToString()));
                f.Close();
            }
        }

        juni.win(ending);
    }
}
