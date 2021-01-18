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
        
        // Detect ambiance
        bool ambiance = false;
        if (song.EndsWith("a"))
        {
            ambiance = true;
            song = song.Substring(0, song.Length-1);
        }

        string loc = ambiance ? $"Ambiance/Ambi{song}.ogg" : $"Music/Song{song}.ogg";
        var stream = GDKnyttDataStore.KWorld.getWorldSound(loc) as AudioStream;
        
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
        return $"{GDKnyttDataStore.CutsceneName}/Scene{scene}.png";
    }

    public void _on_PreviousButton_pressed()
    {
        ClickPlayer.Play();
        changeScene(-1);
    }

    public void _on_NextButton_pressed()
    {
        ClickPlayer.Play();
        if (!last_page) { changeScene(1); return; }
        endCutscene();
    }

    private async void endCutscene()
    {
        var fade = GetNode<FadeLayer>("FadeLayer");
        fade.startFade();
        await ToSignal(fade, "FadeDone");
        if (GDKnyttDataStore.CutsceneAfter != null)
        {
            GetTree().ChangeScene(GDKnyttDataStore.CutsceneAfter);
        }
        else
        {
            GetTree().CurrentScene.QueueFree();
            GetTree().Root.AddChild(GDKnyttDataStore.CutsceneReturn);
            GetTree().CurrentScene = GDKnyttDataStore.CutsceneReturn;
            GetTree().Paused = false;
            (GetTree().Root.FindNode("GKnyttGame", owned: false) as GDKnyttGame).respawnJuni();
        }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("left") && current_scene > 1)  { _on_PreviousButton_pressed(); }
        if (Input.IsActionJustPressed("right")) { _on_NextButton_pressed(); }
    }
}
