using Godot;
using YKnyttLib;

public class GKnyttGame : Node2D
{
	// TODO: This is per-player stuff, and should eventually be abstracted
	public GKnyttArea CurrentArea { get; private set; }
	public GKnyttWorld World { get; private set; }
	public GKnyttCamera Camera { get; private set; }

	// Audio channels
	public KnyttAudioChannel MusicChannel { get; private set; }

	[Export]
	public TransitionType Transition { get; private set; }

	public enum TransitionType
	{
		JUMP,
		EDGE_SCROLL
	}

	[Export]
	public float edgeScrollSpeed = 1500f;

	[Export]
    public bool viewMode = true;

	public override void _Ready()
	{
		this.MusicChannel = GetNode("MusicChannel") as KnyttAudioChannel;
		this.MusicChannel.StreamFetcher = (int num) => World.getSong(num);

		this.Camera = GetNode("GKnyttCamera") as GKnyttCamera;
		this.Camera.initialize(this);

		this.World = GetNode("GKnyttWorld") as GKnyttWorld;

		this.loadDemo();
	}

	private void loadDemo()
	{
		var wd = new Directory();
		var e = wd.Open("./worlds/Nifflas - The Machine");
		this.World.loadWorld(wd);

		this.changeArea(new KnyttPoint(1001, 1000), true);
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
		// This should inform the world that it needs an area loaded
		var area = this.World.instantiateArea(new_area);
		if (area == null) { return; }
		this.CurrentArea = area;
		this.beginTransitionEffects(force_jump);
	}

	// Handles transition effects
	private void beginTransitionEffects(bool force_jump = false)
	{
		// Audio
		this.MusicChannel.setTrack(this.CurrentArea.Area.Song);

		// UI
		if (this.viewMode) { ((LocationLabel)GetNode("UICanvasLayer").GetNode("LocationLabel")).updateLocation(this.CurrentArea.Area.Position); }

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
