using Godot;
using YKnyttLib;

// A swatch for a field holding a colour as red + green×256 + blue×65536. Empty shows the checkers. Right-click clears
public class IniColorButton : ColorPickerButton
{
    [Export] public NodePath Field { get; set; } = new NodePath();
    // 0 turns this key off, so black is written as 65536
    [Export] public bool ZeroIsOff { get; set; }

    private static readonly Color EDGE = new Color(0.6f, 0.6f, 0.65f);
    private IniLineEdit field;

    public override void _Ready()
    {
        field = GetNode<IniLineEdit>(Field);
        field.Shown += show;
        EditAlpha = false;
        Connect("button_down", this, nameof(opening));
        Connect("color_changed", this, nameof(changed));
        Connect("popup_closed", this, nameof(closed));
        Connect("gui_input", this, nameof(input));
    }

    private void show(string text)
    {
        if (!GetPopup().Visible) { paint(text); }
    }

    private void paint(string text)
    {
        int? value = KnyttUtil.parseIniInt(text);
        Color = value == null ? new Color(1, 1, 1, 0) : Color.Color8((byte)(value & 255), (byte)((value >> 8) & 255), (byte)((value >> 16) & 255));
    }

    // An empty field opens on white
    private void opening()
    {
        if (Color.a == 0) { Color = Colors.White; }
    }

    private void changed(Color color)
    {
        int value = color.r8 + color.g8 * 256 + color.b8 * 65536;
        field.choose(value == 0 && ZeroIsOff ? "65536" : value.ToString(), done: false);
    }

    private void closed()
    {
        field.done();
        paint(field.Value);
    }

    private void input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton click && click.Pressed && click.ButtonIndex == (int)ButtonList.Right)) { return; }
        AcceptEvent();
        field.choose("");
    }

    // A thin edge, so white shows on the white field
    public override void _Draw()
    {
        DrawRect(new Rect2(new Vector2(3, 3), RectSize - new Vector2(6, 6)), EDGE, false);
    }
}
