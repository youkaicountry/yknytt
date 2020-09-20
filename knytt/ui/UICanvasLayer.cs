using Godot;
using static YKnyttLib.JuniPowers;

public class UICanvasLayer : CanvasLayer
{
    public GDKnyttGame Game { get; private set; }
    bool showing = false;
    bool sliding_out = false;

    public void initialize(GDKnyttGame game)
    {
        Game = game;
    }

    public override void _PhysicsProcess(float delta)
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
        if (!anim.IsPlaying())
        {
            anim.PlaybackSpeed = 6f;
            if (showing) { anim.PlayBackwards("SlideOut"); sliding_out = false; }
            else { anim.Play("SlideOut"); sliding_out = true; }
        }
        else 
        {
            anim.PlaybackSpeed *= -1f;
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
	}
}
