using System;
using System.Linq;
using Godot;
using YKnyttLib;
using YKnyttLib.Logging;

public class GDKnyttGame : Node2D
{
    PackedScene juni_scene;
    PackedScene pause_scene;
    public Juni Juni { get; private set; }

    public UICanvasLayer UI { get; private set; }

    // TODO: This is per-player stuff, and should eventually be abstracted
    public GDKnyttArea CurrentArea { get; private set; }
    public GDKnyttWorld GDWorld { get; private set; }
    public GDKnyttCamera Camera { get; private set; }
    public BitmapFont SignFont { get; private set; }
    public TrailContainer Trails { get; private set; }
    public DeathContainer Deaths { get; private set; }

    // Audio channels
    public GDKnyttMusicChannel MusicChannel { get; private set; }
    public GDKnyttAmbiChannel AmbianceChannel1 { get; private set; }
    public GDKnyttAmbiChannel AmbianceChannel2 { get; private set; }

    private ShaderMaterial tint;
    private MapViewports viewports;

    [Export]
    public float edgeScrollSpeed = 1500f;

    [Export]
    public bool viewMode = true;

    public GDKnyttGame()
    {
        juni_scene = ResourceLoader.Load("res://knytt/juni/Juni.tscn") as PackedScene;
        pause_scene = ResourceLoader.Load("res://knytt/ui/PauseLayer.tscn") as PackedScene;
    }

    public override void _Ready()
    {
        this.MusicChannel = GetNode<GDKnyttMusicChannel>("%MusicChannel");
        this.MusicChannel.OnFetch = (int num) => GDWorld.AssetManager.getSong(num);
        this.MusicChannel.OnClose = (int num) => GDWorld.AssetManager.returnSong(num);

        this.AmbianceChannel1 = GetNode<GDKnyttAmbiChannel>("%Ambi1Channel");
        this.AmbianceChannel1.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
        this.AmbianceChannel1.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

        this.AmbianceChannel2 = GetNode<GDKnyttAmbiChannel>("%Ambi2Channel");
        this.AmbianceChannel2.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
        this.AmbianceChannel2.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

        this.Camera = GetNode<GDKnyttCamera>("%GKnyttCamera");
        this.Camera.initialize(this);

        this.Trails = GetNode<TrailContainer>("%TrailContainer");
        this.Trails.initialize(this);

        this.Deaths = GetNode<DeathContainer>("%DeathContainer");

        this.viewports = GetNode<MapViewports>("%MapViewports");

        UI = GetNode<UICanvasLayer>("%UICanvasLayer");
        this.GDWorld = GetNode<GDKnyttWorld>("GKnyttWorld");

        tint = ResourceLoader.Load<ShaderMaterial>("res://knytt/AreaTint.tres");

        GDKnyttSettings.setupViewport(for_ui: false);
        this.setupCamera();
        this.setupWorld();
        this.setupShader();
    }

    private void setupWorld()
    {
        GDKnyttWorldImpl world = GDKnyttDataStore.KWorld;
        GDWorld.setWorld(this, world);
        GDWorld.loadWorld();
        createJuni();

        if (GDKnyttSettings.DetailedMap) { viewports.init(world); }
        loadFonts();
        UI.initialize(this);
        UI.updatePowers();

        this.changeArea(GDWorld.KWorld.CurrentSave.getArea(), true);
        Juni.moveToPosition(CurrentArea, GDWorld.KWorld.CurrentSave.getAreaPosition());
        saveGame(Juni, false);
    }

    // On load a save file
    public void createJuni()
    {
        Juni = juni_scene.Instance() as Juni;
        this.AddChild(Juni);
        Juni.initialize(this);
        Juni.Connect(nameof(Juni.PowerChanged), UI, nameof(UI.powerUpdate));
        Juni.Connect(nameof(Juni.PowerChanged), this, nameof(sendPowerUpdate));
        Juni.Connect(nameof(Juni.Died), Deaths, nameof(Deaths.onDied));
        if (Juni.Powers.KSSave) { sendCheat(); }
    }

    public async void respawnJuniWithWSOD()
    {
        // TODO: if respawn executes during save, Juni may save position, but not area coordinates (check again, hard to reproduce)
        if (GDKnyttSettings.WSOD)
        {
            UI.WSOD.startWSOD();
            await ToSignal(UI.WSOD, "WSODFinished");
        }
        respawnJuni();
    }

