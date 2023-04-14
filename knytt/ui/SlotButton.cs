using Godot;

public partial class SlotButton : Button
{
    [Export]
    public int slot = 1;

    [Signal]
    public delegate void StartGameEventHandler();

    public bool ConfirmActive
    {
        get { return GetNode<TextureRect>("ConfirmArrow").Visible; }
        set { GetNode<TextureRect>("ConfirmArrow").Visible = value; }
    }

    public bool EraseActive
    {
        get { return GetNode<TextureRect>("EraseArrow").Visible; }
        set { GetNode<TextureRect>("EraseArrow").Visible = value; }
    }

    public bool StartLoadActive
    {
        get { return GetNode<TextureRect>("StartLoadArrow").Visible; }
        set { GetNode<TextureRect>("StartLoadArrow").Visible = value; }
    }

    // Dir/filename without slot # or .ini
    string _base_file;
    public string BaseFile
    {
        get { return _base_file; }
        set
        {
            _base_file = value;
            checkSlot();
        }
    }

    public bool NewMode { get; private set; }

    public string FullFilename { get { return $"{BaseFile} {slot}.ini"; } }

    public override void _Ready()
    {
        Text = $"Slot {slot}";
        close();
    }

    // Check the slot and configure
    public void checkSlot()
    {
        close();
        using var f = FileAccess.Open(FullFilename, FileAccess.ModeFlags.Read);
        if (f != null && f.IsOpen()) { setupLoadMode(); }
        else { setupNewMode(); }
    }

    private void setupNewMode()
    {
        NewMode = true;
        GetNode<Button>("StartLoadArrow/StartLoadButton").Text = "Start New Game";
        GetNode<Control>("GreenLayer").Visible = false;
    }

    private void setupLoadMode()
    {
        NewMode = false;
        GetNode<Button>("StartLoadArrow/StartLoadButton").Text = "Load Game";
        GetNode<Control>("GreenLayer").Visible = true;
    }

    public void close()
    {
        ConfirmActive = false;
        EraseActive = false;
        StartLoadActive = false;
    }

    public void _on_SlotButton_pressed()
    {
        ClickPlayer.Play();
        GetNode<InfoScreen>("../..").closeOtherSlots(this.slot);
        StartLoadActive = true;
        if (!NewMode) { EraseActive = true; }
    }

    public void _on_EraseButton_pressed()
    {
        ClickPlayer.Play();
        ConfirmActive = true;
    }

    public void _on_ConfirmButton_pressed()
    {
        ClickPlayer.Play();
        DirAccess.RemoveAbsolute(FullFilename);
        checkSlot();
    }

    public void _on_StartLoadButton_pressed()
    {
        ClickPlayer.Play();
        // Message up to the level selection with a new? flag, the filename, and the slot number
        EmitSignal(SignalName.StartGame, NewMode, FullFilename, slot);
    }
}
