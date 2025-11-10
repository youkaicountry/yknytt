using System.Linq;
using System.Text;
using Godot;
using IniParser.Parser;
using YKnyttLib;

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
        save.TotalDeaths = juni.Powers.TotalDeaths;
        save.HardestPlaceDeaths = juni.Powers.HardestPlaceDeaths;
        save.HardestPlace = juni.Powers.HardestPlace;
        save.TotalTime = juni.Powers.TotalTimeNow;
        GDArea.GDWorld.Game.saveGame(save, save_map: false); // save_map is a workaround: different thread + exit causes crash sometimes

        juni.Powers.Endings.Add(ending);
        checkLevelCompleted(GDArea.GDWorld.KWorld, juni, ending);

        juni.win(ending);
    }

    public static void checkLevelCompleted(KnyttWorld kworld, Juni juni, string cutscene)
    {
        string INI_PATH = GDKnyttDataStore.BaseDataDirectory.PlusFile("worlds.ini");
        var ini_text = GDKnyttAssetManager.loadTextFile(INI_PATH);
        var parser = new IniDataParser();
        var worlds_cache_ini = parser.Parse(ini_text);
        var section = worlds_cache_ini[kworld.WorldDirectory];

        if (!section.ContainsKey("Endings") && !section.ContainsKey("Final")) { return; }

        var all_endings = (section["Endings"] ?? "").Split('/');
        var final_cutscenes = (section["Final"] ?? "").Split('/');
        var completed_endings = juni.Powers.Endings;
        if ((all_endings.Length > 0 ? completed_endings.Count >= all_endings.Length : completed_endings.Count > 0)
            || final_cutscenes.Contains(cutscene))
        {
            section["Completed"] = "1";
            var f = new File();
            f.Open(INI_PATH, File.ModeFlags.Write);
            f.StoreBuffer(Encoding.GetEncoding(1252).GetBytes(worlds_cache_ini.ToString()));
            f.Close();
        }
    }
}
