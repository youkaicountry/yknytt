using Godot;

// TODO: merge with KnyttWorldManager<Texture>.WorldEntry?
public class GameButtonInfo
{
    public string Name;
    public string Author;
    public string Description;
    public Texture Icon;
    public bool HasServerInfo;
    public string Link;
    public int FileSize;
    public int Upvotes;
    public int Downvotes;
    public int Downloads;

    public static GameButtonInfo fromLocal(Texture icon, GDKnyttWorldImpl world)
    {
        var info = new GameButtonInfo();
        info.Icon = icon;
        info.Name = world.Info.Name;
        info.Author = world.Info.Author;
        info.Description = world.Info.Description;
        info.HasServerInfo = false;
        return info;
    }
}

public class GameButton : Button
{
    public GDKnyttWorldImpl KWorld { get; set; } // TODO: LevelSelection relies on KWorld to determine if a level was downloaded or not. Is it the best way?

    public GameButtonInfo buttonInfo;
    private ColorRect progressRect;
    private float startProgressLength;
    private float varProgressLength;

    [Signal]
    public delegate void GamePressed();

    public void initialize(GameButtonInfo info)
    {
        buttonInfo = info;

        GetNode<TextureRect>("MainContainer/IconTexture").Texture = info.Icon;
        GetNode<TextureRect>("MainContainer/IconTexture").RectSize = new Vector2(30, 30);
        GetNode<Label>("MainContainer/TextContainer/NameLabel").Text = $"{info.Name} ({info.Author})";
        GetNode<Label>("MainContainer/TextContainer/DescriptionLabel").Text = info.Description;

        progressRect = GetNode<ColorRect>("ProgressRect");
        startProgressLength = progressRect.RectSize.x - progressRect.MarginLeft;
        varProgressLength = RectSize.x - startProgressLength - progressRect.MarginLeft * 2;

        var rating_control = GetNode<Control>("RatingControl");
        if (!buttonInfo.HasServerInfo)
        {
            rating_control.Visible = false;
            RectMinSize = new Vector2(RectMinSize.x, 45);
        }
        else
        {
            rating_control.Visible = true;
            RectMinSize = new Vector2(RectMinSize.x, 55);
            rating_control.GetNode<Label>("SizeLabel").Text = $"{(buttonInfo.FileSize/1024f/1024f):0.#} MB";
            rating_control.GetNode<Label>("UpvotesLabel").Text = $"+{buttonInfo.Upvotes}";
            rating_control.GetNode<Label>("DownvotesLabel").Text = $"-{buttonInfo.Downvotes}";
            rating_control.GetNode<Label>("DownloadsLabel").Text = $"{buttonInfo.Downloads}";
        }
    }

    public void _on_GameButton_pressed()
    {
        EmitSignal(nameof(GamePressed), this);
    }

    private void setProgress(float progress, Color color)
    {
        progressRect.Visible = true;
        progressRect.Color = color;
        progressRect.RectSize = new Vector2(startProgressLength + varProgressLength * progress, progressRect.RectSize.y);
    }

    public void setDownloaded(int bytes_count)
    {
        setProgress((float)bytes_count / buttonInfo.FileSize, new Color(0.375f, 0.375f, 1, 0.5f));
    }

    public void markCompleted()
    {
        setProgress(1, new Color(0.375f, 1, 0.375f, 0.5f));
        var label = GetNode<Label>("RatingControl/DownloadsLabel");
        label.Text = $"{int.Parse(label.Text) + 1}";
    }

    public void markFailed()
    {
        setProgress(1, new Color(1, 0.375f, 0.375f, 0.5f));
    }
}
