using Godot;
using Godot.Collections;
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

    public override void _Ready()
    {
        initFocus();
    }

    public override void initFocus()
    {
        GetNode<SlotButton>("InfoRect/Slot1Button").GrabFocus();
    }

    public override void goBack()
    {
        KWorld.purgeBinFile();
        base.goBack();
    }

    public void initialize(WorldEntry world_entry)
    {
        KWorld = new GDKnyttWorldImpl();
        this.world_entry = world_entry;
        if (new Directory().DirExists(world_entry.Path))
        {
            KWorld.setDirectory(world_entry.Path, GDKnyttAssetManager.extractFilename(world_entry.Path));
        }
        else
        {
            var loader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world_entry.Path));
            KWorld.setBinMode(loader);
            KWorld.setDirectory(world_entry.Path, loader.RootDirectory);
        }
        string ini = GDKnyttAssetManager.loadTextFile(KWorld.getWorldData("World.ini"));
        KWorld.loadWorldConfig(ini);

        Texture info = (KWorld.worldFileExists("Info+.png") ? KWorld.getWorldTexture("Info+.png") :
                        KWorld.worldFileExists("Info.png") ? KWorld.getWorldTexture("Info.png") : null) as Texture;
        if (info != null)
        {
            info.Flags |= (uint)Texture.FlagsEnum.Filter;
            GetNode<TextureRect>("InfoRect").Texture = info;
        }

        GetNode<SlotButton>("InfoRect/Slot1Button").BaseFile = "user://Saves/" + KWorld.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot2Button").BaseFile = "user://Saves/" + KWorld.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot3Button").BaseFile = "user://Saves/" + KWorld.WorldDirectoryName;
        GetNode<Button>("InfoRect/RatePanel/VBoxContainer/Uninstall/MainButton").Disabled = 
        GetNode<Button>("InfoRect/RatePanel/VBoxContainer/OptimizeButton").Disabled = 
            KWorld.WorldDirectory.StartsWith("res://");
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
        string cache_dir = GDKnyttAssetManager.extractFilename(KWorld.WorldDirectory);
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
            string savename = $"user://Saves/{KWorld.WorldDirectoryName} {slot}.ini";
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
        foreach (Dictionary record in cutscene_infos)
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

    private bool complain_visit;

    private void _on_ComplainButton_pressed()
    {
        ClickPlayer.Play();
        if (complain_visit)
        {
            OS.ShellOpen(complainURL);
        }
        else
        {
            sendRating((int)RateHTTPRequest.Action.Complain);
            complain_visit = true;
            GetNode<Button>("InfoRect/RatePanel/VBoxContainer/ComplainButton").Text = "Visit GitHub to report";
        }
    }

    private void sendRating(int action)
    {
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(KWorld.Info.Name, KWorld.Info.Author, action);
    }

    private void _on_RateHTTPRequest_RateAdded(int action)
    {
        if (action == (int)RateHTTPRequest.Action.Upvote) { world_entry.Upvotes++; }
        if (action == (int)RateHTTPRequest.Action.Downvote) { world_entry.Downvotes++; }
        if (action == (int)RateHTTPRequest.Action.Complain) { world_entry.Complains++; }
        updateRates();
    }

    public void updateRates()
    {
        var rate_root = GetNode<Control>("InfoRect/RatePanel/VBoxContainer/Rates/TextContainer/RatesContainer");
        rate_root.GetNode<Label>("UpvoteLabel").Text = $"+{world_entry.Upvotes}";
        rate_root.GetNode<Label>("DownvoteLabel").Text = $"-{world_entry.Downvotes}";
        if (!complain_visit) { GetNode<Button>("InfoRect/RatePanel/VBoxContainer/ComplainButton").Text = $"Mark as broken ({world_entry.Complains})"; }
    }

    private void _on_OptimizeButton_pressed()
    {
        ClickPlayer.Play();
        Task.Run(optimize);
        GetNode<Timer>("HintTimer").Start();
    }

    private void optimize()
    {
        string[] nodes_to_disable = { "InfoRect/BackButton", 
            "InfoRect/Slot1Button", "InfoRect/Slot2Button", "InfoRect/Slot3Button", 
            "InfoRect/RatePanel/VBoxContainer/OptimizeButton", "InfoRect/RatePanel/VBoxContainer/Uninstall/MainButton", 
            "InfoRect/RatePanel/VBoxContainer/Uninstall/ConfirmButton" };
        foreach (string node in nodes_to_disable) { GetNode<Button>(node).Disabled = true; }
        closeOtherSlots(-1);

        if (KWorld.BinMode)
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
        var un_root = GetNode<Control>("InfoRect/RatePanel/VBoxContainer/Uninstall");
        un_root.GetNode<Button>("MainButton").Visible = !show_confirm;
        un_root.GetNode<Button>("ConfirmButton").Visible = show_confirm;
        un_root.GetNode<Button>("CancelButton").Visible = show_confirm;
        un_root.GetNode<Button>(show_confirm ? "CancelButton" : "MainButton").GrabFocus();
    }

    private void _on_ConfirmButton_pressed()
    {
        KWorld.uninstallWorld();
        goBack();
        GetParent<LevelSelection>().reload();
    }
}
