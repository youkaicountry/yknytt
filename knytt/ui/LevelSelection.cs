using Godot;
using Godot.Collections;
using IniParser.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using YKnyttLib;

public partial class LevelSelection : CanvasLayer
{
    private WorldManager Manager { get; }

    private PackedScene info_scene;

    [Export] public int loadThreads = 4;

    string filter_category;
    string filter_difficulty;
    string filter_size;
    string filter_text;
    int filter_category_int;
    int filter_difficulty_int;
    int filter_size_int;
    int filter_order_int = 5; // Downloads by default

    bool halt_consumers = false;
    bool discovery_over = false;
    List<Task> consumers;

    GameContainer game_container;
    ScrollBar games_scrollbar;
    FileHTTPRequest http_node;
    HttpRequest http_levels_node;
    GameButton download_button;

    public bool localLoad = false;
    private int requested_level = 0;
    private string next_page = null;

    ConcurrentQueue<WorldEntry> finished_entries;
    ConcurrentQueue<WorldEntry> remote_finished_entries; // TODO: process all found entries at once?
    ConcurrentQueue<Action> load_hopper;

    public LevelSelection()
    {
        Manager = new WorldManager();
        finished_entries = new ConcurrentQueue<WorldEntry>();
        remote_finished_entries = new ConcurrentQueue<WorldEntry>();
        load_hopper = new ConcurrentQueue<Action>();
        consumers = new List<Task>();
    }

    public override void _Ready()
    {
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");

        game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        games_scrollbar = GetNode<ScrollContainer>("MainContainer/ScrollContainer").GetVScrollBar();
        if (!localLoad) { games_scrollbar.ValueChanged += _on_GameContainter_scrolling; }
        http_node = GetNode<FileHTTPRequest>("FileHTTPRequest");
        http_levels_node = GetNode<HttpRequest>("RestHTTPRequest");

        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/SortDropdown").Visible = localLoad;
        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/RemoteSortDropdown").Visible = !localLoad;

        var sys = OS.GetName();
        //if (sys.Equals("Android") || sys.Equals("HTML5") || sys.Equals("iOS")) { singleThreadedLoad(); }
        //else { multiThreadedLoad(); }
        singleThreadedLoad();
    }

    private void singleThreadedLoad()
    {
        loadDefaultWorlds(true);
        discoverWorlds("./worlds", true);
        discoverWorlds("user://Worlds", true);
        if (!localLoad) { HttpLoad(); }
    }

    private void multiThreadedLoad()
    {
        startHopperConsumers();

        Task.Run(() => this.loadDefaultWorlds(false));
        Task.Run(() => this.discoverWorlds("./worlds", false));
    }

    private void startHopperConsumers()
    {
        Action consumer = () =>
        {
            while (true)
            {
                Action action;
                if (load_hopper.Count > 0 && load_hopper.TryDequeue(out action))
                {
                    var t = Task.Run(action);
                    t.Wait();
                }
                else
                {
                    var t = Task.Delay(5);
                    t.Wait();
                    if (discovery_over) { break; }
                }

                if (halt_consumers) { break; }
            }
        };

        for (int i = 0; i < loadThreads; i++)
        {
            consumers.Add(Task.Run(consumer));
        }
    }

    public void killConsumers()
    {
        halt_consumers = true;
        Task.WaitAll(consumers.ToArray());
    }

    private void HttpLoad()
    {
        game_container.clearWorlds();
        requested_level = 0;
        game_container.fillStubs(4);
        GetNode<ScrollContainer>("MainContainer/ScrollContainer").ScrollVertical = 0;
        connectionLost(lost: false);

        string url = GDKnyttSettings.ServerURL + "/levels/?";
        if (filter_category_int != 0) { url += $"category={filter_category_int}&"; }
        if (filter_difficulty_int != 0) { url += $"difficulty={filter_difficulty_int}&"; }
        if (filter_size_int != 0) { url += $"size={filter_size_int}&"; }
        if (filter_text != null && filter_text != "") { url += $"text={Uri.EscapeDataString(filter_text)}&"; }
        url += $"order={filter_order_int}";

        http_levels_node.CancelRequest();
        var error = http_levels_node.Request(url);
        if (error != Error.Ok) { connectionLost(); }
    }

