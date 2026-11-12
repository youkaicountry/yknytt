using Godot;
using YKnyttLib;

// A swatch for MapColor that opens the 64 colours. Empty or 64 shows the checkers. Right-click clears
public class IniMapColorButton : Button
{
    [Export] public NodePath Field { get; set; } = new NodePath();

    private IniLineEdit field;
    private int color = -1;

    public override void _Ready()
    {
        field = GetNode<IniLineEdit>(Field);
        field.Shown += show;
        Connect("pressed", this, nameof(pressed));
        Connect("gui_input", this, nameof(input));
    }

    private void show(string text)
    {
        int value = KnyttUtil.parseIniInt(text) ?? -1;
        color = value >= 0 && value < 64 ? value : -1;
        Update();
    }

    private void pressed()
    {
        ((LevelEditor)Owner).pickMapColor(this, color, chosen => field.choose(chosen.ToString()));
    }

    private void input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton click && click.Pressed && click.ButtonIndex == (int)ButtonList.Right)) { return; }
        AcceptEvent();
        field.choose("");
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, RectSize).Grow(-3);
        if (color < 0) { DrawTextureRect(GetIcon("bg", "ColorPickerButton"), rect, true); }
        else { DrawRect(rect, MapColorGrid.colorOf(color)); }
        DrawRect(rect, new Color(0.6f, 0.6f, 0.65f), false);
    }
}
