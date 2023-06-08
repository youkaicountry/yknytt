using Godot;
using Godot.Collections;
using IniParser.Model;
using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using YKnyttLib;

public class LevelSelection : BasicScreeen
{
    private WorldManager Manager { get; }

    private PackedScene info_scene;

    string filter_category;
    string filter_difficulty;
    string filter_size;
    string filter_text;
    int filter_category_int;
    int filter_difficulty_int;
    int filter_size_int;
    int filter_order_int = 5; // Downloads by default

    GameContainer game_container;
    ScrollBar games_scrollbar;
    FileHTTPRequest http_node;
    HTTPRequest http_levels_node;
    GameButton download_button;

    public bool localLoad = false;
    private int requested_level = 0;
    private string next_page = null;

    ConcurrentQueue<WorldEntry> finished_entries;
    ConcurrentQueue<WorldEntry> remote_finished_entries; // TODO: process all found entries at once?

    public LevelSelection()
    {
        Manager = new WorldManager();
        finished_entries = new ConcurrentQueue<WorldEntry>();
        remote_finished_entries = new ConcurrentQueue<WorldEntry>();
    }

    public override void _Ready()
    {
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");

        game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        games_scrollbar = GetNode<ScrollContainer>("MainContainer/ScrollContainer").GetVScrollbar();
        if (!localLoad) { games_scrollbar.Connect("value_changed", this, nameof(_on_GameContainter_scrolling)); }
        http_node = GetNode<FileHTTPRequest>("FileHTTPRequest");
        http_levels_node = GetNode<HTTPRequest>("RestHTTPRequest");

        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/SortDropdown").Visible = localLoad;
        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/RemoteSortDropdown").Visible = !localLoad;

        loadDefaultWorlds();
        discoverWorlds("./worlds");
        discoverWorlds("user://Worlds");
        if (!localLoad) { HttpLoad(); }
    }

    public override void initFocus()
    {
        game_container.GrabFocus();
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
        GetNode<Button>("BackButton").GrabFocus();
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200) {; }
        else { connectionLost(); return; }

        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { connectionLost(); return; }

        var world_infos = HTTPUtil.jsonValue<Godot.Collections.Array>(json.Result, "results");
        if (world_infos == null) { connectionLost(); return; }
        connectionLost(lost: false);

        foreach (Dictionary record in world_infos)
        {
            if (record != null) { remote_finished_entries.Enqueue(generateRemoteWorld(record)); }
        }

        var worlds_total = HTTPUtil.jsonInt(json.Result, "count");
        game_container.fillStubs(worlds_total);

        requested_level = Math.Max(requested_level,
            2 * (((int)games_scrollbar.RectSize.y) / GameContainer.BUTTON_HEIGHT) + 2);

