using Godot;

public class Cutscene : Control
{
    private int current_scene = 0;
    private bool has_next;
    private bool last_page;

    public override void _Ready()
    {
        changeScene(1);
        loadMusic();
    }

    private void loadMusic()
    {
        var data = GDKnyttDataStore.KWorld.INIData;
        if (!data.Sections.ContainsSection("Cutscene Music")) { return; }
        if (!data["Cutscene Music"].ContainsKey(GDKnyttDataStore.CutsceneName)) { return; }
        string song = data["Cutscene Music"][GDKnyttDataStore.CutsceneName];
        var stream = GDKnyttDataStore.KWorld.getWorldSound(string.Format("Music/Song{0}.ogg", song)) as AudioStream;
        var player = GetNode<AudioStreamPlayer>("MusicPlayer");
        player.Stream = stream;
        player.Play();
    }

    private void changeScene(int delta)
    {
        current_scene += delta;
        Texture t = GDKnyttDataStore.KWorld.getWorldTexture(makeScenePath(current_scene)) as Texture;
        GetNode<TextureRect>("Image").Texture = t;
        setupBackButton();
        setupNextButton();
    }

    private void setupBackButton()
    {
        var back_button = GetNode<Button>("BackButton");
        var darrow = GetNode<TextureRect>("BackButton/DisabledArrow");

        bool disabled = current_scene == 1;
        darrow.Visible = disabled;
        back_button.Disabled = disabled;
    }

    private void setupNextButton()
    {
        var next_button = GetNode<Button>("NextButton");
        var arrow = GetNode<TextureRect>("NextButton/Arrow");

        last_page = !GDKnyttDataStore.KWorld.worldFileExists(makeScenePath(current_scene+1));
        next_button.Text = last_page ? "OK" : "";
        arrow.Visible = !last_page;
    }

    public static string makeScenePath(int scene)
    {
        return string.Format("{0}/Scene{1}.png", GDKnyttDataStore.CutsceneName, scene);
    }

    public void _on_PreviousButton_pressed()
    {
        GetNode<AudioStreamPlayer>("ClickPlayer").Play();
        changeScene(-1);
    }

    public void _on_NextButton_pressed()
    {
        GetNode<AudioStreamPlayer>("ClickPlayer").Play();
        if (!last_page) { changeScene(1); return; }
        endCutscene();
    }

    private async void endCutscene()
    {
        var fade = GetNode<FadeLayer>("FadeLayer");
        fade.startFade();
        await ToSignal(fade, "FadeDone");
        GetTree().ChangeScene(GDKnyttDataStore.CutsceneAfter);
    }
}
