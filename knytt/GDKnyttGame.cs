using Godot;
using YKnyttLib;

public class GDKnyttGame : Node2D
{
	PackedScene juni_scene;
	PackedScene pause_scene;
	public Juni Juni { get; private set; }

	public UICanvasLayer UI { get; private set; }
	private MapPanel mapPanel;

	// TODO: This is per-player stuff, and should eventually be abstracted
	public GDKnyttArea CurrentArea { get; private set; }
	public GDKnyttWorld GDWorld { get; private set; }
	public GDKnyttCamera Camera { get; private set; }

	// Audio channels
	public GDKnyttAudioChannel MusicChannel { get; private set; }
	public GDKnyttAudioChannel AmbianceChannel1 { get; private set; }
	public GDKnyttAudioChannel AmbianceChannel2 { get; private set; }

	[Export]
	public string demoWorld = "./worlds/Nifflas - The Machine";

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
		this.MusicChannel = GetNode<GDKnyttAudioChannel>("MusicChannel");
		this.MusicChannel.OnFetch = (int num) => GDWorld.AssetManager.getSong(num);
		this.MusicChannel.OnClose = (int num) => GDWorld.AssetManager.returnSong(num);

		this.AmbianceChannel1 = GetNode<GDKnyttAudioChannel>("Ambi1Channel");
		this.AmbianceChannel1.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
		this.AmbianceChannel1.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

		this.AmbianceChannel2 = GetNode<GDKnyttAudioChannel>("Ambi2Channel");
		this.AmbianceChannel2.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
		this.AmbianceChannel2.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

		this.Camera = GetNode<GDKnyttCamera>("GKnyttCamera");
		this.Camera.initialize(this);

		UI = GetNode<UICanvasLayer>("UICanvasLayer");
		this.GDWorld = GetNode<GDKnyttWorld>("GKnyttWorld");

		if (!this.viewMode) { GetNode<LocationLabel>("UICanvasLayer/LocationLabel").Visible = false; }