    public void respawnJuni()
    {
        var save = GDWorld.KWorld.CurrentSave;
        Juni.Powers = new JuniValues(save.SourcePowers, Juni.Powers);
        Juni.Powers.respawn(save.getArea(), save.getAreaPosition());
        this.changeArea(save.getArea(), force_jump: true, regenerate_same: true, respawn: true);
        Juni.moveToPosition(CurrentArea, save.getAreaPosition());
        Juni.reset();
        if (GDKnyttSettings.SideScroll) { adjustCenteredScroll(initial: true); }
        UI.updatePowers();
        KnyttLogger.Debug("Juni has respawned");
    }

    public void saveGame(Juni juni, bool write)
    {
        saveGame(juni.GDArea.Area.Position, juni.AreaPosition, write);
    }

    public void saveGame(KnyttPoint area, KnyttPoint position, bool write)
    {
        var save = GDWorld.KWorld.CurrentSave;
        save.setArea(area);
        save.setAreaPosition(position);
        Juni.Powers.writeToSave(save);
        if (!write) { return; }
        saveGame(save);
    }

    public void saveGame(KnyttSave save, bool save_map = true)
    {
        GDKnyttAssetManager.ensureDirExists(GDKnyttSettings.Saves);
        var f = new File();
        var fname = GDKnyttSettings.Saves.PlusFile(save.SaveFileName);
        f.Open(fname, File.ModeFlags.Write);
        f.StoreString(save.ToString());
        f.Close();

        if (save_map) { viewports.saveAll(); }
        KnyttLogger.Debug($"Game saved to {fname}");
    }

    public void warpJuni(Juni juni)
    {
        // This function, Switch._execute and Juni._die can be unwrapped, but you'll get a lot of messages about physics
        // that should be deferred. I don't know if they are dangerous or not.
        CallDeferred("_warp_juni", juni);
    }

    private void _warp_juni(Juni juni)
    {
        if (juni.dead) { return; } // If juni is dead, ignore warps

        // Calculate the warp
        var area = CurrentArea.Area;
        var jgp = juni.GlobalPosition;
        var new_coords = GDKnyttWorld.getAreaCoords(jgp);
        if (new_coords.Equals(area.Position)) { return; } // Area may change if warp is deferred
        var wc = area.Warp.getWarpCoords(new_coords, area.Position);

        // Apply the warp
        jgp += new Vector2(GDKnyttArea.Width * wc.x, GDKnyttArea.Height * wc.y);
        var after_warp_coords = GDKnyttWorld.getAreaCoords(jgp);

        // Apply flag warps
        var found_warp = getFlagWarp(after_warp_coords, juni);
        if (found_warp != null) { jgp += new Vector2(GDKnyttArea.Width * found_warp.Value.x, GDKnyttArea.Height * found_warp.Value.y); }
        var after_flag_warp_coords = GDKnyttWorld.getAreaCoords(jgp);

        juni.GlobalPosition = jgp;
        changeArea(after_flag_warp_coords, regenerate_same: false);
    }

    public KnyttPoint? getFlagWarp(KnyttPoint area_coords, Juni juni)
    {
        var area = GDWorld.KWorld.getArea(area_coords);
        if (area == null) { return null; }
        var all_flag_warp = area.FlagWarps[(int)KnyttArea.FlagWarpID.All];
        bool some_check_failed = false;
        KnyttPoint? found_warp = null;
        foreach (var flag_warp in area.FlagWarps)
        {
            if (flag_warp != null)
            {
                if (juni.Powers.check(flag_warp.flag))
                {
                    if (flag_warp == all_flag_warp) // Special case
                    {
                        if (some_check_failed) { continue; } else { found_warp = null; } // Use previous found warp; else override
                    }
                    found_warp = found_warp ?? new KnyttPoint(
                        flag_warp.xArtifactMode ? juni.Powers.getArtifactsCount(flag_warp.x - 1) : flag_warp.x,
                        flag_warp.yArtifactMode ? juni.Powers.getArtifactsCount(flag_warp.y - 1) : flag_warp.y);
                }
                else
                {
                    some_check_failed = true;
                }
            }
        }
        return found_warp;
    }

