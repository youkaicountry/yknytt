using Godot;

// Clicking the button of an open menu closes the menu, like a desktop menu bar
public class EditorMenuButton : MenuButton
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton button && button.Pressed && GetPopup().Visible &&
            GetGlobalRect().HasPoint(button.Position))
        {
            GetPopup().Hide();
            GetTree().SetInputAsHandled();
        }
    }
}
