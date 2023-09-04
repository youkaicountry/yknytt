using Godot;
using Godot.Collections;
using IniParser.Model;
using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YKnyttLib;

public class LevelSelection : BasicScreen
{
    private WorldManager Manager { get; }

    private PackedScene info_scene;

    string filter_category;
    string filter_difficulty;
    string filter_size;
    string filter_text;
    const int FILTER_ALL = 10;
    int filter_category_int = FILTER_ALL;
    int filter_difficulty_int = FILTER_ALL;
    int filter_size_int = FILTER_ALL;
    int filter_order_int = 5; // Downloads by default

    GameContainer game_container;
    ScrollBar games_scrollbar;
    FileHTTPRequest http_node;
    HTTPRequest http_levels_node;
    GameButton download_button;

    public bool localLoad = false;
    private int requested_level = 0;
    private string next_page = null;

    private int request_retry;
    private string request_url;

    private bool remotes_grab_focus = false;
    private Control prev_focus_owner;
    private double prev_scroll;

    ConcurrentQueue<WorldEntry> finished_entries;
    ConcurrentQueue<WorldEntry> remote_finished_entries;
    Task local_load_task;

    public LevelSelection()
    {
        Manager = new WorldManager();
        finished_entries = new ConcurrentQueue<WorldEntry>();
        remote_finished_entries = new ConcurrentQueue<WorldEntry>();
    }

    public async override void _Ready()
    {
        base._Ready();
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");

        game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        games_scrollbar = GetNode<ScrollContainer>("MainContainer/ScrollContainer").GetVScrollbar();
        if (!localLoad) { games_scrollbar.Connect("value_changed", this, nameof(_on_GameContainter_scrolling)); }
        http_node = GetNode<FileHTTPRequest>("FileHTTPRequest");
        http_levels_node = GetNode<HTTPRequest>("RestHTTPRequest");

        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/SortDropdown").Visible = localLoad;
        GetNode<OptionButton>("MainContainer/FilterContainer/Sort/RemoteSortDropdown").Visible = !localLoad;

        loadDefaultWorlds();
        if (OS.GetName() != "HTML5")
        {
            enableFilter(false);
            local_load_task = Task.Run(loadLocalWorlds);
        }
        else
        {
            loadLocalWorlds();
        }
        
        if (!localLoad) { HttpLoad(grab_focus: true); }
        
        if (local_load_task?.IsCompleted == false)
        {
            await local_load_task;
            if (IsInstanceValid(this)) { enableFilter(true); }
        }
    }

    public override void initFocus()
    {
        game_container.GrabFocus();
    }

    private void loadLocalWorlds()
    {
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
        discoverWorlds((OS.HasFeature("standalone") ? OS.GetExecutablePath().GetBaseDir() : ".").PlusFile("worlds"));
        if (OS.HasFeature("standalone")) { discoverWorlds(OS.GetExecutablePath().GetBaseDir()); }
        discoverWorlds("user://Worlds");
        if (GDKnyttSettings.WorldsDirectory != "") { discoverWorlds(GDKnyttSettings.WorldsDirectory); }
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
    }

    private void HttpLoad(bool grab_focus = false)
    {
        game_container.clearWorlds();
        requested_level = 0;
        game_container.fillStubs(4);
        games_scrollbar.Value = 0;
        connectionLost(lost: false);
        remotes_grab_focus = grab_focus;

        request_url = GDKnyttSettings.ServerURL + "/levels/?";
        if (filter_category_int != FILTER_ALL) { request_url += $"category={filter_category_int}&"; }
        if (filter_difficulty_int != FILTER_ALL) { request_url += $"difficulty={filter_difficulty_int}&"; }
        if (filter_size_int != FILTER_ALL) { request_url += $"size={filter_size_int}&"; }
        if (filter_text != null && filter_text != "") { request_url += $"text={Uri.EscapeDataString(filter_text)}&"; }
        request_url += $"order={filter_order_int}";

        http_levels_node.CancelRequest();
        request_retry = 1;
        var error = http_levels_node.Request(request_url);
        if (error != Error.Ok) { connectionLost(); }
    }

    private void connectionLost(bool lost = true)
    {
        GetNode<Label>("ConnectionLostLabel").Visible = lost;
        if (lost) { GetNode<Button>("BackButton").GrabFocus(); }
    }