    private void connectionLost(bool lost = true)
    {
        GetNode<Label>("ConnectionLostLabel").Visible = lost;
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        GD.Print($"{result} {response_code}");
        if (result == (int)HttpRequest.Result.Success && response_code == 200) {; }
        else { connectionLost(); return; }

        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var jsonObject = new Json();
        var error = jsonObject.Parse(response);
        if (error != Error.Ok) { connectionLost(); return; }

        var world_infos = HTTPUtil.jsonArray(jsonObject.Data, "results");
        if (world_infos == null) { connectionLost(); return; }
        connectionLost(lost: false);

        foreach (Dictionary record in world_infos)
        {
            if (record != null) { remote_finished_entries.Enqueue(generateRemoteWorld(record)); }
        }

        var worlds_total = HTTPUtil.jsonInt(jsonObject.Data, "count");
        game_container.fillStubs(worlds_total);

        requested_level = Math.Max(requested_level,
            2 * (((int)games_scrollbar.Size.Y) / GameContainer.BUTTON_HEIGHT) + 2);

        next_page = HTTPUtil.jsonString(jsonObject.Data, "next");
        if (next_page != null && requested_level > game_container.GamesCount + world_infos.Count)
        {
            http_levels_node.Request(next_page);
            next_page = null;
        }
    }

    private void _on_GameContainter_scrolling(double value)
    {
        requested_level = 2 * (((int)(value + games_scrollbar.Size.Y)) / GameContainer.BUTTON_HEIGHT) + 2;
        if (next_page != null && requested_level > game_container.GamesCount && http_levels_node.GetHttpClientStatus() == 0)
        {
            http_levels_node.Request(next_page);
            next_page = null;
        }
    }

    private WorldEntry findLocal(WorldEntry world_entry)
    {
        foreach (var entry in finished_entries)
        {
            if (entry.Name == world_entry.Name && entry.Author == world_entry.Author) { return entry; }
        }
        return null;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Process the queue
        if ((localLoad && finished_entries.Count == 0) ||
            (!localLoad && remote_finished_entries.Count == 0)) { return; }

        if (localLoad)
        {
            if (!finished_entries.TryDequeue(out var entry)) { return; }
            if (Manager.addWorld(entry))
            {
                game_container.addWorld(entry, focus: game_container.Count == 0);
            }
        }
        else
        {
            if (!remote_finished_entries.TryDequeue(out var world_entry)) { return; }
            var entry = findLocal(world_entry); // TODO: with multithreading local level can be added later than remote, and it won't be shown as downloaded
            if (entry != null) { world_entry.MergeLocal(entry); }
            game_container.addWorld(world_entry, focus: game_container.Count == 0, mark_completed: entry != null);
        }
    }

    private void loadDefaultWorlds(bool single_threaded)
    {
        startBinLoad("res://knytt/worlds/Nifflas - The Machine.knytt.bin", single_threaded);
        startBinLoad("res://knytt/worlds/Nifflas - Gustav's Daughter.knytt.bin", single_threaded);
        startBinLoad("res://knytt/worlds/Nifflas - Sky Flowerz.knytt.bin", single_threaded);
        startBinLoad("res://knytt/worlds/Nifflas - An Underwater Adventure.knytt.bin", single_threaded);
        startBinLoad("res://knytt/worlds/Nifflas - This Level is Unfinished.knytt.bin", single_threaded);
        startBinLoad("res://knytt/worlds/Nifflas - Tutorial.knytt.bin", single_threaded);
    }

