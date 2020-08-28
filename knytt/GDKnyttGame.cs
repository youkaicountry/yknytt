using Godot;
using YKnyttLib;

public class GDKnyttGame : Node2D
{
	PackedScene juni_scene;
	public Juni Juni { get; private set; }

	// TODO: This is per-player stuff, and should eventually be abstracted
	public GDKnyttArea CurrentArea { get; private set; }
	public GDKnyttWorld GDWorld { get; private set; }
	public GDKnyttCamera Camera { get; private set; }

	// Audio channels
	public GDKnyttAudioChannel MusicChannel { get; private set; }
	public GDKnyttAudioChannel AmbianceChannel1 { get; private set; }
	public GDKnyttAudioChannel AmbianceChannel2 { get; private set; }

	[Export]
	public TransitionType Transition { get; private set; }

	[Export]
	public string demoWorld = "./worlds/Nifflas - The Machine";

	public enum TransitionType
	{
		JUMP,
		EDGE_SCROLL
	}

	[Export]
	public float edgeScrollSpeed = 1500f;

	[Export]
    public bool viewMode = true;

	public GDKnyttGame()
	{
		juni_scene = ResourceLoader.Load("res://knytt/juni/Juni.tscn") as PackedScene;
	}

	public override void _Ready()
	{
		this.MusicChannel = GetNode("MusicChannel") as GDKnyttAudioChannel;
		this.MusicChannel.OnFetch = (int num) => GDWorld.AssetManager.getSong(num);
		this.MusicChannel.OnClose = (int num) => GDWorld.AssetManager.returnSong(num);

		this.AmbianceChannel1 = GetNode("Ambi1Channel") as GDKnyttAudioChannel;
		this.AmbianceChannel1.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
		this.AmbianceChannel1.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

		this.AmbianceChannel2 = GetNode("Ambi2Channel") as GDKnyttAudioChannel;
		this.AmbianceChannel2.OnFetch = (int num) => GDWorld.AssetManager.getAmbiance(num);
		this.AmbianceChannel2.OnClose = (int num) => GDWorld.AssetManager.returnAmbiance(num);

		this.Camera = GetNode("GKnyttCamera") as GDKnyttCamera;
		this.Camera.initialize(this);

		this.GDWorld = GetNode("GKnyttWorld") as GDKnyttWorld;

		if (!this.viewMode) { GetNode<LocationLabel>("UICanvasLayer/LocationLabel").Visible = false; }

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
		spawnJuni();
		GDWorld.loadWorld();

		this.changeArea(GDWorld.KWorld.CurrentSave.getArea(), true);
		Juni.GlobalPosition = CurrentArea.getTileLocation(GDWorld.KWorld.CurrentSave.getAreaPosition());
	}

	// On load a save file (or die)
	public void spawnJuni()
	{
		if (Juni == null)
		{
			Juni = juni_scene.Instance() as Juni;
			Juni.initialize(this);
			this.AddChild(Juni);
		}
	}

	public override void _Process(float delta)
	{
		if (this.viewMode) { this.editorControls(); }
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

    public void changeAreaDelta(KnyttPoint delta, bool force_jump = false)
    {
        this.changeArea(this.CurrentArea.Area.Position + delta, force_jump);
    }

	// Changes the current area
	public void changeArea(KnyttPoint new_area, bool force_jump = false)
	{
		// Deactivate old area
		if (this.CurrentArea != null) { CurrentArea.deactivateArea(); }
		
		// Update the paging
		GDWorld.Areas.setLocation(new_area);
		var area = GDWorld.getArea(new_area);

		if (area == null) { return; }

		this.CurrentArea = area;
		this.CurrentArea.activateArea();
		this.beginTransitionEffects(force_jump);
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
		if (force_jump || this.Transition == TransitionType.JUMP)
		{
			this.Camera.jumpTo(this.CurrentArea.GlobalCenter);
			return;
		}

		switch(this.Transition)
		{
			case TransitionType.EDGE_SCROLL:
				this.Camera.scrollTo(this.CurrentArea.GlobalCenter, edgeScrollSpeed);
				break;
		}
	}
}