    public bool hasMap()
    {
        var world_section = GDWorld.KWorld.INIData["World"];
        if (UI.ForceMap) { return true; } // map is shown anyway in case of console command or if a player finds map power
        if (world_section["Map"]?.ToLower() == "false") { return false; }
        if (GDKnyttSettings.ForcedMap) { return true; }
        return world_section["Format"] == "4";
    }

    public override void _Process(float delta)
    {
        if (this.viewMode) { this.editorControls(); }

        if (Input.IsActionJustPressed("pause") && !GetNode<Console>("/root/Console").IsOpen) { pause(); }

        if (Input.IsActionPressed("show_info") && Input.IsActionPressed("umbrella") && Input.IsActionJustPressed("down"))
        {
            saveGame(Juni, true);
            UI.closePanel();
            Juni.Umbrella.Deployed = false;
            Juni.playSound("save");
        }
    }

    // TODO: Difference between Paged areas, active areas, and current area.
    // Current area is per-Juni
    public override void _PhysicsProcess(float delta)
    {
        // TODO: Do this only if the local player
        if (!CurrentArea.isIn(Juni.GlobalPosition))
        {
            warpJuni(Juni);
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest) { pause(); }
        if (what == MainLoop.NotificationWmGoBackRequest)
        {
            if (GetNode<Console>("/root/Console").IsOpen) { GetNode<Console>("/root/Console").toggleConsole(); return; }
            pause();
        }
    }

    private void pause()
    {
        if (GetTree().Paused) { return; }
        var node = pause_scene.Instance();
        GetNode<Node>("UICanvasLayer").CallDeferred("add_child", node);
    }

    private void editorControls()
    {
        if (!this.Camera.Scrolling)
        {
            if (Input.IsActionPressed("up")) { this.changeAreaDelta(new KnyttPoint(0, -1)); }
            if (Input.IsActionPressed("down")) { this.changeAreaDelta(new KnyttPoint(0, 1)); }
            if (Input.IsActionPressed("left")) { this.changeAreaDelta(new KnyttPoint(-1, 0)); }
            if (Input.IsActionPressed("right")) { this.changeAreaDelta(new KnyttPoint(1, 0)); }
        }
    }

    public void changeAreaDelta(KnyttPoint delta, bool force_jump = false, bool regenerate_same = true)
    {
        this.changeArea(this.CurrentArea.Area.Position + delta, force_jump);
    }

    // Changes the current area
    public void changeArea(KnyttPoint new_area, bool force_jump = false, bool regenerate_same = true, bool respawn = false)
    {
        // Regenerate current area if no change, else deactivate old area
        if (this.CurrentArea != null)
        {
            if (CurrentArea.Area.Position.Equals(new_area))
            {
                if (regenerate_same) { CurrentArea.regenerateArea(regenerate_same: regenerate_same); }
                return;
            }

            if (!respawn) { CurrentArea?.Objects?.checkCollectables(GDWorld.Game.Juni.Powers); }
            CurrentArea.scheduleDeactivation();
            Juni.juniInput.altInput.ClearInput();
        }

        // Update the paging
        GDWorld.Areas.setLocation(new_area);
        var area = GDWorld.getArea(new_area);

        if (area == null) { return; }

        int change_distance = CurrentArea == null ? 0 : CurrentArea.Area.Position.manhattanDistance(new_area);
        bool old_area_swim = CurrentArea == null ? false : CurrentArea.Swim;

        this.CurrentArea = area;
        CurrentArea.activateArea();
        CurrentArea?.Objects?.checkCollectables(GDWorld.KWorld.CurrentSave.SourcePowers);
        beginTransitionEffects(force_jump || change_distance > 1); // never scroll if jump distance is over 1

        Juni.stopHologram(cleanup: true);
        if (old_area_swim && !CurrentArea.Swim) { Juni.Swim = true; Juni.Swim = false; } // boost when exit swim area
        if (area.Area.ExtraData?.ContainsKey("Attach") ?? false) { Juni.enableAttachment(area.Area.getExtraData("Attach")); }
        checkTint(area);
        CustomObject.cleanUnused();

        if (Juni.DebugFlyMode) { return; }
        Juni.Powers.setVisited(CurrentArea.Area);
        if (hasMap()) { viewports.addArea(CurrentArea); }
    }

