using System.Collections.Generic;

public class AltInput
{
    private HashSet<string> justPressed = new HashSet<string>();
    private HashSet<string> justReleased = new HashSet<string>();
    private HashSet<string> pressed = new HashSet<string>();

    public void FinishFrame()
    {
        justPressed.Clear();
        justReleased.Clear();
    }

    public void ClearInput()
    {
        FinishFrame();
        pressed.Clear();
    }

    public void ActionPress(string action)
    {
        pressed.Add(action);
        justPressed.Add(action);
    }
    
    public void ActionRelease(string action)
    {
        pressed.Remove(action);
        justReleased.Add(action);
    }

    public bool IsActionPressed(string action)
    {
        return pressed.Contains(action);
    }

    public bool IsActionJustPressed(string action)
    {
        return justPressed.Contains(action);
    }

    public bool IsActionJustReleased(string action)
    {
        return justReleased.Contains(action);
    }
}