		this.setupCamera();
		this.setupWorld();
	}

	private void setupWorld()
	{
		GDKnyttWorldImpl world;
		if (GDKnyttDataStore.KWorld != null) { world = GDKnyttDataStore.KWorld; }
		else
		{
			world = new GDKnyttWorldImpl();
			world.setDirectory(this.demoWorld, "");
			var save_data = GDKnyttAssetManager.loadTextFile(this.demoWorld + "/DefaultSavegame.ini");
			world.CurrentSave = new KnyttSave(world, save_data, 1);
		}

		GDWorld.setWorld(this, world);
		GDWorld.loadWorld();
		createJuni();

		this.changeArea(GDWorld.KWorld.CurrentSave.getArea(), true);
		Juni.moveToPosition(CurrentArea, GDWorld.KWorld.CurrentSave.getAreaPosition());

		UI.initialize(this);
		UI.updatePowers();

		mapPanel = GetNode<MapPanel>("UICanvasLayer/MapBackgroundPanel/MapPanel");
		if (hasMap()) { mapPanel.init(GDWorld.KWorld, Juni); } else { mapPanel.init(null, null); }
	}

	// On load a save file
	public void createJuni()
	{
		Juni = juni_scene.Instance() as Juni;
		Juni.initialize(this);
		this.AddChild(Juni);
		Juni.Connect("PowerChanged", UI, "powerUpdate");
	}

	public async void respawnJuni()
	{
		UI.WSOD.startWSOD();
        await ToSignal(UI.WSOD, "WSODFinished");

		var save = GDWorld.KWorld.CurrentSave;
		Juni.Powers.readFromSave(save);
		this.changeArea(save.getArea(), force_jump: true, regenerate_same: true);
		Juni.moveToPosition(CurrentArea, save.getAreaPosition());
		Juni.reset();
		UI.updatePowers();
		
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
		var f = new File();
		f.Open($"user://Saves/{save.SaveFileName}", File.ModeFlags.Write);
		f.StoreString(save.ToString());
		f.Close();
	}

    public void warpJuni(Juni juni)
	{
		CallDeferred("_warp_juni", juni);
	}

    private void _warp_juni(Juni juni)
    {
		// Calculate the warp
		var area = CurrentArea.Area;
		var jgp = juni.GlobalPosition;
		var new_coords = GDKnyttWorld.getAreaCoords(jgp);
		var wc = area.Warp.getWarpCoords(new_coords, area.Position);
		
		// Apply the warp
		jgp += new Vector2(GDKnyttArea.Width*wc.x, GDKnyttArea.Height*wc.y);
		var after_warp_coords = GDKnyttWorld.getAreaCoords(jgp);

		// Find flag warps
		var flag_warps = new KnyttArea(after_warp_coords, GDWorld.KWorld).FlagWarps;
		var all_flag_warp = flag_warps[(int)KnyttArea.FlagWarpID.All];
		bool some_check_failed = false;
		Vector2? found_warp = null;
		foreach (var flag_warp in flag_warps)
		{
			if (flag_warp != null)
			{
				if (juni.Powers.check(flag_warp.flag))
				{
					if (flag_warp == all_flag_warp && flag_warp.flag.true_flag) // Special case
					{
						if (some_check_failed) { continue; } else { found_warp = null; } // Use previous found warp; else override
					}
					found_warp = found_warp ?? new Vector2(GDKnyttArea.Width * flag_warp.x, GDKnyttArea.Height * flag_warp.y);
				}
				else
				{
					some_check_failed = true;
				}
			}
		}

		// Apply flag warps
		if (found_warp != null) { jgp += found_warp.Value; }
		var after_flag_warp_coords = GDKnyttWorld.getAreaCoords(jgp);
		
		juni.GlobalPosition = jgp;
		changeArea(after_flag_warp_coords, regenerate_same:false);
    }

	public bool hasMap()
	{
		var world_section = GDWorld.KWorld.INIData["World"];
		return world_section["Format"] == "4" && world_section["Map"] != "False";
	}

	public override void _Process(float delta)
	{
		if (this.viewMode) { this.editorControls(); }

		if (Input.IsActionJustPressed("pause")) { pause(); }

		if (Input.IsActionJustPressed("map") && hasMap()) {	mapPanel.ShowMap(true); }
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
		if (what == MainLoop.NotificationWmGoBackRequest) { pause(); }
    }

	private void pause()
	{
		if (GetTree().Paused) { return; }
		var node = pause_scene.Instance();
		GetNode<Node>("UICanvasLayer").CallDeferred("add_child", node);
	}

	private void editorControls()
	{
		if (Input.IsActionJustPressed("show_info")) { ((LocationLabel)GetNode("UICanvasLayer").GetNode("LocationLabel")).showLocation(); }

		if (!this.Camera.Scrolling)
		{
			if (Input.IsActionPressed("up"))    { this.changeAreaDelta(new KnyttPoint( 0, -1)); }
			if (Input.IsActionPressed("down"))  { this.changeAreaDelta(new KnyttPoint( 0,  1)); }
			if (Input.IsActionPressed("left"))  { this.changeAreaDelta(new KnyttPoint(-1,  0)); }
			if (Input.IsActionPressed("right")) { this.changeAreaDelta(new KnyttPoint( 1,  0)); }
		}
	}

    public void changeAreaDelta(KnyttPoint delta, bool force_jump = false, bool regenerate_same = true)
    {
        this.changeArea(this.CurrentArea.Area.Position + delta, force_jump);
    }

	// Changes the current area
	public void changeArea(KnyttPoint new_area, bool force_jump = false, bool regenerate_same = true)
	{
		// Regenerate current area if no change, else deactivate old area
		if (this.CurrentArea != null)
		{ 
			if (CurrentArea.Area.Position.Equals(new_area))
			{
				if (regenerate_same) { CurrentArea.regenerateArea(regenerate_same: regenerate_same); }
				return;
			}

			CurrentArea.scheduleDeactivation();
		}
		
		// Update the paging
		GDWorld.Areas.setLocation(new_area);
		var area = GDWorld.getArea(new_area);

		if (area == null) { return; }

		this.CurrentArea = area;
		this.CurrentArea.activateArea();
		this.beginTransitionEffects(force_jump);

		Juni.stopHologram(cleanup:true);
		if (hasMap()) { Juni.Powers.VisitedAreas.Add(CurrentArea.Area.Position); }
	}

	public async void win(string ending)
	{
		var fade = GetNode<FadeLayer>("FadeCanvasLayer/Fade");
		fade.startFade();
		await ToSignal(fade, "FadeDone");
		GDKnyttDataStore.winGame(ending);
	}

	// Handles transition effects
	private void beginTransitionEffects(bool force_jump = false)
	{
		// Audio
		this.MusicChannel.setTrack(this.CurrentArea.Area.Song);
		this.AmbianceChannel1.setTrack(this.CurrentArea.Area.AtmosphereA);
		this.AmbianceChannel2.setTrack(this.CurrentArea.Area.AtmosphereB);

		// UI
		if (this.viewMode) { GetNode<LocationLabel>("UICanvasLayer/LocationLabel").updateLocation(this.CurrentArea.Area.Position); }

		// Camera
		var scroll = GDKnyttSettings.ScrollType;
		if (force_jump || scroll == GDKnyttSettings.ScrollTypes.Original)
		{
			this.Camera.jumpTo(this.CurrentArea.GlobalCenter);
			return;
		}

		switch(scroll)
		{
			case GDKnyttSettings.ScrollTypes.Smooth:
				this.Camera.scrollTo(this.CurrentArea.GlobalCenter, edgeScrollSpeed);
				break;
		}
	}

	public void setupCamera()
	{
		if (!TouchSettings.EnablePanel)
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
}