    public async void win(string ending)
    {
        string statistics = $"{Juni.Powers.TotalTimeNow}s {Juni.Powers.TotalDeaths}d " +
                            $"{Juni.Powers.HardestPlace} {Juni.Powers.HardestPlaceDeaths}d";
        if (GDKnyttSettings.Connection == GDKnyttSettings.ConnectionType.Online)
        {
            GetNode<RateHTTPRequest>("RateHTTPRequest").send(GDWorld.KWorld.Info.Name, GDWorld.KWorld.Info.Author,
                (int)RateHTTPRequest.Action.WinExit, statistics);
        }
        await fade(fast: false, color: Cutscene.getCutsceneColor(ending));
        GDKnyttDataStore.winGame(ending, statistics);
    }

    public SignalAwaiter fade(bool fast, Color? color)
    {
        var fade = GetNode<FadeLayer>("FadeCanvasLayer/Fade");
        fade.startFade(fast: fast, color: color);
        return ToSignal(fade, "FadeDone");
    }

    public async void quit()
    {
        CustomObject.clean();
        GC.Collect(); // not neccessary, white screen exit bug is still here
        GDKnyttDataStore.CurrentSpeed = 1;
        await fade(fast: false, color: null);

        GetTree().Paused = false;
        GetTree().ChangeScene("res://knytt/ui/MainMenu.tscn");
    }

    // Handles transition effects
    private void beginTransitionEffects(bool force_jump = false)
    {
        // Audio
        this.MusicChannel.setTrack(CurrentArea.PlayNoMusic ? 0 : CurrentArea.Area.Song);
        this.AmbianceChannel1.setTrack(CurrentArea.Area.AtmosphereA, CurrentArea.Ambiance1CustomVolume);
        this.AmbianceChannel2.setTrack(CurrentArea.Area.AtmosphereB, CurrentArea.Ambiance2CustomVolume);

        // UI
        UI.Location.updateLocation(this.CurrentArea.Area.Position);

        // Camera
        if (GDKnyttSettings.SideScroll)
        {
            adjustCenteredScroll(initial: true, force_jump: force_jump);
            GDWorld.createFakeObjects();
        }
        else
        {
            this.Camera.jumpTo(this.CurrentArea.GlobalCenter);
        }
    }

    private bool left_area_restricted, right_area_restricted;

    public void adjustCenteredScroll(bool initial = false, bool force_jump = false)
    {
        if (GetViewport() == null) { return; }

        float camera_global_position_x = Camera.GlobalPosition.x;
        
        if (initial)
        {
            GDWorld.Areas.Areas.TryGetValue(CurrentArea.Area.Position + new KnyttPoint(-1, 0), out var left_area);
            left_area_restricted = !GDKnyttSettings.SeamlessScroll || left_area == null || left_area.Area.Empty ||
                (CurrentArea.Area.Warp.LoadedWarp && !CurrentArea.Area.Warp.WarpLeft.isZero())/* ||
                left_area.Area.FlagWarps.Any(w => w != null)*/;

            GDWorld.Areas.Areas.TryGetValue(CurrentArea.Area.Position + new KnyttPoint(1, 0), out var right_area);
            right_area_restricted = !GDKnyttSettings.SeamlessScroll || right_area == null || right_area.Area.Empty ||
                (CurrentArea.Area.Warp.LoadedWarp && !CurrentArea.Area.Warp.WarpRight.isZero())/* ||
                right_area.Area.FlagWarps.Any(w => w != null)*/;

            if (force_jump) { camera_global_position_x = CurrentArea.GlobalPosition.x + camera_global_position_x % 600; }
            else if (!GDKnyttSettings.SeamlessScroll) { camera_global_position_x = CurrentArea.GlobalCenter.x; } // fix flickering
        }

        float juni_in_area = Juni.GlobalPosition.x - CurrentArea.GlobalCenter.x;
        float juni_on_camera = Juni.GlobalPosition.x - camera_global_position_x;
        float camera_in_area = camera_global_position_x - CurrentArea.GlobalCenter.x;

        float x_viewport = GetViewport().GetVisibleRect().Size.x * TouchSettings.ViewportNow;
        float camera_restriction = (600 - x_viewport) / 2;
        float camera_threshold = x_viewport * 72f / 600f;

        if (Math.Abs(juni_on_camera) > camera_threshold)
        {
            camera_in_area = juni_in_area - Math.Sign(juni_on_camera) * camera_threshold;
        }

        if (right_area_restricted && camera_in_area > camera_restriction)
        {
            camera_in_area = camera_restriction;
        }
        if (left_area_restricted && camera_in_area < -camera_restriction)
        {
            camera_in_area = -camera_restriction;
        }
        if (left_area_restricted && right_area_restricted && camera_restriction < 0)
        {
            camera_in_area = 0;
        }

        Camera.GlobalPosition = new Vector2(CurrentArea.GlobalCenter.x + camera_in_area, CurrentArea.GlobalCenter.y);

        if (x_viewport <= 600)
        {
            foreach (var area in GDWorld.Areas.Areas.Values)
            {
                float juni_in_this_area = Juni.GlobalPosition.x - area.GlobalCenter.x;
                area.Background.Position = new Vector2((juni_in_this_area - juni_on_camera) * 0.5f, 0);
            }
        }
    }

