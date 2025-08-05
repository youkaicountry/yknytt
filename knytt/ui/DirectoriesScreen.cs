using Godot;
using System;
using System.IO.Compression;


public class DirectoriesScreen : BasicScreen
{
    Label error_label;
    CheckBox download_box;

    public override void _Ready()
    {
        base._Ready();
        error_label = GetNode<Label>("VBoxContainer/ErrorLabel");
        download_box = GetNode<CheckBox>("VBoxContainer/WorldsContainer/DownloadBox");
        initFocus();
        updatePaths();
        disableKeyboard(this);
        if (OS.GetName() == "X11") { GetNode<Label>("VBoxContainer/Label2").Text += "On Linux, unpacked world files must be lowercase."; }
    }

    public override void initFocus()
    {
        GetNode<Button>("VBoxContainer/WorldsContainer/ChangeButton").GrabFocus();
    }

    private void updatePaths()
    {
        GetNode<Label>("VBoxContainer/WorldsContainer/DirLabel").Text = 
            GDKnyttSettings.WorldsDirectory == "" ? "Not set" : GDKnyttSettings.WorldsDirectory;
        download_box.Pressed = GDKnyttSettings.WorldsDirForDownload;
        download_box.Disabled = GDKnyttSettings.WorldsDirectory == "";
        GetNode<Label>("VBoxContainer/SavesContainer/DirLabel").Text = 
            GDKnyttSettings.SavesDirectory == "" ? "Default" : GDKnyttSettings.SavesDirectory;
    }

    private void disableKeyboard(Node node)
    {
        foreach (Node n in node.GetChildren())
        {
            if (n is LineEdit e) { e.VirtualKeyboardEnabled = false; }
            if (n is Button b && b.Text == "Create Folder") { b.Visible = false; }
            if (n is Label l && l.Text == "Directories & Files:") { l.Visible = false; }
            if (n.GetChildCount() > 0) { disableKeyboard(n); }
        }
    }

    private void gamepadArrowsMode(bool tab)
    {
        InputMap.ActionEraseEvent(tab ? "ui_left" : "ui_focus_prev", new InputEventJoypadButton() { ButtonIndex = (int)JoystickList.DpadLeft, Device = -1 });
        InputMap.ActionEraseEvent(tab ? "ui_right" : "ui_focus_next", new InputEventJoypadButton() { ButtonIndex = (int)JoystickList.DpadRight, Device = -1 });
        InputMap.ActionAddEvent(tab ? "ui_focus_prev" : "ui_left", new InputEventJoypadButton() { ButtonIndex = (int)JoystickList.DpadLeft, Device = -1 });
        InputMap.ActionAddEvent(tab ? "ui_focus_next" : "ui_right", new InputEventJoypadButton() { ButtonIndex = (int)JoystickList.DpadRight, Device = -1 });
    }

    private void _on_WorldsChangeButton_pressed()
    {
        gamepadArrowsMode(tab: true);
        GetNode<FileDialog>("WorldsFileDialog").CurrentDir = OS.GetSystemDir(OS.SystemDir.Downloads);
        GetNode<FileDialog>("WorldsFileDialog").PopupCentered();
    }

    private void _on_WorldsResetButton_pressed()
    {
        GDKnyttSettings.WorldsDirectory = "";
        updatePaths();
        error_label.Text = "";
    }

    private void _on_DownloadBox_toggled(bool button_pressed)
    {
        if (GDKnyttSettings.WorldsDirectory == "") { return; }
        if (!checkDir(GDKnyttSettings.WorldsDirectory, button_pressed)) { return; }
        GDKnyttSettings.WorldsDirForDownload = button_pressed;
    }

    private void _on_SavesChangeButton_pressed()
    {
        gamepadArrowsMode(tab: true);
        GetNode<FileDialog>("SavesFileDialog").CurrentDir = OS.GetSystemDir(OS.SystemDir.Documents);
        GetNode<FileDialog>("SavesFileDialog").PopupCentered();
    }

    private void _on_SavesResetButton_pressed()
    {
        GDKnyttSettings.SavesDirectory = "";
        updatePaths();
        error_label.Text = "";
    }

    private void _on_SavesBackupButton_pressed()
    {
        gamepadArrowsMode(tab: true);
        GetNode<FileDialog>("BackupFileDialog").CurrentDir = OS.GetSystemDir(OS.SystemDir.Documents);
        GetNode<FileDialog>("BackupFileDialog").CurrentFile = "yknytt-saves.zip";
        GetNode<FileDialog>("BackupFileDialog").PopupCentered();
    }

    private bool checkWritable(string dir)
    {
        try
        {
            File f = new File();
            var err = f.Open(dir.PlusFile("test.ini"), File.ModeFlags.Write);
            if (err != Error.Ok) { return false; }
            f.StoreLine("test");
            f.Close();
            err = new Directory().Remove(dir.PlusFile("test.ini"));
            if (err != Error.Ok) { return false; }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    private bool checkReadable(string dir)
    {
        var d = new Directory();
        if (d.Open(dir) != Error.Ok || d.ListDirBegin() != Error.Ok) { return false; }
        d.ListDirEnd();
        return true;
    }

    private bool checkDir(string dir, bool write)
    {
        if (write ? !checkWritable(dir) : !checkReadable(dir))
        {
            error_label.Text = "Cannot " + (write ? "write to" : "read") + " selected directory!\n" +
                (OS.GetName() != "Android" ? "Make sure you have all required permission." :
                "Please grant permissions in Settings -> Apps -> YKnytt -> Permissions -> Storage.");
            return false;
        }
        error_label.Text = "";
        return true;
    }

    private void _on_WorldsFileDialog_dir_selected(String dir)
    {
        if (!checkDir(dir, GDKnyttSettings.WorldsDirForDownload)) { return; }
        GDKnyttSettings.WorldsDirectory = dir;
        updatePaths();
    }

    private void _on_SavesFileDialog_dir_selected(String dir)
    {
        if (!checkDir(dir, true)) { return; }
        GDKnyttSettings.SavesDirectory = dir;
        updatePaths();
    }

    private void _on_BackupFileDialog_file_selected(String path)
    {
        string src_dir = GDKnyttSettings.SavesDirectory == "" ? GDKnyttDataStore.BaseDataDirectory.PlusFile("Saves") : GDKnyttSettings.SavesDirectory;
        try
        {
            if (new Directory().FileExists(path)) { new Directory().Remove(path); }
            ZipFile.CreateFromDirectory(src_dir, path);
            error_label.Text = $"{path} was successfully created.";
        }
        catch (Exception)
        {
            error_label.Text = "Cannot create zip. Please check that you have access to this directory." + 
            (OS.GetName() == "Android" ? "\nGrant permissions in Settings -> Apps -> YKnytt -> Permissions -> Storage." : "");
        }
    }

    private void _on_FileDialog_popup_hide()
    {
        gamepadArrowsMode(tab: false);
        initFocus();
    }
}
