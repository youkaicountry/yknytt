using Godot;

public class CreditsScreen : BasicScreen
{
    private bool testers_open = true;
    private bool tilesets_open;
    private bool music_open;

    public override void _Ready()
    {
        base._Ready();
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TestersPanel/AnimationPlayer").Play("RESET");
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TilesetsPanel/AnimationPlayer").Play("RESET");
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/MusicPanel/AnimationPlayer").Play("RESET");
        initFocus();
    }
    
    public override void initFocus()
    {
        GetNode<Button>("BackButton").GrabFocus();
    }

    private void _on_Testers_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TestersPanel/AnimationPlayer").Play(
            testers_open ? "collapse" : "expand");
        testers_open = !testers_open;
    }

    private void _on_Tilesets_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/TilesetsPanel/AnimationPlayer").Play(
            tilesets_open ? "collapse" : "expand");
        tilesets_open = !tilesets_open;
    }

    private void _on_Music_pressed()
    {
        GetNode<AnimationPlayer>("ScrollContainer/VBoxContainer/MusicPanel/AnimationPlayer").Play(
            music_open ? "collapse" : "expand");
        music_open = !music_open;
    }

    private void _on_LinkButton_pressed(string url)
    {
        OS.ShellOpen(url);
    }
}