    public void setupCamera(bool force_fullscreen = false)
    {
        if (!TouchSettings.EnablePanel || force_fullscreen)
        {
            Camera.AnchorMode = Camera2D.AnchorModeEnum.DragCenter;
            Camera.Offset = new Vector2(0, 0);
        }
        else
        {
            Camera.AnchorMode = Camera2D.AnchorModeEnum.FixedTopLeft;
            // Later panel will set camera offset to stick it to the top or to the bottom
        }
    }

    public void sendPowerUpdate(int power, bool value)
    {
        if (GDKnyttSettings.Connection != GDKnyttSettings.ConnectionType.Online) { return; }
        if (power < 0 || !value) { return; }
        if (GetNodeOrNull<Console>("/root/Console")?.IsOpen ?? false) { return; }
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(GDWorld.KWorld.Info.Name, GDWorld.KWorld.Info.Author, 100 + power);
    }

    public void sendCheat()
    {
        if (GDKnyttSettings.Connection != GDKnyttSettings.ConnectionType.Online) { return; }
        GetNode<RateHTTPRequest>("RateHTTPRequest").send(GDWorld.KWorld.Info.Name, GDWorld.KWorld.Info.Author, 
            (int)RateHTTPRequest.Action.Cheat);
    }

    enum TintInk { TRANS, ADD, SUB, AND, OR, XOR };

    private void checkTint(GDKnyttArea area)
    {
        if (!(area.Area.ExtraData?.ContainsKey("Tint") ?? false)) { return; }

        string color = area.Area.ExtraData["Tint"];
        string ink = area.Area.ExtraData.ContainsKey("TintInk") ? area.Area.ExtraData["TintInk"] : "Trans";
        string trans = area.Area.ExtraData.ContainsKey("TintTrans") ? area.Area.ExtraData["TintTrans"] : "46";

        Juni.Powers.Tint = (color, ink, trans);
        applyTint(color, ink, trans);
    }

    public void applyTint(string color_str, string ink_str, string trans_str)
    {
        var glass = GetNode<Control>("GKnyttCamera/TintNode/Tint");

        int bgr = KnyttUtil.parseBGRString(color_str, 0);
        TintInk mode = Enum.TryParse<TintInk>(ink_str?.ToUpper(), out var i) ? i : TintInk.TRANS;
        int trans = KnyttUtil.parseIniInt(trans_str) ?? 46;

        if (bgr == 0)
        {
            glass.Material = null;
            glass.Visible = false;
            Juni.GetNode<Sprite>("ShiftHintSprite").ZIndex = 0;
            return;
        }

        int r = 0xFF & bgr, g = (0xFF00 & bgr) >> 8, b = (0xFF0000 & bgr) >> 16;
        float a = 1.0f - trans / 127.0f;

        tint.SetShaderParam("mode", mode);
        tint.SetShaderParam("r", r);
        tint.SetShaderParam("g", g);
        tint.SetShaderParam("b", b);
        tint.SetShaderParam("a", a);
        glass.Material = tint;
        glass.Visible = true;
        Juni.GetNode<Sprite>("ShiftHintSprite").ZIndex = 17;
    }

    public void setupBorder()
    {
        foreach (var area in GDWorld.Areas.Areas.Values)
        {
            area.setBorderVisible(GDKnyttSettings.Border);
        }
    }

