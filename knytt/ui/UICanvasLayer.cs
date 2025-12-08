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
    private InfoPanel infoPanel;

    private bool _force_map;
    public bool ForceMap
    {
        get { return _force_map; } 
        set { _force_map = value; forceMap(value, add_icon: true); }
    }

    public void forceMap(bool force, bool add_icon = false)
    {
        if (force)
        {
            mapPanel.init(Game.GDWorld.KWorld, Game.Juni);
            if (Game.hasMap()) { GetNode<MapViewports>("%MapViewports").addArea(Game.CurrentArea); }
            if (add_icon) { infoPanel.addItem("ItemInfo", (int)PowerNames.Map); }
        }
        else
        {
            mapPanel.init(null, null);
        }
        Game.UI.GetNode<TouchPanel>("TouchPanel").InstallMap(force);
    }

    public void initialize(GDKnyttGame game)
    {
        Game = game;

        infoPanel = Game.GetNode<InfoPanel>("%InfoPanel");
        infoPanel.checkCustomPowers();

        mapPanel = GetNode<MapPanel>("MapBackgroundPanel/MapPanel");
        if (game.hasMap())
        {
            mapPanel.init(game.GDWorld.KWorld, game.Juni);
            GetNode<TouchPanel>("TouchPanel").InstallMap();
            if (Game.GDWorld.KWorld.INIData["World"]["Format"] == "4" || Game.Juni.Powers.getPower(PowerNames.Map))
            {
                infoPanel.addItem("ItemInfo", (int)PowerNames.Map);
            }
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
            if (!sliding_out || GetNode<Timer>("StayTimer").TimeLeft > 0f) { return; }
            togglePanel();
        }
    }

    private void togglePanel()
    {
        var anim = infoPanel.GetNode<AnimationPlayer>("AnimationPlayer");
        var anim2 = artifactsPanel?.GetNode<AnimationPlayer>("AnimationPlayer");

        if (!anim.IsPlaying())
        {
            anim.PlaybackSpeed = Mathf.Abs(anim.PlaybackSpeed);
            if (anim2 != null) { anim2.PlaybackSpeed = Mathf.Abs(anim.PlaybackSpeed); }
            if (showing)
            {
                anim.PlayBackwards("SlideOut");
                anim2?.PlayBackwards("SlideOut");
                sliding_out = false;
            }
            else
            {
                float xscreen = GetViewport().GetVisibleRect().Size.x;
                if (xscreen > 600 && !GDKnyttSettings.SeamlessScroll) { xscreen = 600; }
                xscreen *= TouchSettings.ViewportNow;
                float xinfo = infoPanel.RectSize.x;
                infoPanel.MarginLeft = -xscreen / 2 - 3;
                infoPanel.MarginRight = infoPanel.MarginLeft + xinfo;
                if (artifactsPanel != null) { artifactsPanel.RectScale = Vector2.One * Mathf.Min(xscreen / 600, 1); }

                anim.Play("SlideOut");
                anim2?.Play("SlideOut");
                sliding_out = true;
            }
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
        if (Game.hasMap()) { GetNode<TouchPanel>("TouchPanel").InstallMap(!showing); }
    }

    public void closePanel()
    {
        if (sliding_out) { togglePanel(); }
    }

    public void powerUpdate(PowerNames names, bool value)
    {
        updatePowers();
    }

    public void updatePowers()
    {
        infoPanel.updateItems(Game.Juni);

        if (artifactsPanel == null && (
                Game.Juni.Powers.getCreaturesCount() > 0 || 
                Game.Juni.Powers.getCoinCount() > 0 || 
                Game.Juni.Powers.getArtifactsCount() > 0))
        {
            artifactsPanel = ResourceLoader.Load<PackedScene>("res://knytt/ui/info_panel/ArtifactsPanel.tscn").Instance<ArtifactsPanel>();
            artifactsPanel.Modulate = new Color(1, 1, 1, 0);
            Game.GetNode<Node2D>("%TintNode").AddChild(artifactsPanel);
        }

        artifactsPanel?.updateItems(Game.Juni);

        if (Game.Juni.Powers.getPower(PowerNames.Map) && !Game.hasMap()) { ForceMap = true; } // if Map=false but map power was given
    }
}
