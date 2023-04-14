using Godot;

public partial class GameButton : Button
{
    public WorldEntry worldEntry;
    private ColorRect progressRect;
    private float startProgressLength;
    private float varProgressLength;

    [Signal]
    public delegate void GamePressedEventHandler(GameButton button);

    public void initialize(WorldEntry info)
    {
        worldEntry = info;

        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Visible = false;
        GetNode<Control>("MainContainer").Visible = true;
        GetNode<Control>("RatingControl").Visible = true;

        GetNode<TextureRect>("MainContainer/IconTexture").Texture = info.Icon;
        GetNode<TextureRect>("MainContainer/IconTexture").Size = new Vector2(30, 30);
        GetNode<Label>("MainContainer/TextContainer/NameLabel").Text = $"{info.Name} ({info.Author})";
        GetNode<Label>("MainContainer/TextContainer/DescriptionLabel").Text = info.Description;

        progressRect = GetNode<ColorRect>("ProgressRect");
        startProgressLength = progressRect.Size.X - progressRect.OffsetLeft;
        varProgressLength = Size.X - startProgressLength - progressRect.OffsetLeft * 2;

        var rating_control = GetNode<Control>("RatingControl");
        if (!worldEntry.HasServerInfo)
        {
            rating_control.Visible = false;
            CustomMinimumSize = new Vector2(CustomMinimumSize.X, 45);
        }
        else
        {
            rating_control.Visible = true;
            CustomMinimumSize = new Vector2(CustomMinimumSize.X, 55);
            rating_control.GetNode<Label>("SizeLabel").Text = $"{(worldEntry.FileSize / 1024f / 1024f):0.#} MB";
            rating_control.GetNode<Label>("UpvotesLabel").Text = $"+{worldEntry.Upvotes}";
            rating_control.GetNode<Label>("DownvotesLabel").Text = $"-{worldEntry.Downvotes}";
            rating_control.GetNode<Label>("DownloadsLabel").Text = $"{worldEntry.Downloads}";
            rating_control.GetNode<Label>("VerifiedLabel").Text =
                worldEntry.Approved ? "Approved" : worldEntry.Verified ? "Autoverified" : "Not Verified";
            rating_control.GetNode<Label>("VerifiedLabel").AddThemeColorOverride("font_color",
                worldEntry.Approved || worldEntry.Verified ? new Color(0, 0.5f, 0) : new Color(0.5f, 0, 0));
        }
    }

    public void _on_GameButton_pressed()
    {
        EmitSignal(SignalName.GamePressed, this);
    }

    private void setProgress(float progress, Color color)
    {
        progressRect.Visible = true;
        progressRect.Color = color;
        progressRect.Size = new Vector2(startProgressLength + varProgressLength * progress, progressRect.Size.Y);
    }

    public void setDownloaded(int bytes_count)
    {
        setProgress((float)bytes_count / worldEntry.FileSize, new Color(0.375f, 0.375f, 1, 0.5f));
    }

    public void markCompleted()
    {
        setProgress(1, new Color(0.375f, 1, 0.375f, 0.5f));
    }

    public void markFailed()
    {
        setProgress(1, new Color(1, 0.375f, 0.375f, 0.5f));
    }

    public void incDownloads()
    {
        var label = GetNode<Label>("RatingControl/DownloadsLabel");
        label.Text = $"{int.Parse(label.Text) + 1}";
    }
}
