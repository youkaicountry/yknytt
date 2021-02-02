using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO.Compression;
using Godot;
using Godot.Collections;
using YKnyttLib;

public class LevelSelection : CanvasLayer
{
    KnyttWorldManager<Texture> Manager { get; }
    PackedScene info_scene;

    [Export] public int loadThreads = 4;

    string filter_category;
    string filter_difficulty;
    string filter_size;
    int filter_category_int;
    int filter_difficulty_int;
    int filter_size_int;

    bool halt_consumers = false;
    bool discovery_over = false;
    List<Task> consumers;

    GameButton top_button;
    GameContainer game_container;
    ScrollBar games_scrollbar;
    FileHTTPRequest http_node;
    GameButton download_button;

    public bool localLoad = false;
    private string next_page = null;

    ConcurrentQueue<KnyttWorldManager<Texture>.WorldEntry> finished_entries;
    ConcurrentQueue<GameButtonInfo> remote_finished_entries; // TODO: process all found entries at once?
    ConcurrentQueue<Action> load_hopper;

    public LevelSelection()
    {
        Manager = new KnyttWorldManager<Texture>();
        finished_entries = new ConcurrentQueue<KnyttWorldManager<Texture>.WorldEntry>();
        remote_finished_entries = new ConcurrentQueue<GameButtonInfo>();
        load_hopper = new ConcurrentQueue<Action>();
        consumers = new List<Task>();
    }

    public override void _Ready()
    {
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");

        game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        games_scrollbar = GetNode<ScrollContainer>("MainContainer/ScrollContainer").GetVScrollbar();
        if (!localLoad) { games_scrollbar.Connect("value_changed", this, nameof(_on_GameContainter_scrolling)); }
        http_node = GetNode<FileHTTPRequest>("FileHTTPRequest");

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
        GetNode<Label>("ConnectionLostLabel").Visible = false;

        string url = GDKnyttSettings.getServerURL() + "/levels/?";
        if (filter_category_int != 0) { url += $"category={filter_category_int}&"; }
        if (filter_difficulty_int != 0) { url += $"difficulty={filter_difficulty_int}&"; }
        if (filter_size_int != 0) { url += $"size={filter_size_int}&"; }

        var error = GetNode<HTTPRequest>("RestHTTPRequest").Request(url);
        if (error != Error.Ok) { connectionLost(); }
    }

    private void connectionLost()
    {
        GetNode<Label>("ConnectionLostLabel").Visible = true;
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200) { ; }
        else { connectionLost(); return; }

        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { connectionLost(); return; }

        var world_infos = HTTPUtil.jsonValue<Godot.Collections.Array>(json.Result, "results");
        if (world_infos == null) { connectionLost(); return; }

        foreach (Dictionary record in world_infos)
        {
            if (record != null) { remote_finished_entries.Enqueue(generateRemoteWorld(record)); }
        }

