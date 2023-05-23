using Godot;
using System;
using System.Text;
using YKnyttLib.Logging;
using System.Collections.Generic;
using YKnyttLib.Parser;

public class Console : CanvasLayer, IKnyttLoggerTarget
{
    [Signal] public delegate void ConsoleOpen();
    [Signal] public delegate void ConsoleClosed();

    [Export] public int HistoryLength = 256;
    [Export] public float SlideSpeed = 5f;

    bool showing = false;
    bool sliding_out = false;
    HashSet<char> disallowed = new HashSet<char>(new char[] {'`'});
    LineEdit lineEdit;
    RichTextLabel textLabel;

    LinkedList<string> displayBuffer;
    List<string> backBuffer;

    CommandParser parser;
    ConsoleExecutionEnvironment environment;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Setup logger
        KnyttLogger.AddTarget(new GodotLoggerTarget());
        KnyttLogger.AddTarget(this);

        // Setup parser
        var cs = ConsoleCommands.BuildCommandSet();
        parser = new CommandParser(cs);

        environment = new ConsoleExecutionEnvironment(parser, this);

        displayBuffer = new LinkedList<string>();
        backBuffer = new List<string>();
        var consoleContainer = GetNode<Control>("ConsoleContainer");
        consoleContainer.MarginTop = -240;
        consoleContainer.MarginBottom = -240;
        lineEdit = GetNode<LineEdit>("ConsoleContainer/Panel/LineEdit");
        textLabel = GetNode<RichTextLabel>("ConsoleContainer/Panel/RichTextLabel");
        AddMessage("[color=#cc00FF]Welcome to YKnytt![/color]");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("debug_console"))
        {
            toggleConsole();
            flushBuffer();
            if (showing)
            {
                lineEdit.ReleaseFocus();
                EmitSignal(nameof(ConsoleClosed));
            }
        }
    }

    public void NewMessage(KnyttLogMessage message)
    {
        AddMessage(message.RenderBBCode());
    }

    public void AddMessage(string message)
    {
        backBuffer.Add(message);
        if (showing) { flushBuffer(); }
    }

    public void toggleConsole()
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");

        if (!anim.IsPlaying())
        {
            anim.PlaybackSpeed = SlideSpeed * (1f/Godot.Engine.TimeScale);
            if (showing) { anim.PlayBackwards("SlideOut"); sliding_out = false; }
            else { anim.Play("SlideOut"); sliding_out = true; }
        }
        else
        {
            anim.PlaybackSpeed *= -1f;
            sliding_out = !sliding_out;
        }
    }

    private void handleOpen()
    {
        lineEdit.GrabFocus();
        EmitSignal(nameof(ConsoleOpen));
        flushBuffer();
    }

    private void handleClose()
    {
        lineEdit.ReleaseFocus();
    }

    private void flushBuffer()
    {
        bool dirty = backBuffer.Count > 0;
        foreach (var message in backBuffer)
        {
            displayBuffer.AddLast(message);
        }
        backBuffer.Clear();

        while (displayBuffer.Count > HistoryLength)
        {
            displayBuffer.RemoveFirst();
        }

        if (dirty) { render(); }
    }

    private void render()
    {
        textLabel.BbcodeText = string.Join("\n", displayBuffer);
    }

    public void _on_AnimationPlayer_animation_finished(string name)
    {
        showing = sliding_out;
        if (showing) { handleOpen(); }
        else { handleClose(); }
    }

    public void _on_LineEdit_text_changed(string newText)
    {
        var caretPosition = lineEdit.CaretPosition;

        StringBuilder sb = new StringBuilder();
        foreach (var c in newText)
        {
            if (!disallowed.Contains(c)) { sb.Append(c); }
        }

        lineEdit.Text = sb.ToString();
        lineEdit.CaretPosition = caretPosition;
    }

    public void RunCommand(string command)
    {
        var p = parser.Parse(command);
        if (p.Error != null)
        {
            AddMessage($"[color=#CC0000]{p.Error}[/color]");
            return;
        }

        // Parse the commands
        foreach (var res in p.Results.Results)
        {
            var cmd = res.Decl.Instantiation(res);
            var err = cmd.Execute(environment);
            if (err != null) { AddMessage($"[color=#CC0000]{err}[/color]"); }
        }
    }

    public void _on_LineEdit_text_entered(string enteredText)
    {
        AddMessage($"> [color=#00CC00]{lineEdit.Text}[/color]");
        RunCommand(lineEdit.Text);
        lineEdit.Text = "";
    }
}