    // Search the given directory for worlds
    private void discoverWorlds(string path, bool single_threaded)
    {
        var wd = DirAccess.Open(path);
        if (wd == null) { discovery_over = true; return; }

        wd.IncludeNavigational = false;
        wd.ListDirBegin();
        while (true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }

            if (wd.CurrentIsDir())
            {
                if (!verifyDirWorld(wd, name)) { continue; }
                startDirectoryLoad(wd.GetCurrentDir() + "/" + name, single_threaded);
            }
            else
            {
                if (!name.EndsWith(".knytt.bin")) { continue; }
                startBinLoad(wd.GetCurrentDir() + "/" + name, single_threaded);
            }
        }
        wd.ListDirEnd();
        discovery_over = true;
    }

    private void startDirectoryLoad(string world_dir, bool single_threaded)
    {
        if (single_threaded) { directoryLoad(world_dir); return; }

        Action action = () => { directoryLoad(world_dir); };
        load_hopper.Enqueue(action);
    }

    private void directoryLoad(string world_dir)
    {
        var entry = generateDirectoryWorld(world_dir);
        finished_entries.Enqueue(entry);
    }

    private void startBinLoad(string world_dir, bool single_threaded)
    {
        if (single_threaded) { binLoad(world_dir); return; }

        Action action = () => { binLoad(world_dir); };
        load_hopper.Enqueue(action);
    }

    private void binLoad(string world_dir)
    {
        var entry = generateBinWorld(world_dir);
        if (entry == null) { return; }
        finished_entries.Enqueue(entry);
    }

    private WorldEntry generateDirectoryWorld(string world_dir)
    {
        KnyttWorldInfo world_info;
        string cache_dir = "user://Cache/" + GDKnyttAssetManager.extractFilename(world_dir);
        string ini_cache_name = cache_dir + "/World.ini";
        string played_flag_name = cache_dir + "/LastPlayed.flag";
        if (FileAccess.FileExists(ini_cache_name))
        {
            world_info = getWorldInfo(GDKnyttAssetManager.loadFile(ini_cache_name));
        }
        else
        {
            var ini_bin = GDKnyttAssetManager.loadFile(world_dir + "/world.ini");
            var ini_data = new IniData();
            world_info = getWorldInfo(ini_bin, merge_to: ini_data["World"]);
            GDKnyttAssetManager.ensureDirExists(cache_dir);
            using var f = FileAccess.Open(ini_cache_name, FileAccess.ModeFlags.Write);
            f.StoreString(ini_data.ToString());
        }

        Texture2D icon = GDKnyttAssetManager.loadExternalTexture(world_dir + "/icon.png");
        var last_played = FileAccess.FileExists(played_flag_name) ? FileAccess.GetModifiedTime(played_flag_name) : 0;

        return new WorldEntry(icon, world_info, world_dir, last_played);
    }

    private WorldEntry generateBinWorld(string world_dir)
    {
        byte[] icon_bin;
        KnyttWorldInfo world_info;

        string cache_dir = "user://Cache/" + GDKnyttAssetManager.extractFilename(world_dir);
        string icon_cache_name = cache_dir + "/Icon.png";
        string ini_cache_name = cache_dir + "/World.ini";
        string played_flag_name = cache_dir + "/LastPlayed.flag";
        if (DirAccess.DirExistsAbsolute(cache_dir))
        {
            icon_bin = GDKnyttAssetManager.loadFile(icon_cache_name);
            world_info = getWorldInfo(GDKnyttAssetManager.loadFile(ini_cache_name));
        }
        else
        {
            KnyttBinWorldLoader binloader;
            byte[] ini_bin;
            try
            {
                binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world_dir));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            icon_bin = binloader.GetFile("Icon.png");
            ini_bin = binloader.GetFile("World.ini");

            GDKnyttAssetManager.ensureDirExists("user://Cache");
            DirAccess.MakeDirAbsolute(cache_dir);
            using var icon_file = FileAccess.Open(icon_cache_name, FileAccess.ModeFlags.Write);
            icon_file.StoreBuffer(icon_bin);

            var ini_data = new IniData();
            world_info = getWorldInfo(ini_bin, merge_to: ini_data["World"]);
            using var ini_cache_file = FileAccess.Open(ini_cache_name, FileAccess.ModeFlags.Write);
            ini_cache_file.StoreString(ini_data.ToString());
        }

        Texture2D icon = GDKnyttAssetManager.loadTexture(icon_bin);
        var last_played = FileAccess.FileExists(played_flag_name) ? FileAccess.GetModifiedTime(played_flag_name) : 0;

        return new WorldEntry(icon, world_info, world_dir, last_played);
    }

    private KnyttWorldInfo getWorldInfo(byte[] ini_bin, KeyDataCollection merge_to = null)
    {
        string ini = GDKnyttAssetManager.loadTextFile(ini_bin);
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.loadWorldConfig(ini);
        if (merge_to != null) { merge_to.Merge(world.INIData["World"]); }
        return world.Info;
    }

    private WorldEntry generateRemoteWorld(Dictionary json_item)
    {
        WorldEntry world_info = new WorldEntry();
        world_info.HasServerInfo = true;
        world_info.Name = HTTPUtil.jsonString(json_item, "name");
        world_info.Author = HTTPUtil.jsonString(json_item, "author");
        world_info.Description = HTTPUtil.jsonString(json_item, "description");
        var base64_icon = HTTPUtil.jsonString(json_item, "icon");
        world_info.Icon = base64_icon != null && base64_icon.Length > 0 ?
            GDKnyttAssetManager.loadTexture(decompress(Convert.FromBase64String(base64_icon))) : null;
        world_info.Link = HTTPUtil.jsonString(json_item, "link");
        world_info.FileSize = HTTPUtil.jsonInt(json_item, "file_size");
        world_info.Upvotes = HTTPUtil.jsonInt(json_item, "upvotes");
        world_info.Downvotes = HTTPUtil.jsonInt(json_item, "downvotes");
        world_info.Downloads = HTTPUtil.jsonInt(json_item, "downloads");
        world_info.Complains = HTTPUtil.jsonInt(json_item, "complains");
        world_info.Verified = HTTPUtil.jsonBool(json_item, "verified");
        world_info.Approved = HTTPUtil.jsonBool(json_item, "approved");
        return world_info;
    }

    private static byte[] decompress(byte[] file)
    {
        if (file[0] != 31 || file[1] != 139) { return file; }
        var output = new System.IO.MemoryStream();
        using (var gzip = new GZipStream(new System.IO.MemoryStream(file), CompressionMode.Decompress))
        {
            gzip.CopyTo(output);
        }
        return output.ToArray();
    }

    private bool verifyDirWorld(DirAccess dir, string name)
    {
        if (name.Equals(".") || name.Equals("..")) { return false; }
        if (dir.FileExists($"{name}/_do_not_load_")) { return false; }
        if (!dir.FileExists($"{name}/map.bin") || !dir.FileExists($"{name}/world.ini")) { return false; }
        return true;
    }

    private void listWorlds()
    {
        // Should take filter settings into account
        Manager.setFilter(filter_category, filter_difficulty, filter_size, filter_text, (WorldManager.Order)filter_order_int);
        var worlds = Manager.Filtered;
        game_container.clearWorlds();
        requested_level = 0;

        foreach (var world_entry in worlds)
        {
            game_container.addWorld(world_entry, false);
        }
    }

    public void _on_BackButton_pressed()
    {
        killConsumers();
        ClickPlayer.Play();
        this.QueueFree();
    }

    public void _on_GamePressed(GameButton button)
    {
        if (button.worldEntry.Path == null)
        {
            var timer = GetNode<Timer>("DownloadMonitor");
            if (!timer.IsStopped()) { return; }

            GDKnyttAssetManager.ensureDirExists("user://Worlds");

            string filename = button.worldEntry.Link;
            filename = filename.Substring(filename.LastIndexOf('/') + 1);
            if (filename.IndexOf('?') != -1) { filename = filename.Substring(0, filename.IndexOf('?')); }
            if (!filename.EndsWith(".knytt.bin")) { filename += ".knytt.bin"; }
            filename = Uri.UnescapeDataString(filename);
            http_node.DownloadFile = $"user://Worlds/{filename}.part";

            var error = http_node.Request(button.worldEntry.Link);
            if (error != Error.Ok) { download_button.markFailed(); return; }

            timer.Start();

            download_button = button;
            button.setDownloaded(0);
        }
        else
        {
            ClickPlayer.Play();
            var info_screen = info_scene.Instantiate<InfoScreen>();
            info_screen.initialize(button.worldEntry.Path);
            info_screen.worldEntry = button.worldEntry;
            this.GetParent().AddChild(info_screen);
        }
    }

    private void _on_DownloadMonitor_timeout()
    {
        download_button.setDownloaded(http_node.GetDownloadedBytes());
    }

    private void _on_FileHTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HttpRequest.Result.Success && response_code == 200)
        {
            string final_filename = http_node.DownloadFile.Substring(0, http_node.DownloadFile.Length - 5);
            DirAccess.RenameAbsolute(http_node.DownloadFile, final_filename);
            var entry = generateBinWorld(final_filename);

            if (entry != null)
            {
                download_button.markCompleted();
                download_button.incDownloads(); // TODO: increment count only on first download (use RateAdded event)
                download_button.worldEntry.MergeLocal(entry);
                finished_entries.Enqueue(entry);
                sendDownload();
            }
            else
            {
                DirAccess.RemoveAbsolute(final_filename);
                download_button.markFailed();
            }
        }
        else
        {
            download_button.markFailed();
            http_node.cleanup();
        }

        GetNode<Timer>("DownloadMonitor").Stop();
        http_node.DownloadFile = null;
        download_button = null;
    }

    private void sendDownload()
    {
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(
            download_button.worldEntry.Name, download_button.worldEntry.Author, (int)RateHTTPRequest.Action.Download);
    }

    public void _on_CategoryDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Category/CategoryDropdown");
        filter_category = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        filter_category_int = dropdown.GetSelectedId();
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    public void _on_DifficultyDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Difficulty/DifficultyDropdown");
        filter_difficulty = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        filter_difficulty_int = dropdown.GetSelectedId();
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    public void _on_SizeDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Size/SizeDropdown");
        filter_size = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        filter_size_int = dropdown.GetSelectedId();
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    private void _on_SortDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Sort/SortDropdown");
        filter_order_int = dropdown.GetSelectedId();
        this.listWorlds();
    }

    private void _on_RemoteSortDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Sort/RemoteSortDropdown");
        filter_order_int = dropdown.GetSelectedId();
        this.HttpLoad();
    }

    private void _on_SearchEdit_text_changed(string new_text)
    {
        GetNode<Timer>("SearchEditTimer").Start();
    }

    private void _on_SearchEditTimer_timeout()
    {
        filter_text = GetNode<LineEdit>("MainContainer/FilterContainer/Search/SearchEdit").Text;
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    private void _on_SearchEdit_text_entered(string new_text)
    {
        filter_text = new_text;
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }
}