        next_page = HTTPUtil.jsonValue<string>(json.Result, "next");
    }

    private void _on_GameContainter_scrolling(float value)
    {
        if (next_page != null && value + games_scrollbar.RectSize.y >= games_scrollbar.MaxValue)
        {
            GetNode<HTTPRequest>("RestHTTPRequest").Request(next_page);
            next_page = null;
        }
    }

    private KnyttWorldManager<Texture>.WorldEntry? findLocal(GameButtonInfo button_info)
    {
        foreach (var entry in finished_entries)
        {
            if (entry.world.Info.Name == button_info.Name && entry.world.Info.Author == button_info.Author) { return entry; }
        }
        return null;
    }

    public override void _PhysicsProcess(float delta)
    {
        // Process the queue
        if ((localLoad && finished_entries.Count == 0) || 
            (!localLoad && remote_finished_entries.Count == 0)) { return; }

        if (localLoad)
        {
            if (!finished_entries.TryDequeue(out var entry)) { return; }
            if (Manager.addWorld(entry))
            {
                game_container.addWorld((GDKnyttWorldImpl)entry.world, entry.extra_data, game_container.Count == 0);
            }
        }
        else
        {
            if (!remote_finished_entries.TryDequeue(out var button_info)) { return; }
            var entry = findLocal(button_info); // TODO: with multithreading local level can be added later than remote, and it won't be shown as downloaded
            game_container.addWorld(button_info, entry != null ? (GDKnyttWorldImpl)entry.Value.world : null, mark_completed: entry != null);
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
    // TODO: This should do a very different process for bin files
    private void discoverWorlds(string path, bool single_threaded)
    {
        var wd = new Directory();
        //if (!wd.DirExists(path)) { discovery_over = true; return; }
        var error = wd.Open(path);
        if (error != Error.Ok) { discovery_over = true; return; }

        wd.ListDirBegin();
        while(true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }
            
            if (wd.CurrentIsDir())
            {
                if (!verifyDirWorld(wd, name)) { continue; }
                startDirectoryLoad(wd.GetCurrentDir() + "/" + name, name, single_threaded);
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

    private void startDirectoryLoad(string world_dir, string name, bool single_threaded)
    {
        if (single_threaded) { directoryLoad(world_dir, name); return; }

        Action action = () => { directoryLoad(world_dir, name); };
        load_hopper.Enqueue(action);
    }

    private void directoryLoad(string world_dir, string name)
    {
        var entry = generateDirectoryWorld(world_dir, name);
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
        finished_entries.Enqueue(entry);
    }

    object file_lock = new object();

    private KnyttWorldManager<Texture>.WorldEntry generateDirectoryWorld(string world_dir, string name)
    {
        Texture icon;
        string txt;
        lock(file_lock) {icon = GDKnyttAssetManager.loadExternalTexture(world_dir + "/icon.png");}
        lock(file_lock) {txt = GDKnyttAssetManager.loadTextFile(world_dir + "/world.ini");}
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.setDirectory(world_dir, name);
        world.loadWorldConfig(txt);
        return new KnyttWorldManager<Texture>.WorldEntry(world, icon);
    }

    private KnyttWorldManager<Texture>.WorldEntry generateBinWorld(string world_dir)
    {
        byte[] icon_bin;
        byte[] ini_bin;

        string filename = world_dir.Substring(world_dir.LastIndexOf('/') + 1);
        string cache_dir = "user://Cache/" + filename;
        string icon_cache_name = cache_dir + "/Icon.png";
        string ini_cache_name = cache_dir + "/World.ini";
        if (new Directory().DirExists(cache_dir))
        {
            icon_bin = GDKnyttAssetManager.loadFile(icon_cache_name);
            ini_bin = GDKnyttAssetManager.loadFile(ini_cache_name);
        }
        else
        {
            KnyttBinWorldLoader binloader;
            lock(file_lock) { binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world_dir)); }
            lock(file_lock) { icon_bin = binloader.GetFile("Icon.png"); }
            lock(file_lock) { ini_bin = binloader.GetFile("World.ini"); }

            GDKnyttAssetManager.ensureDirExists("user://Cache");
            new Directory().MakeDir(cache_dir);
            var f = new File();
            f.Open(icon_cache_name, File.ModeFlags.Write);
            f.StoreBuffer(icon_bin);
            f.Close();
            f.Open(ini_cache_name, File.ModeFlags.Write);
            f.StoreBuffer(ini_bin);
            f.Close();
        }

        Texture icon = GDKnyttAssetManager.loadTexture(icon_bin);
        string txt = GDKnyttAssetManager.loadTextFile(ini_bin);
        GDKnyttWorldImpl world = new GDKnyttWorldImpl();
        world.setDirectory(world_dir, null); // TODO: replace setDirectory with something else
        world.loadWorldConfig(txt);
        world.setBinMode(null);
        return new KnyttWorldManager<Texture>.WorldEntry(world, icon);
    }

    private GameButtonInfo generateRemoteWorld(Dictionary json_item)
    {
        GameButtonInfo world_info = new GameButtonInfo();
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

        // TODO: Check for file signatures
        return true;
    }

    private void listWorlds()
    {
        // Should take filter settings into account
        Manager.setFilter(filter_category, filter_difficulty, filter_size);
        var worlds = Manager.Filtered;
        game_container.clearWorlds();

        foreach(var world_entry in worlds)
        {
            game_container.addWorld((GDKnyttWorldImpl)world_entry.world, world_entry.extra_data, false);
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
        if (button.KWorld == null)
        {
            var timer = GetNode<Timer>("DownloadMonitor");
            if (!timer.IsStopped()) { return; }

            GDKnyttAssetManager.ensureDirExists("user://Worlds");
            
            string filename = button.buttonInfo.Link;
            filename = filename.Substring(filename.LastIndexOf('/') + 1);
            if (filename.IndexOf('?') != -1) { filename = filename.Substring(0, filename.IndexOf('?')); }
            if (!filename.EndsWith(".knytt.bin")) { filename += ".knytt.bin"; }
            filename = Uri.UnescapeDataString(filename);
            http_node.DownloadFile = $"user://Worlds/{filename}.part";
            
            var error = http_node.Request(button.buttonInfo.Link);
            if (error != Error.Ok) { download_button.markFailed(); return; }

            timer.Start();

            download_button = button;
            button.setDownloaded(0);
        }
        else
        {
            ClickPlayer.Play();
            var info_screen = info_scene.Instance() as InfoScreen;
            info_screen.initialize(button.KWorld);
            info_screen.ButtonInfo = button.buttonInfo;
            this.GetParent().AddChild(info_screen);
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
            download_button.markCompleted();
            download_button.incDownloads();
            download_button.KWorld = (GDKnyttWorldImpl)generateBinWorld(final_filename).world;
            sendDownload();
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
            download_button.buttonInfo.Name, download_button.buttonInfo.Author, (int)RateHTTPRequest.Action.Download);
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
}
