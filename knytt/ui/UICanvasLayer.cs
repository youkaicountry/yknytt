using Godot;
using static YKnyttLib.JuniValues;

public class UICanvasLayer : CanvasLayer
{
    public GDKnyttGame Game { get; private set; }
    bool showing = false;
    bool sliding_out = false;

    public WSOD WSOD { get; private set; }
    public LocationLabel Location { get; private set; }
    private ArtifactsPanel artifactsPanel;
    private MapPanel mapPanel;

    private bool _force_map;
    public bool ForceMap
    {
        get { return _force_map; } 
        set
        {
            _force_map = value;
            if (value)
            {
                mapPanel.init(Game.GDWorld.KWorld, Game.Juni);
                Game.Juni.Powers.setVisited(Game.CurrentArea.Area);
            }
            else
            {
                mapPanel.init(null, null);
            }
        }
    }

    public void initialize(GDKnyttGame game)
    {
        Game = game;

        mapPanel = GetNode<MapPanel>("MapBackgroundPanel/MapPanel");
        if (game.hasMap())
        {
            mapPanel.init(game.GDWorld.KWorld, game.Juni);
            GetNode<TouchPanel>("TouchPanel").InstallMap();
            GetNode<InfoPanel>("InfoPanel").addItem("ItemInfo", (int)PowerNames.Map);
        }
        else
        {
            mapPanel.init(null, null);
        }
    }

    public override void _Ready()
    {
        WSOD = GetNode<WSOD>("WSOD");
        Location = GetNode<LocationLabel>("LocationLabel");
    }

    public override void _Process(float delta)
    {
        if (GetNode<Console>("/root/Console").IsOpen) { return; }

        if (Input.IsActionJustPressed("map") && Game.hasMap()) { mapPanel.ShowMap(true); }

        if (Input.IsActionJustPressed("show_info"))
        {
            togglePanel();
            GetNode<Timer>("StayTimer").Start();
        }
        else if (Input.IsActionJustReleased("show_info"))
        {
            if (!showing || GetNode<Timer>("StayTimer").TimeLeft > 0f) { return; }
            togglePanel();
        }
    }

    private void togglePanel()
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        var anim2 = artifactsPanel?.GetNode<AnimationPlayer>("AnimationPlayer");

        if (!anim.IsPlaying())
        {
            anim.PlaybackSpeed = Mathf.Abs(anim.PlaybackSpeed);
            if (anim2 != null) { anim2.PlaybackSpeed = Mathf.Abs(anim.PlaybackSpeed); }
            if (showing) { anim.PlayBackwards("SlideOut"); anim2?.PlayBackwards("SlideOut"); sliding_out = false; }
            else { anim.Play("SlideOut"); anim2?.Play("SlideOut"); sliding_out = true; }
        }
        else
        {
            anim.PlaybackSpeed *= -1f;
            if (anim2 != null) { anim2.PlaybackSpeed *= -1f; }
            sliding_out = !sliding_out;
        }
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        showing = sliding_out;
    }

    public void powerUpdate(PowerNames names, bool value)
    {
        updatePowers();
    }

    public void updatePowers()
    {
        GetNode<InfoPanel>("InfoPanel").updateItems(Game.Juni);

        if (artifactsPanel == null && (
                Game.Juni.Powers.getCreaturesCount() > 0 || 
                Game.Juni.Powers.getCoinCount() > 0 || 
                Game.Juni.Powers.getArtifactsCount() > 0))
        {
            var scene = ResourceLoader.Load("res://knytt/ui/info_panel/ArtifactsPanel.tscn") as PackedScene;
            artifactsPanel = scene.Instance() as ArtifactsPanel;
            artifactsPanel.Modulate = new Color(1, 1, 1, 0);
            AddChild(artifactsPanel);
        }

        artifactsPanel?.updateItems(Game.Juni);
    }
}