    private async void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result != (int)HTTPRequest.Result.Success || response_code == 500)
        {
            if (request_retry-- <= 0) { connectionLost(); return; }
            http_levels_node.CancelRequest();
            http_levels_node.Request(request_url);
            return;
        }
        if (response_code != 200) { connectionLost(); return; }

        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { connectionLost(); return; }

        var world_infos = HTTPUtil.jsonValue<Godot.Collections.Array>(json.Result, "results");
        if (world_infos == null) { connectionLost(); return; }
        connectionLost(lost: false);

        if (local_load_task?.IsCompleted == false) { await local_load_task; }

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
            if (entry.Name == world_entry.Name && entry.Author == world_entry.Author && !entry.Disabled) { return entry; }
        }
        return null;
    }

    public override void _PhysicsProcess(float delta)
    {
        prev_scroll = games_scrollbar.Value;
        prev_focus_owner = game_container.GetFocusOwner();
        if (prev_focus_owner == null) { game_container.GrabFocus(); }

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
            var entry = findLocal(world_entry);
            if (entry != null) { world_entry.MergeLocal(entry); }
            bool focus = remotes_grab_focus && game_container.GamesCount == 0;
            remotes_grab_focus = false;
            game_container.addWorld(world_entry, focus: focus, mark_completed: entry != null);
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
        if (OS.GetName() == "HTML5")
        {
            binLoad("res://knytt/worlds/html5/Ania - The Cursed House.knytt.bin");
            //binLoad("res://knytt/worlds/html5/Ble - Timecraft 1.1.knytt.bin");
            binLoad("res://knytt/worlds/html5/Chironex - Afar.knytt.bin");
            binLoad("res://knytt/worlds/html5/dessgeega-TheLighthouse.knytt.bin");
            binLoad("res://knytt/worlds/html5/dessgeega-Fossil.knytt.bin");
            binLoad("res://knytt/worlds/html5/dessgeega-Torchlight.knytt.bin");
            binLoad("res://knytt/worlds/html5/dessgeega-UndertheCrack.knytt.bin");
            binLoad("res://knytt/worlds/html5/egomassive-AKnyttinTime1.5.knytt.bin");
            binLoad("res://knytt/worlds/html5/Introversity - Scrolly Polly Snow.knytt.bin");
            binLoad("res://knytt/worlds/html5/Diesel-Station07.knytt.bin");
        }
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
        string cache_dir = "user://Cache".PlusFile(world_dir.GetFile());
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
            ini_data["World"]["World Folder"] = world_dir.GetFile();
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

        string cache_dir = "user://Cache".PlusFile(world_dir.GetFile());
        string icon_cache_name = cache_dir + "/Icon.png";
        string ini_cache_name = cache_dir + "/World.ini";
        string played_flag_name = cache_dir + "/LastPlayed.flag";
        if (new Directory().FileExists(ini_cache_name))
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
            ini_data["World"]["World Folder"] = binloader.RootDirectory;
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
        var category_option = GetNode<OptionButton>("MainContainer/FilterContainer/Category/CategoryDropdown");
        var difficulty_option = GetNode<OptionButton>("MainContainer/FilterContainer/Difficulty/DifficultyDropdown");
        var size_option = GetNode<OptionButton>("MainContainer/FilterContainer/Size/SizeDropdown");
        var base64_icon = HTTPUtil.jsonValue<string>(json_item, "icon");
        int size_index = HTTPUtil.jsonInt(json_item, "size");
        return new WorldEntry()
        {
            HasServerInfo = true,
            Name = HTTPUtil.jsonValue<string>(json_item, "name"),
            Author = HTTPUtil.jsonValue<string>(json_item, "author"),
            Description = HTTPUtil.jsonValue<string>(json_item, "description"),
            Icon = base64_icon != null && base64_icon.Length > 0 ?
                        GDKnyttAssetManager.loadTexture(decompress(Convert.FromBase64String(base64_icon))) : null,
            Link = HTTPUtil.jsonValue<string>(json_item, "link"),
            FileSize = HTTPUtil.jsonInt(json_item, "file_size"),
            Upvotes = HTTPUtil.jsonInt(json_item, "upvotes"),
            Downvotes = HTTPUtil.jsonInt(json_item, "downvotes"),
            Downloads = HTTPUtil.jsonInt(json_item, "downloads"),
            Complains = HTTPUtil.jsonInt(json_item, "complains"),
            AutoVerified = HTTPUtil.jsonBool(json_item, "autoverified"),
            Status = HTTPUtil.jsonInt(json_item, "status"),
            Categories = HTTPUtil.jsonValue<Godot.Collections.Array>(json_item, "category")
                .Cast<float>().Where(i => i > 0 && i < category_option.GetItemCount())
                .Select(i => category_option.GetItemText((int)i)).ToList(),
            Difficulties = HTTPUtil.jsonValue<Godot.Collections.Array>(json_item, "difficulty")
                .Cast<float>().Where(i => i > 0 && i < difficulty_option.GetItemCount())
                .Select(i => difficulty_option.GetItemText((int)i)).ToList(),
            Size = size_index > 0 && size_index < size_option.GetItemCount() ? size_option.GetItemText(size_index) : ""
        };
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
            filename = Uri.UnescapeDataString(filename).Replace("+", " ");
            string download_dir = GDKnyttSettings.WorldsDirectory != "" && GDKnyttSettings.WorldsDirForDownload ? 
                GDKnyttSettings.WorldsDirectory : "user://Worlds";
            http_node.DownloadFile = download_dir.PlusFile($"{filename}.part");

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
            download_button.worldEntry.Name, download_button.worldEntry.Author, (int)RateHTTPRequest.Action.Download, once: false);
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

    public void disableButton(WorldEntry entry)
    {
        Manager.removeWorld(entry);
        var local = findLocal(entry);
        if (local != null) { local.Disabled = true; }
        foreach (var hbox in game_container.GetChildren())
        {
            foreach (GameButton button in (hbox as Control).GetChildren())
            {
                if (button.worldEntry == entry)
                {
                    button.Disabled = true;
                    button.markDisabled();
                    return;
                }
            }
        }
    }

    public void _on_CategoryDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Category/CategoryDropdown");
        filter_category = dropdown.Text.Equals("[All]") ? null : 
            dropdown.Text.Equals("Other") ? "" : dropdown.Text;
        filter_category_int = dropdown.GetSelectedId();
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    public void _on_DifficultyDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Difficulty/DifficultyDropdown");
        filter_difficulty = dropdown.Text.Equals("[All]") ? null : 
            dropdown.Text.Equals("Other") ? "" : dropdown.Text;
        filter_difficulty_int = dropdown.GetSelectedId();
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(); }
    }

    public void _on_SizeDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/Size/SizeDropdown");
        filter_size = dropdown.Text.Equals("[All]") ? null : 
            dropdown.Text.Equals("Other") ? "" : dropdown.Text;
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
        if (localLoad) { this.listWorlds(); } else { this.HttpLoad(grab_focus: true); }
        game_container.GrabFocus(); // no focus, but workaround in _PhysicsProcess fixes it
    }

    private void _on_SearchEdit_focus_entered()
    {
        if (!GDKnyttSettings.Mobile) { return; }
        GetNode<Control>("MainContainer").MoveChild(GetNode<Control>("MainContainer/FilterContainer"), 0);
    }

    private void _on_SearchEdit_focus_exited()
    {
        if (!GDKnyttSettings.Mobile) { return; }
        GetNode<Control>("MainContainer").MoveChild(GetNode<Control>("MainContainer/ScrollContainer"), 0);
    }

    private void _on_GameContainer_focus_entered()
    {
        if (game_container.GamesCount == 0)
        {
            GetNode<Button>("MainContainer/FilterContainer/Category/CategoryDropdown").GrabFocus();
            return;
        }
        if (prev_focus_owner != null && game_container.IsAParentOf(prev_focus_owner)) // to prevent scrolling to top
        {
            prev_focus_owner.GrabFocus();
            games_scrollbar.Value = prev_scroll;
            return;
        }
        game_container.GetChild(0).GetChild<GameButton>(0).forceGrabFocus();
        games_scrollbar.Value = 0;
    }

    private void enableFilter(bool enable)
    {
        var parent = GetNode<Control>("MainContainer/FilterContainer");
        string[] childs = {"Category/CategoryDropdown", "Difficulty/DifficultyDropdown", "Size/SizeDropdown", "Sort/SortDropdown", "Sort/RemoteSortDropdown"};
        foreach (var child in childs) { parent.GetNode<OptionButton>(child).Disabled = !enable; }
        parent.GetNode<LineEdit>("Search/SearchEdit").Editable = enable;
    }
}
