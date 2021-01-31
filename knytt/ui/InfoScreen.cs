using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using YKnyttLib;

public class InfoScreen : CanvasLayer
{
    [Export] string complainURL;

    public GDKnyttWorldImpl KWorld { get; private set; }

    public void initialize(GDKnyttWorldImpl world)
    {
        this.KWorld = world;
        if (world.BinMode)
        {
            var loader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world.WorldDirectory));
            world.setBinMode(loader);
            world.setDirectory(world.WorldDirectory, loader.RootDirectory); // only WorldDirectory was set earlier
        }
        Texture info = (world.worldFileExists("Info+.png") ? world.getWorldTexture("Info+.png") : 
                                                             world.getWorldTexture("Info.png")) as Texture;
        info.Flags |= (uint)Texture.FlagsEnum.Filter;
        GetNode<TextureRect>("InfoRect").Texture = (Texture)info;
        GetNode<SlotButton>("InfoRect/Slot1Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot2Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot3Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
    }

    public void _on_BackButton_pressed()
    {
        ClickPlayer.Play();
        KWorld.purgeBinFile();
        this.QueueFree();
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
        GetNode<LevelSelection>("../LevelSelection").killConsumers();
        
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
        string serverURL = GDKnyttSettings.getServerURL();
        GetNode<HTTPRequest>("StatHTTPRequest").Request(
            $"{serverURL}/rating/?name={Uri.EscapeDataString(KWorld.Info.Name)}&author={Uri.EscapeDataString(KWorld.Info.Author)}");
    }

    private void _on_StatHTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200) { ; } else { return; }
        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { return; }

        upvotes = HTTPUtil.jsonInt(json.Result, "upvotes");
        downvotes = HTTPUtil.jsonInt(json.Result, "downvotes");
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
            for (int i = 0; i <= 12; i++)
            {
                if (powers_count[i] > 0)
                {
                    stat_panel.addPower(i, powers_count[i]);
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
            bool is_ending = (bool)record["ending"];
            (is_ending ? endings : cutscenes).Add(HTTPUtil.jsonValue<string>(record, "name"));
            (is_ending ? endings_count : cutscenes_count).Add(HTTPUtil.jsonInt(record, "counter"));
        }

        if (endings.Count > 0)
        {
            stat_panel.addLabel("Endings:");
            foreach (var p in endings.Zip(endings_count, (a, b) => new {Name = a, Count = b}))
            {
                stat_panel.addEnding(p.Name, p.Count);
            }
        }

        if (cutscenes.Count > 0)
        {
            stat_panel.addLabel("Cutscenes:");
            foreach (var p in cutscenes.Zip(cutscenes_count, (a, b) => new {Name = a, Count = b}))
            {
                stat_panel.addCutscene(p.Name, p.Count);
            }
        }
    }

    private void _on_StatsButton_pressed()
    {
        var panel = GetNode<Panel>("InfoRect/StatPanel");
        panel.Visible = !panel.Visible;
    }

    private int upvotes;
    private int downvotes;

    public GameButtonInfo ButtonInfo
    {
        set
        {
            upvotes = value.Upvotes;
            downvotes = value.Downvotes;
            updateRates();
        }
    }

    private void _on_UpvoteButton_pressed()
    {
        sendRating((int)RateHTTPRequest.Action.Upvote);
    }

    private void _on_DownvoteButton_pressed()
    {
        sendRating((int)RateHTTPRequest.Action.Downvote);
    }

    private void _on_ComplainButton_pressed()
    {
        sendRating((int)RateHTTPRequest.Action.Complain);
        OS.ShellOpen(complainURL);
    }

    private void sendRating(int action)
    {
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(KWorld.Info.Name, KWorld.Info.Author, action);
    }

    private void _on_RateHTTPRequest_RateAdded(int action)
    {
        if (action == (int)RateHTTPRequest.Action.Upvote) { upvotes++; }
        if (action == (int)RateHTTPRequest.Action.Downvote) { downvotes++; }
        updateRates();
    }

    public void updateRates()
    {
        var rate_root = GetNode<Control>("InfoRect/RatePanel/VBoxContainer/Rates");
        rate_root.GetNode<Label>("UpvoteLabel").Text = $"+{upvotes}";
        rate_root.GetNode<Label>("DownvoteLabel").Text = $"-{downvotes}";
    }

    private void _on_OptimizeButton_pressed()
    {
        // TODO: waiting animation
        if (KWorld.BinMode) { KWorld.unpackWorld(); }
        GDKnyttAssetManager.compileInternalTileset(KWorld, recompile: true); // To fix errors if chromakey and alpha channel used together
    }
}
