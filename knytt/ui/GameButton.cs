using Godot;
using System.Linq;

public class GameButton : GDKnyttButton
{
    public WorldEntry worldEntry;
    private ColorRect progressRect;
    private Label descriptionLabel;
    private float startProgressLength;
    private float varProgressLength;

    [Signal]
    public delegate void GamePressed();

    public void initialize(WorldEntry info)
    {
        worldEntry = info;

        GetNode<AnimatedSprite>("AnimatedSprite").Visible = false;
        GetNode<Control>("MainContainer").Visible = true;
        GetNode<Control>("RatingControl").Visible = true;

        GetNode<TextureRect>("MainContainer/IconTexture").Texture = info.Icon;
        GetNode<TextureRect>("MainContainer/IconTexture").RectSize = new Vector2(30, 30);
        GetNode<Label>("MainContainer/TextContainer/NameLabel").Text = $"{info.Name} ({info.Author})";
        descriptionLabel = GetNode<Label>("MainContainer/TextContainer/DescriptionLabel");
        descriptionLabel.Text = info.Description;

        progressRect = GetNode<ColorRect>("ProgressRect");
        startProgressLength = progressRect.RectSize.x - progressRect.MarginLeft;
        varProgressLength = RectSize.x - startProgressLength - progressRect.MarginLeft * 2;

        var rating_control = GetNode<Control>("RatingControl");
        if (!worldEntry.HasServerInfo)
        {
            rating_control.Visible = false;
            RectMinSize = new Vector2(RectMinSize.x, 45);
        }
        else
        {
            rating_control.Visible = true;
            RectMinSize = new Vector2(RectMinSize.x, 55);
            rating_control.GetNode<Label>("SizeLabel").Text = $"{(worldEntry.FileSize / 1024f / 1024f):0.#} MB";
            rating_control.GetNode<Label>("UpvotesLabel").Text = $"+{worldEntry.Upvotes}";
            rating_control.GetNode<Label>("DownvotesLabel").Text = $"-{worldEntry.Downvotes}";
            rating_control.GetNode<Label>("DownloadsLabel").Text = $"{worldEntry.Downloads}";
            rating_control.GetNode<Label>("VerifiedLabel").Text = worldEntry.StatusDescription;
            rating_control.GetNode<Label>("VerifiedLabel").AddColorOverride("font_color", worldEntry.StatusColor);
        }

        hint = capitalize(worldEntry.Size + " " +
                string.Join("/", worldEntry.Difficulties.Where(s => !string.IsNullOrWhiteSpace(s))) + " " +
                string.Join("/", worldEntry.Categories.Where(s => !string.IsNullOrWhiteSpace(s))))
                .Replace("environmental", "environment").Replace("misc", "misc level").Replace("  ", " ");
        if (HasFocus()) { _on_GameButton_ShowHint(hint); }
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

    public void markDisabled()
    {
        setProgress(1, new Color(0.375f, 0.375f, 0.375f, 0.5f));
    }

    public void markCompletedByPlayer()
    {
        setProgress(1, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    public void markClear()
    {
        setProgress(1, new Color(1f, 1f, 1f, 0));
    }

    public void incDownloads()
    {
        var label = GetNode<Label>("RatingControl/DownloadsLabel");
        label.Text = $"{int.Parse(label.Text) + 1}";
    }

    private string capitalize(string s)
    {
        s = s.Trim();
        return s == "" ? "" : (s[0].ToString().ToUpper() + s.Substring(1).ToLower());
    }

    private void _on_GameButton_ShowHint(string hint)
    {
        if (ignore_focus) { ignore_focus = false; return; }
        descriptionLabel.Text = hint == null ? worldEntry.Description : hint;
        descriptionLabel.AddColorOverride("font_color", hint == null ? new Color(0, 0, 0) : new Color(0, 0.5f, 0));
    }

    bool ignore_focus;

    public void forceGrabFocus()
    {
        ignore_focus = true;
        GrabFocus();
    }
}