    public void setupShader()
    {
        GetNode<ColorRect>("GKnyttCamera/ShaderNode/Shader").Material = 
            GDKnyttSettings.Shader <= GDKnyttSettings.ShaderType.HQ4X ? null :
            ResourceLoader.Load<ShaderMaterial>($"res://knytt/ui/screen_shaders/{GDKnyttSettings.Shader}.tres");

        var mat = ResourceLoader.Load<ShaderMaterial>("res://knytt/ui/screen_shaders/TileShader.tres");
        mat.Shader = GDKnyttSettings.Shader == GDKnyttSettings.ShaderType.HQ4X ?
            ResourceLoader.Load<Shader>("res://knytt/ui/screen_shaders/HQ4X.gdshader") : null;
    }

    private BitmapFont loadFont(string key, int width, int height)
    {
        if (!GDWorld.KWorld.INIData["World"].ContainsKey(key)) { return null; }
        string font_path = "Custom Objects/" + GDWorld.KWorld.INIData["World"][key];
        var t = GDWorld.KWorld.getWorldTexture(font_path) as Texture;
        if (t == null) { return null; }
        var font = new BitmapFont();
        font.Height = height;
        font.AddTexture(t);
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                font.AddChar(32 + y * 32 + x, 0, new Rect2(x * width, y * height, width, height));
            }
        }
        font.AddChar(0x100, 0, new Rect2(0, 0, width, height)); // workaround for space char (godot just skips it)
        return font;
    }

    public void loadFonts()
    {
        SignFont = loadFont("font", 7, 13);
        var title_font = loadFont("title", 13, 24);
        if (title_font != null)
        {
            GetNode<Label>("%Title/TitleLabel").AddFontOverride("font", title_font);
            GetNode<Label>("%Title/TitleLabel").RemoveStyleboxOverride("normal");
        }
        var subtitle_font = loadFont("subtitle", 7, 13);
        if (subtitle_font != null)
        {
            GetNode<Label>("%Title/SubtitleLabel").AddFontOverride("font", subtitle_font);
            GetNode<Label>("%Title/SubtitleLabel").RemoveStyleboxOverride("normal");
        }
    }

    private const float ASPECT_STEP = 0.04f;

    public async void processAspectChange()
    {
        if (!(Input.IsActionPressed("show_info") && Input.IsActionPressed("umbrella") &&
             (Input.IsActionJustPressed("left") || Input.IsActionJustPressed("right")))) { return; }

        float aspect = GDKnyttSettings.Aspect;
        float window_x_fixed = OS.WindowSize.x * TouchSettings.ViewportNow;
        var aspects = 
            Enumerable.Range(1, 20) // should be the same if written/read from ini
                .Select(i => float.Parse((i * 240 / window_x_fixed).ToString())) // integer scales
                .Append(float.Parse((1 / OS.WindowSize.Aspect()).ToString()))    // height = 100%
                .Append(0.4f)                                                    // width = 100%
                .Append(aspect)
                .Distinct()
                .ToList();
        aspects.Sort();
        int cur_index = aspects.IndexOf(aspect);

        if (Input.IsActionJustPressed("left"))
        {
            if (cur_index + 1 < aspects.Count && aspects[cur_index + 1] - aspect < ASPECT_STEP)
            {
                aspect = aspects[cur_index + 1];
            }
            else { aspect += ASPECT_STEP; }
            Input.ActionRelease("left");
        }
        else
        {
            if (cur_index - 1 >= 0 && aspect - aspects[cur_index - 1] < ASPECT_STEP)
            {
                aspect = aspects[cur_index - 1];
            }
            else { aspect -= ASPECT_STEP; }
            Input.ActionRelease("right");
        }
        
        float min_value = Mathf.Floor(window_x_fixed / 600) * 240 / window_x_fixed;
        if (min_value == 0) { min_value = 0.4f; }
        float max_value = 1 / (OS.WindowSize.Aspect() * TouchSettings.ViewportNow);
        if (aspect < min_value) { aspect = min_value; }
        if (aspect > max_value) { aspect = max_value; }
        
        UI.closePanel();
        GDKnyttSettings.Aspect = aspect;
        GDKnyttSettings.saveSettings();
        GDKnyttSettings.setupViewport();
        GDKnyttSettings.setupCamera();
        UI.Location.updateResolution();
        UI.GetNodeOrNull<TouchPanel>("TouchPanel").CallDeferred("Configure");
        GetTree().SetInputAsHandled();
    }
}
