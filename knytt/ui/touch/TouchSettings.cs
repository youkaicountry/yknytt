using Godot;
using System;

public class TouchSettings : Node
{
    public enum VerticalPosition
    {
        Top = 0,
        BelowTop = 1,
        AboveBottom = 2,
        Bottom = 3
    }

    public static bool EnablePanel
    {
        get { return GDKnyttSettings.ini["TouchPanel"]["Enable"].Equals("1"); }
        set
        {
            GDKnyttSettings.ini["TouchPanel"]["Enable"] = value ? "1" : "0";
            GDKnyttSettings.SmoothScaling = GDKnyttSettings.SmoothScaling;
        }
    }

    public static bool SwapHands
    {
        get { return GDKnyttSettings.ini["TouchPanel"]["Swap"].Equals("1"); }
        set { GDKnyttSettings.ini["TouchPanel"]["Swap"] = value ? "1" : "0"; }
    }

    public static VerticalPosition Position
    {
        get { return (VerticalPosition)Enum.Parse(typeof(VerticalPosition), GDKnyttSettings.ini["TouchPanel"]["VerticalPosition"]); }
        set { GDKnyttSettings.ini["TouchPanel"]["VerticalPosition"] = value.ToString(); }
    }
    
    public static bool Swipe
    {
        get { return GDKnyttSettings.ini["TouchPanel"]["Swipe"].Equals("1"); }
        set { GDKnyttSettings.ini["TouchPanel"]["Swipe"] = value ? "1" : "0"; }
    }

    public static float Scale
    {
        get { return float.Parse(GDKnyttSettings.ini["TouchPanel"]["Scale"]); }
        set { GDKnyttSettings.ini["TouchPanel"]["Scale"] = value.ToString(); }
    }

    public static float Viewport
    {
        get { return float.Parse(GDKnyttSettings.ini["TouchPanel"]["Viewport"]); }
        set { GDKnyttSettings.ini["TouchPanel"]["Viewport"] = value.ToString(); }
    }
    
    public static float ScreenWidth
    {
        get { return EnablePanel ? 600 / Viewport : 600; }
    }
    
    public static float JumpScale
    {
        get { return float.Parse(GDKnyttSettings.ini["TouchPanel"]["JumpScale"]); }
        set { GDKnyttSettings.ini["TouchPanel"]["JumpScale"] = value.ToString(); }
    }

    public static float Opacity
    {
        get { return float.Parse(GDKnyttSettings.ini["TouchPanel"]["Opacity"]); }
        set { GDKnyttSettings.ini["TouchPanel"]["Opacity"] = value.ToString(); }
    }

    public static float PanelAnchor
    {
        get { return new[] { 1f, 0.8f, 0.2f, 0f }[(int)Position]; }
    }

    public static float AreaAnchor
    {
        get { return new[] { 0f, 0f, 1f, 1f }[(int)Position]; }
    }

    public static bool ensureSettings()
    {
        bool modified = false;
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Enable", OS.GetName() == "Android" || OS.GetName() == "iOS" ? "1" : "0");
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Swap", "0");
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "VerticalPosition", VerticalPosition.Top.ToString());
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Swipe", "1");
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Scale", "1");
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Viewport", (0.85).ToString());
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "JumpScale", "1");
        modified |= GDKnyttSettings.ensureSetting("TouchPanel", "Opacity", (0.4).ToString());
        return modified;
    }

    public static void applyAllSettings(SceneTree tree)
    {
        tree.Root.GetNodeOrNull<TouchPanel>("GKnyttGame/UICanvasLayer/TouchPanel")?.Configure();
    }
}