        next_page = HTTPUtil.jsonValue<string>(json.Result, "next");
        if (next_page != null && requested_level > game_container.GamesCount + world_infos.Count)
        {
            http_levels_node.Request(next_page);
            next_page = null;
        }
    }

    private void _on_GameContainter_scrolling(float value)
    {
        requested_level = 2 * (((int)(value + games_scrollbar.RectSize.y)) / GameContainer.BUTTON_HEIGHT) + 2;
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

    public override void _PhysicsProcess(float delta)
    {
        if (GetNode<Control>("MainContainer").GetFocusOwner() == null) { game_container.GrabFocus(); }
        
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

    private void loadDefaultWorlds()
    {
        binLoad("res://knytt/worlds/Nifflas - The Machine.knytt.bin");
        binLoad("res://knytt/worlds/Nifflas - Gustav's Daughter.knytt.bin");
        binLoad("res://knytt/worlds/Nifflas - Sky Flowerz.knytt.bin");
        binLoad("res://knytt/worlds/Nifflas - An Underwater Adventure.knytt.bin");
        binLoad("res://knytt/worlds/Nifflas - This Level is Unfinished.knytt.bin");
        binLoad("res://knytt/worlds/Nifflas - Tutorial.knytt.bin");
        if (OS.GetName() != "Android" && OS.GetName() != "iOS") {binLoad("/home/mike/sandbox/test - test.knytt.bin");}
    }

    // Search the given directory for worlds
    private void discoverWorlds(string path)
    {
        var wd = new Directory();
        if (!wd.DirExists(path)) { return; }
        var error = wd.Open(path);
        if (error != Error.Ok) { return; }

        wd.ListDirBegin(skipNavigational: true);
        while (true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }

            if (wd.CurrentIsDir())
            {
                if (!verifyDirWorld(wd, name)) { continue; }
                directoryLoad(wd.GetCurrentDir() + "/" + name);
            }
            else
            {
                if (!name.EndsWith(".knytt.bin")) { continue; }
                binLoad(wd.GetCurrentDir() + "/" + name);
            }
        }
        wd.ListDirEnd();
    }

    private void directoryLoad(string world_dir)
    {
        var entry = generateDirectoryWorld(world_dir);
        finished_entries.Enqueue(entry);
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
        if (new File().FileExists(ini_cache_name))
        {
            world_info = getWorldInfo(GDKnyttAssetManager.loadFile(ini_cache_name));
        }
        else
        {
            var ini_bin = GDKnyttAssetManager.loadFile(world_dir + "/world.ini");
            var ini_data = new IniData();
            world_info = getWorldInfo(ini_bin, merge_to: ini_data["World"]);
            GDKnyttAssetManager.ensureDirExists(cache_dir);
            var f = new File();
            f.Open(ini_cache_name, File.ModeFlags.Write);
            f.StoreString(ini_data.ToString());
            f.Close();
        }

        Texture icon = GDKnyttAssetManager.loadExternalTexture(world_dir + "/icon.png");
        var last_played = new File().FileExists(played_flag_name) ? new File().GetModifiedTime(played_flag_name) : 0;

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
        if (new Directory().DirExists(cache_dir))
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
            new Directory().MakeDir(cache_dir);
            var f = new File();
            f.Open(icon_cache_name, File.ModeFlags.Write);
            f.StoreBuffer(icon_bin);
            f.Close();

            var ini_data = new IniData();
            world_info = getWorldInfo(ini_bin, merge_to: ini_data["World"]);
            f.Open(ini_cache_name, File.ModeFlags.Write);
            f.StoreString(ini_data.ToString());
            f.Close();
        }

        Texture icon = GDKnyttAssetManager.loadTexture(icon_bin);
        var last_played = new File().FileExists(played_flag_name) ? new File().GetModifiedTime(played_flag_name) : 0;

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
        world_info.Name = HTTPUtil.jsonValue<string>(json_item, "name");
        world_info.Author = HTTPUtil.jsonValue<string>(json_item, "author");
        world_info.Description = HTTPUtil.jsonValue<string>(json_item, "description");
        var base64_icon = HTTPUtil.jsonValue<string>(json_item, "icon");
        world_info.Icon = base64_icon != null && base64_icon.Length > 0 ?
            GDKnyttAssetManager.loadTexture(decompress(Convert.FromBase64String(base64_icon))) : null;
        world_info.Link = HTTPUtil.jsonValue<string>(json_item, "link");
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

    private bool verifyDirWorld(Directory dir, string name)
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

    public void _on_GamePressed(GameButton button)
    {
        if (button.worldEntry.Path == null)
        {
            var timer = GetNode<Timer>("DownloadMonitor");
            if (!timer.IsStopped()) { return; }

            GDKnyttAssetManager.ensureDirExists("user://Worlds");
            cleanUnfinished();

            string filename = button.worldEntry.Link;
            filename = filename.Substring(filename.LastIndexOf('/') + 1);
            if (filename.IndexOf('?') != -1) { filename = filename.Substring(0, filename.IndexOf('?')); }
            if (!filename.EndsWith(".knytt.bin")) { filename += ".knytt.bin"; }
            filename = Uri.UnescapeDataString(filename);
            http_node.DownloadFile = $"user://Worlds/{filename}.part";

            var error = http_node.Request(button.worldEntry.Link);
            if (error != Error.Ok) { download_button.markFailed(); return; }

            timer.Start();
            enableFilter(false);

            download_button = button;
            button.setDownloaded(0);
        }
        else
        {
            var info_screen = info_scene.Instance() as InfoScreen;
            info_screen.initialize(button.worldEntry);
            loadScreen(info_screen);
        }
    }

    private void _on_DownloadMonitor_timeout()
    {
        download_button.setDownloaded(http_node.GetDownloadedBytes());
    }

    private void _on_FileHTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200)
        {
            string final_filename = http_node.DownloadFile.Substring(0, http_node.DownloadFile.Length - 5);
            new Directory().Rename(http_node.DownloadFile, final_filename);
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
                new Directory().Remove(final_filename);
                download_button.markFailed();
            }
        }
        else
        {
            download_button.markFailed();
            http_node.cleanup();
        }

        GetNode<Timer>("DownloadMonitor").Stop();
        enableFilter(true);
        http_node.DownloadFile = null;
        download_button = null;
    }

    private void sendDownload()
    {
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(
            download_button.worldEntry.Name, download_button.worldEntry.Author, (int)RateHTTPRequest.Action.Download);
    }

    private void cleanUnfinished()
    {
        var dir = new Directory();
        dir.Open("user://Worlds");
        dir.ListDirBegin(skipNavigational: true);
        for (string filename = dir.GetNext(); filename != ""; filename = dir.GetNext())
        {
            if (filename.EndsWith(".part") && dir.FileExists(filename)) { dir.Remove(filename); }
        }
    }

    public void reload()
    {
        game_container.clearWorlds();
        Manager.clearAll();
        loadDefaultWorlds();
        discoverWorlds("./worlds");
        discoverWorlds("user://Worlds");
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
        game_container.GrabFocus(); // no focus, but workaround in _PhysicsProcess fixes it
    }

    private void _on_SearchEdit_focus_entered()
    {
        if (OS.GetName() != "Android" && OS.GetName() != "iOS") { return; }
        GetNode<Control>("MainContainer").MoveChild(GetNode<Control>("MainContainer/FilterContainer"), 0);
    }

    private void _on_SearchEdit_focus_exited()
    {
        if (OS.GetName() != "Android" && OS.GetName() != "iOS") { return; }
        GetNode<Control>("MainContainer").MoveChild(GetNode<Control>("MainContainer/ScrollContainer"), 0);
    }

    private void _on_GameContainer_focus_entered()
    {
        if (game_container.GamesCount == 0)
        {
            GetNode<Button>("MainContainer/FilterContainer/Category/CategoryDropdown").GrabFocus();
            return;
        }
        game_container.GetChild(0).GetChild<GameButton>(0).GrabFocus();
        GetNode<ScrollContainer>("MainContainer/ScrollContainer").ScrollVertical = 0;
    }

    private void enableFilter(bool enable)
    {
        var parent = GetNode<Control>("MainContainer/FilterContainer");
        string[] childs = {"Category/CategoryDropdown", "Difficulty/DifficultyDropdown", "Size/SizeDropdown", "Sort/SortDropdown", "Sort/RemoteSortDropdown"};
        foreach (var child in childs) { parent.GetNode<OptionButton>(child).Disabled = !enable; }
        parent.GetNode<LineEdit>("Search/SearchEdit").Visible = enable;
    }
}
