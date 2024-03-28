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
            GetNode<Button>("%UpvoteButton").Disabled = GetNode<Button>("%DownvoteButton").Disabled = 
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
        world_entry.Completions = HTTPUtil.jsonInt(json.Result, "completions");
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

        if (endings.Count > 0)
        {
            stat_panel.addLabel("Endings:");
            foreach (var p in endings.Zip(endings_count, (a, b) => new { Name = a, Count = b }))
            {
                stat_panel.addEnding(p.Name, p.Count, my_endings.Contains(p.Name));
            }
        }

        if (cutscenes.Count > 0)
        {
            stat_panel.addLabel("Cutscenes:");
            foreach (var p in cutscenes.Zip(cutscenes_count, (a, b) => new { Name = a, Count = b }))
            {
                stat_panel.addCutscene(p.Name, p.Count, my_cutscenes.Contains(p.Name));
            }
        }

        if (!(powers_count.Any(c => c > 0) || endings.Count > 0 || cutscenes.Count > 0))
        {
            stat_panel.addLabel("No achievements found");
        }
    }

    private void _on_StatsButton_pressed()
    {
        ClickPlayer.Play();
        var panel = GetNode<Panel>("InfoRect/StatPanel");
        panel.Visible = !panel.Visible;
    }

    private void _on_UpvoteButton_pressed()
    {
        ClickPlayer.Play();
        sendRating((int)RateHTTPRequest.Action.Upvote);
    }

    private void _on_DownvoteButton_pressed()
    {
        ClickPlayer.Play();
        sendRating((int)RateHTTPRequest.Action.Downvote);
    }

    private void _on_CompleteButton_pressed()
    {
        ClickPlayer.Play();
        string cache_dir = KWorld.WorldDirectory.GetFile();
        GDKnyttAssetManager.ensureDirExists($"user://Cache/{cache_dir}");
        string flagname = $"user://Cache/{cache_dir}/Completed.flag";
        bool pressed = GetNode<Button>("%CompleteButton").Pressed;
        if (pressed)
        {
            GDKnyttAssetManager.ensureDirExists($"user://Cache/{cache_dir}");
            var f = new File();
            f.Open(flagname, File.ModeFlags.Write);
            f.Close();
            world_entry.Completed = true;
            GetNode<Label>("InfoRect/HintLabel").Text = "";
            sendRating((int)RateHTTPRequest.Action.Complete);
        }
        else
        {
            new Directory().Remove(flagname);
            world_entry.Completed = false;
            GetNode<Label>("InfoRect/HintLabel").Text = "Level was unmarked as completed.";
        }
        if (!in_game) { GetParent<LevelSelection>().refreshButton(world_entry); }
        updateRates();
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
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(KWorld.Info.Name, KWorld.Info.Author, action, cutscene: additional);
    }

    private void _on_RateHTTPRequest_RateAdded(int action)
    {
        if (action == (int)RateHTTPRequest.Action.Upvote) { world_entry.Upvotes++; }
        if (action == (int)RateHTTPRequest.Action.Downvote) { world_entry.Downvotes++; }
        if (action == (int)RateHTTPRequest.Action.Complain)
        {
            world_entry.Complains++;
            GetNode<Label>("InfoRect/HintLabel").Text = "Your latest save was sent to the server.";
        }
        if (action == (int)RateHTTPRequest.Action.Complete)
        {
            world_entry.Completions++;
            GetNode<Label>("InfoRect/HintLabel").Text = "Level was marked as completed.";
        }
        
        updateRates();
    }

    public void updateRates()
    {
        GetNode<Label>("%UpvoteLabel").Text = $"+{world_entry.Upvotes}";
        GetNode<Label>("%DownvoteLabel").Text = $"-{world_entry.Downvotes}";

        var complete_button = GetNode<GDKnyttButton>("%CompleteButton");
        var complain_button = GetNode<GDKnyttButton>("%ComplainButton");

        complete_button.Text = world_entry.Completed ? "Completed" : "Complete";
        complete_button.Pressed = world_entry.Completed;
        complete_button.hint = (world_entry.Completed ? "Unmark this level as completed" : "Mark this level as completed") + 
            (world_entry.Completions > 0 ? $" (marked {world_entry.Completions} times)" : "");

        complain_button.hint = "The latest save will be sent to the server as a complain" + 
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
