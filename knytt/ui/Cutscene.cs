using Godot;
using YKnyttLib;

public partial class Cutscene : CanvasLayer
{
    private int current_scene = 0;
    private bool has_next;
    private bool last_page;

    public override void _Ready()
    {
        if (GDKnyttDataStore.CutsceneFadeIn)
        {
            GetNode<FadeLayer>("FadeLayer").startFade(is_out: false, color: getCutsceneColor());
        }
        
        if (GDKnyttDataStore.CutsceneSound != null)
        {
            GetNode<StandartSoundPlayer>("StandartSoundPlayer").playSound(GDKnyttDataStore.CutsceneSound);
        }

        changeScene(1);
        loadMusic();
        releaseAll();
    }

    public static Color getCutsceneColor(string name = null)
    {
        int bgr = KnyttUtil.parseBGRString(GDKnyttDataStore.KWorld.INIData["Cutscene Color"][name ?? GDKnyttDataStore.CutsceneName], 0xFFFFFF);
        return new Color(KnyttUtil.R(bgr), KnyttUtil.G(bgr), KnyttUtil.B(bgr));
    }

    private void loadMusic()
    {
        string song = GDKnyttDataStore.KWorld.INIData["Cutscene Music"][GDKnyttDataStore.CutsceneName];
        if (song == null || song == "") { return; }

        // Detect ambiance
        bool ambiance = false;
        if (song.EndsWith("a"))
        {
            ambiance = true;
            song = song.Substring(0, song.Length - 1);
        }

        string loc = ambiance ? $"Ambiance/Ambi{song}.ogg" : $"Music/Song{song}.ogg";
        var stream = GDKnyttDataStore.KWorld.getWorldSound(loc, loop: false) as AudioStream;

        var player = GetNode<AudioStreamPlayer>("MusicPlayer");
        player.Stream = stream;
        player.Play();
    }

    private void changeScene(int delta)
    {
        current_scene += delta;
        Texture2D t = GDKnyttDataStore.KWorld.getWorldTexture(makeScenePath(current_scene)) as Texture2D;
        //if (t != null) { t.Flags |= (uint)Texture2D.FlagsEnum.Filter; }
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

        last_page = !GDKnyttDataStore.KWorld.worldFileExists(makeScenePath(current_scene + 1));
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
        SetProcess(false);
        var fade = GetNode<FadeLayer>("FadeLayer");
        fade.startFade(color: getCutsceneColor());
        GetNode<AnimationPlayer>("MusicPlayer/AnimationPlayer").Play("FadeOut"); // both fadeouts should be synchronized
        await ToSignal(fade, "FadeDone");
        if (GDKnyttDataStore.CutsceneAfter != null)
        {
            GetTree().ChangeSceneToFile(GDKnyttDataStore.CutsceneAfter);
            releaseAll();
        }
        else
        {
            GetTree().CurrentScene.QueueFree();
            GetTree().Root.AddChild(GDKnyttDataStore.CutsceneReturn);
            GetTree().CurrentScene = GDKnyttDataStore.CutsceneReturn;
            GetTree().Paused = false;
            (GetTree().Root.FindChild("GKnyttGame", owned: false) as GDKnyttGame).respawnJuni();
            releaseAll();
        }
    }

    private static readonly string[] actions = {"left", "right", "up", "down",
        "show_info", "debug_die", "umbrella", "walk", "jump", "hologram"};

    public static void releaseAll()
    {
        foreach (string a in actions) { Input.ActionRelease(a); }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("left") && current_scene > 1) { _on_PreviousButton_pressed(); }
        if (Input.IsActionJustPressed("right")) { _on_NextButton_pressed(); }
    }

    private void _on_RateHTTPRequest_ready()
    {
        if (GDKnyttDataStore.Mode == GDKnyttDataStore.CutsceneMode.Intro) { return; }

        var action = GDKnyttDataStore.Mode == GDKnyttDataStore.CutsceneMode.Middle &&
                     GDKnyttDataStore.CutsceneName.ToLower() != "ending" ?
                        RateHTTPRequest.Action.Cutscene : RateHTTPRequest.Action.Ending;

        GetNode<RateHTTPRequest>("RateHTTPRequest").send(
            GDKnyttDataStore.KWorld.Info.Name, GDKnyttDataStore.KWorld.Info.Author, (int)action, GDKnyttDataStore.CutsceneName);
    }
}
