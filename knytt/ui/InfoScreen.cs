using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YKnyttLib;

public class InfoScreen : BasicScreen
{
    [Export] string complainURL;

    public GDKnyttWorldImpl KWorld { get; private set; }

    private WorldEntry world_entry;

    private bool in_game;

    public override void _Ready()
    {
        base._Ready();
        initFocus();

        if (GDKnyttSettings.Connection == GDKnyttSettings.ConnectionType.Offline)
        {
            GetNode<StarRate>("%Rate").Visible = false;
            GetNode<Button>("%ComplainButton").Disabled = GetNode<Button>("%StatsButton").Disabled = true;
        }
    }

    public override void initFocus()
    {
        GetNode<SlotButton>("InfoRect/Slot1Button").GrabFocus();
    }

    public override void goBack()
    {
        if (!in_game) { KWorld.purgeBinFile(); }
        base.goBack();
    }

    public void initialize(WorldEntry world_entry)
    {
        var kworld = new GDKnyttWorldImpl();
        this.world_entry = world_entry;
        if (new Directory().DirExists(world_entry.Path))
        {
            kworld.setDirectory(world_entry.Path, world_entry.Path.GetFile());
        }
        else
        {
            var loader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world_entry.Path));
            kworld.setBinMode(loader);
            kworld.setDirectory(world_entry.Path, loader.RootDirectory);
        }
        string ini = GDKnyttAssetManager.loadTextFile(kworld.getWorldData("World.ini"));
        kworld.loadWorldConfig(ini);
        initialize(kworld);
    }

    public void initialize(GDKnyttWorldImpl kworld)
    {
        KWorld = kworld;

        GetNode<Button>("%Uninstall/MainButton").Disabled = 
        GetNode<Button>("%OptimizeButton").Disabled = 
            world_entry == null || KWorld.WorldDirectory.StartsWith("res://");

        if (world_entry == null)
        {
            in_game = true;
            world_entry = new WorldEntry();
            // TODO: fill world_entry.Completed
        }

        Texture info = (KWorld.worldFileExists("Info+.png") ? KWorld.getWorldTexture("Info+.png") :
                        KWorld.worldFileExists("Info.png") ? KWorld.getWorldTexture("Info.png") : null) as Texture;
        if (info != null)
        {
            //info.Flags |= (uint)Texture.FlagsEnum.Filter;
            GetNode<TextureRect>("InfoRect").Texture = info;
        }

        GetNode<SlotButton>("InfoRect/Slot1Button").BaseFile = 
        GetNode<SlotButton>("InfoRect/Slot2Button").BaseFile = 
        GetNode<SlotButton>("InfoRect/Slot3Button").BaseFile = GDKnyttSettings.Saves.PlusFile(KWorld.WorldDirectoryName);
        updateRates();
    }

    public void closeOtherSlots(int slot)
    {
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot1Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot2Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot3Button"), slot);
    }

    private void closeSlotIfNot(SlotButton sb, int slot)
    {
        if (sb.slot != slot) { sb.close(); }
    }

    public void _on_SlotButton_StartGame(bool new_save, string filename, int slot)
    {
        string cache_dir = KWorld.WorldDirectory.GetFile();
        GDKnyttAssetManager.ensureDirExists($"user://Cache/{cache_dir}");
        var f = new File();
        f.Open($"user://Cache/{cache_dir}/LastPlayed.flag", File.ModeFlags.Write);
        f.Close();

        KnyttSave save = new KnyttSave(KWorld,
                         new_save ? GDKnyttAssetManager.loadTextFile(KWorld.getWorldData("DefaultSavegame.ini")) :
                                    GDKnyttAssetManager.loadTextFile(filename),
                                    slot);

        KWorld.CurrentSave = save;
        GDKnyttDataStore.KWorld = KWorld;
        GDKnyttDataStore.startGame(new_save);
        this.QueueFree();
    }

    private void _on_StatHTTPRequest_ready()
    {
        if (GDKnyttSettings.Connection == GDKnyttSettings.ConnectionType.Offline) { return; }
        string serverURL = GDKnyttSettings.ServerURL;
        GetNode<HTTPRequest>("StatHTTPRequest").Request(
            $"{serverURL}/rating/?name={Uri.EscapeDataString(KWorld.Info.Name)}&author={Uri.EscapeDataString(KWorld.Info.Author)}");
    }

    private void _on_StatHTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200) {; } else { return; }
        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { return; }

        var my_powers = new HashSet<int>();
        var my_cutscenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var my_endings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int slot = 1; slot <= 3; slot++)
        {
            string savename = $"{GDKnyttSettings.Saves}/{KWorld.WorldDirectoryName} {slot}.ini";
            if (new File().FileExists(savename))
            {
                KnyttSave save = new KnyttSave(KWorld, GDKnyttAssetManager.loadTextFile(savename), slot);
                for (int i = 0; i < 13; i++) { if (save.getPower(i)) { my_powers.Add(i); } }
                my_cutscenes.UnionWith(save.Cutscenes);
                my_endings.UnionWith(save.Endings);
            }
        }

        world_entry.Upvotes = HTTPUtil.jsonInt(json.Result, "upvotes");
        world_entry.Downvotes = HTTPUtil.jsonInt(json.Result, "downvotes");
        world_entry.Complains = HTTPUtil.jsonInt(json.Result, "complains");
        world_entry.OverallScore = HTTPUtil.jsonFloat(json.Result, "score");
        world_entry.Voters = HTTPUtil.jsonInt(json.Result, "voters");
        world_entry.Completions[1] = HTTPUtil.jsonInt(json.Result, "completions");
        world_entry.Completions[2] = HTTPUtil.jsonInt(json.Result, "backlogged");
        world_entry.Completions[3] = HTTPUtil.jsonInt(json.Result, "too_hard");
        world_entry.Completions[4] = HTTPUtil.jsonInt(json.Result, "not_interested");
        world_entry.Completions[5] = HTTPUtil.jsonInt(json.Result, "bad_design");
        updateRates();

        var stat_panel = GetNode<StatPanel>("InfoRect/StatPanel");
        int[] powers_count = new int[13];
        for (int i = 0; i < 13; i++)
        {
            powers_count[i] = HTTPUtil.jsonInt(json.Result, $"power{i}");
        }

        if (powers_count.Any(c => c > 0))
        {
            stat_panel.addLabel("Powers:");
            for (int i = 0; i < 13; i++)
            {
                if (powers_count[i] > 0)
                {
                    stat_panel.addPower(i, powers_count[i], my_powers.Contains(i));
                }
            }
        }

        List<string> cutscenes = new List<string>();
        List<int> cutscenes_count = new List<int>();
        List<string> endings = new List<string>();
        List<int> endings_count = new List<int>();

        var cutscene_infos = HTTPUtil.jsonValue<Godot.Collections.Array>(json.Result, "cutscenes");
        foreach (Godot.Collections.Dictionary record in cutscene_infos)
        {
            bool is_ending = HTTPUtil.jsonBool(record, "ending");
            (is_ending ? endings : cutscenes).Add(HTTPUtil.jsonValue<string>(record, "name"));
            (is_ending ? endings_count : cutscenes_count).Add(HTTPUtil.jsonInt(record, "counter"));
        }

        int winexits = HTTPUtil.jsonInt(json.Result, "winexits");
        if (endings.Count > 0)
        {
            stat_panel.addLabel("Endings:");
            foreach (var p in endings.Zip(endings_count, (a, b) => new { Name = a, Count = b }))
            {
                stat_panel.addEnding(p.Name, p.Count, my_endings.Contains(p.Name));
            }
        }
        else if (winexits > 0)
        {
            stat_panel.addEnding("Win Exit", winexits, my_endings.Contains("Ending"));
        }

        if (cutscenes.Count > 0)
        {
            stat_panel.addLabel("Cutscenes:");
            foreach (var p in cutscenes.Zip(cutscenes_count, (a, b) => new { Name = a, Count = b }))
            {
                stat_panel.addCutscene(p.Name, p.Count, my_cutscenes.Contains(p.Name));
            }
        }

        if (!(powers_count.Any(c => c > 0) || endings.Count > 0 || cutscenes.Count > 0 || winexits > 0))
        {
            stat_panel.addLabel("No achievements found");
        }

        var complete_option = GetNode<OptionButton>("%CompleteOption");
        for (int i = 1; i < world_entry.Completions.Length; i++)
        {
            complete_option.SetItemText(i, complete_option.GetItemText(i) + $" [{world_entry.Completions[i]}]");
        }
    }

    private void _on_StatsButton_pressed()
    {
        ClickPlayer.Play();
        var panel = GetNode<Panel>("InfoRect/StatPanel");
        panel.Visible = !panel.Visible;
    }

    private void _on_Rate_RateEvent(int n)
    {
        ClickPlayer.Play();
        string cache_dir = KWorld.WorldDirectory.GetFile();
        GDKnyttAssetManager.ensureDirExists($"user://Cache/{cache_dir}");
        string flagname = $"user://Cache/{cache_dir}/Score-{n}.flag";
        if (new File().FileExists(flagname)) { return; }
        if (n == 0 && world_entry.UserScore == 0) { return; }
        GetNode<Label>("InfoRect/HintLabel").Text = n == 0 ? "Removing your rating..." : $"Setting your rating as {n}...";
        sendRating(20 + n);
    }

    private void updateRating(int n)
    {
        string cache_dir = KWorld.WorldDirectory.GetFile();
        string flagname = $"user://Cache/{cache_dir}/Score-{n}.flag";
        for (int i = 2; i <= 10; i += 2) { new Directory().Remove($"user://Cache/{cache_dir}/Score-{i}.flag"); }
        if (n != 0)
        {
            var f = new File();
            f.Open(flagname, File.ModeFlags.Write);
            f.Close();
        }

        int old_voters = world_entry.Voters;
        world_entry.Voters += world_entry.UserScore == 0 && n > 0 ? 1 : world_entry.UserScore > 0 && n == 0 ? -1 : 0;
        world_entry.OverallScore = world_entry.Voters == 0 ? 0 :
            (world_entry.OverallScore * old_voters - (world_entry.UserScore - n) / 2) / world_entry.Voters;
        world_entry.UserScore = n;
    }

    private void _on_CompleteOption_item_selected(int index)
    {
        ClickPlayer.Play();
        string cache_dir = KWorld.WorldDirectory.GetFile();
        GDKnyttAssetManager.ensureDirExists($"user://Cache/{cache_dir}");
        string flagname = $"user://Cache/{cache_dir}/Completed-{index}.flag";
        
        for (var i = 1; i <= 5; i++) { new Directory().Remove($"user://Cache/{cache_dir}/Completed-{i}.flag"); }
        new Directory().Remove($"user://Cache/{cache_dir}/Completed.flag");

        if (index != 0)
        {
            var f = new File();
            f.Open(flagname, File.ModeFlags.Write);
            f.Close();
        }

        world_entry.Completed = index;
        var option_text = GetNode<OptionButton>("%CompleteOption").Text;
        if (option_text.LastIndexOf('[') != -1) { option_text = option_text.Left(option_text.LastIndexOf('[') - 1); }
        GetNode<Label>("InfoRect/HintLabel").Text = $"Setting your completion status as '{option_text}'...";
        if (!in_game) { GetParent<LevelSelection>().refreshButton(world_entry); }
        sendRating(40 + index);
    }

    private bool complain_visit;

    private void _on_ComplainButton_pressed()
    {
        ClickPlayer.Play();
        if (complain_visit) { OS.ShellOpen(complainURL); return; }

        string latest_save = null;
        ulong latest_time = 0;
        for (int slot = 1; slot <= 3; slot++)
        {
            string savename = $"{GDKnyttSettings.Saves}/{KWorld.WorldDirectoryName} {slot}.ini";
            if (new File().FileExists(savename) && new File().GetModifiedTime(savename) > latest_time)
            {
                latest_save = savename;
                latest_time = new File().GetModifiedTime(savename);
            }
        }

        string short_save = null;
        if (latest_save != null)
        {
            KnyttSave save = new KnyttSave(KWorld, GDKnyttAssetManager.loadTextFile(latest_save), 0);
            short_save = string.Join("", Enumerable.Range(0, 13).Select(i => save.getPower(i) ? "1" : "0")) + "," +
                string.Join("", Enumerable.Range(0, 10).Select(i => save.getFlag(i) ? "1" : "0")) +
                $";{save.getArea().x} {save.getArea().y} {save.getAreaPosition().x} {save.getAreaPosition().y}";
        }

        sendRating((int)RateHTTPRequest.Action.Complain, additional: short_save);
        complain_visit = true;
        GetNode<Button>("%ComplainButton").Text = "To GitHub";
    }

    private void sendRating(int action, string additional = null)
    {
        if (GDKnyttSettings.Connection == GDKnyttSettings.ConnectionType.Offline) { return; }
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(KWorld.Info.Name, KWorld.Info.Author, action, cutscene: additional, once: false);
    }

    private void _on_RateHTTPRequest_RateAdded(int action)
    {
        if (action >= 20 && action <= 30)
        {
            updateRating(action - 20);
            GetNode<Label>("InfoRect/HintLabel").Text = action == 20 ? "Your rating for this level was removed." :
                $"You rated this level as {(action - 20) / 2} (out of 5).";
        }
        if (action >= 40 && action <= 45)
        {
            string item_text = GetNode<OptionButton>("%CompleteOption").GetItemText(action - 40);
            int br_pos = item_text.IndexOf('[');
            if (br_pos != -1)
            {
                int new_count = int.Parse(item_text.Substring(br_pos + 1, item_text.IndexOf(']') - br_pos - 1)) + 1;
                GetNode<OptionButton>("%CompleteOption").SetItemText(action - 40, item_text.Left(br_pos) + $"[{new_count}]");
            }
            GetNode<Label>("InfoRect/HintLabel").Text = "Your completion status was set.";
        }
        if (action == (int)RateHTTPRequest.Action.Complain)
        {
            world_entry.Complains++;
            GetNode<Label>("InfoRect/HintLabel").Text = "Your latest save was sent to the server.";
        }
        
        updateRates();
    }

    public void updateRates()
    {
        GetNode<StarRate>("%Rate").Rate = world_entry.UserScore;
        GetNode<Label>("%OverallLabel").Text = world_entry.Voters != 0 ?
            $"Rating: {world_entry.OverallScore:0.0} (by {world_entry.Voters} people)" : "Rating: -";

        var complete_option = GetNode<OptionButton>("%CompleteOption");
        complete_option.Selected = world_entry.Completed;
        complete_option.SetItemText(0, world_entry.HasSaves ? "In Progress" : "Not Started");

        var complain_button = GetNode<GDKnyttButton>("%ComplainButton");
        complain_button.hint = "The latest save will be sent to the server as a bug report" + 
               (world_entry.Complains > 0 ? $" (marked {world_entry.Complains} times)" : "");
    }

    private void _on_OptimizeButton_pressed()
    {
        ClickPlayer.Play();
        if (OS.GetName() != "HTML5")
        {
            GetNode<Timer>("HintTimer").Start();
            Task.Run(optimize);
        }
        else
        {
            optimize();
        }
    }

    private void optimize()
    {
        string[] nodes_to_disable = { "InfoRect/BackButton", 
            "InfoRect/Slot1Button", "InfoRect/Slot2Button", "InfoRect/Slot3Button", 
            "%OptimizeButton", "%Uninstall/MainButton", "%Uninstall/ConfirmButton" };
        foreach (string node in nodes_to_disable) { GetNode<Button>(node).Disabled = true; }
        closeOtherSlots(-1);

        bool from_external = GDKnyttSettings.WorldsDirectory != "" && KWorld.WorldDirectory.StartsWith(GDKnyttSettings.WorldsDirectory);
        if (KWorld.BinMode && !from_external)
        {
            KWorld.unpackWorld();
            world_entry.Path = KWorld.WorldDirectory;
        }
        GDKnyttAssetManager.compileInternalTileset(KWorld, recompile: true);

        GetNode<Timer>("HintTimer").Stop();
        GDKnyttDataStore.ProgressHint = "Level was unpacked and compiled.";
        _on_HintTimer_timeout();
        foreach (string node in nodes_to_disable) { GetNode<Button>(node).Disabled = false; }
    }

    private void _on_HintTimer_timeout()
    {
        GetNode<Label>("InfoRect/HintLabel").Text = GDKnyttDataStore.ProgressHint;
    }

    private void _on_UninstallButton_pressed(bool show_confirm)
    {
        ClickPlayer.Play();
        GetNode<Button>("%Uninstall/MainButton").Visible = !show_confirm;
        GetNode<Button>("%Uninstall/ConfirmButton").Visible = show_confirm;
        GetNode<Button>("%Uninstall/CancelButton").Visible = show_confirm;
        GetNode<Button>(show_confirm ? "%Uninstall/CancelButton" : "%Uninstall/MainButton").GrabFocus();
    }

    private void _on_ConfirmButton_pressed()
    {
        KWorld.uninstallWorld();
        goBack();
        GetParent<LevelSelection>().refreshButton(world_entry, disable: true);
    }

    private void _on_ShowHint(string hint)
    {
        GetNode<Label>("InfoRect/HintLabel").Text = hint ?? "";
    }
}
