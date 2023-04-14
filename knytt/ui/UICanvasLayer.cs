using Godot;
using static YKnyttLib.JuniValues;

public partial class UICanvasLayer : CanvasLayer
{
    public GDKnyttGame Game { get; private set; }
    bool showing = false;
    bool sliding_out = false;

    public WSOD WSOD { get; private set; }
    public LocationLabel Location { get; private set; }
    private ArtifactsPanel artifactsPanel;

    public void initialize(GDKnyttGame game)
    {
        Game = game;

        if (game.hasMap()) { GetNode<InfoPanel>("InfoPanel").addItem("ItemInfo", (int)PowerNames.Map); }
    }

    public override void _Ready()
    {
        WSOD = GetNode<WSOD>("WSOD");
        Location = GetNode<LocationLabel>("LocationLabel");
    }

    public override void _PhysicsProcess(double delta)
    {
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
            anim.SpeedScale = Mathf.Abs(anim.SpeedScale);
            if (anim2 != null) { anim2.SpeedScale = Mathf.Abs(anim2.SpeedScale); }
            if (showing) { anim.PlayBackwards("SlideOut"); anim2?.PlayBackwards("SlideOut"); sliding_out = false; }
            else { anim.Play("SlideOut"); anim2?.Play("SlideOut"); sliding_out = true; }
        }
        else
        {
            anim.SpeedScale *= -1f;
            if (anim2 != null) { anim2.SpeedScale *= -1f; }
            sliding_out = !sliding_out;
        }
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        showing = sliding_out;
    }

    public void powerUpdate(int name, bool value)
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
            artifactsPanel = scene.Instantiate<ArtifactsPanel>();
            artifactsPanel.Modulate = new Color(1, 1, 1, 0);
            AddChild(artifactsPanel);
        }

        artifactsPanel?.updateItems(Game.Juni);
    }
}
