using Godot;
using System.Collections.Generic;
using System.Linq;
using YKnyttLib;

public partial class Sign : GDKnyttBaseObject
{
    private List<string> texts = new List<string>();
    private int messageIndex;
    private bool messageVisible;
    private int shiftMessageIndex;
    private int triggerMessageIndex;
    private AnimationPlayer player;

    private Dictionary<Juni, int> refCounter = new Dictionary<Juni, int>(); // ConnectFlags.ReferenceCounted doesn't work..

    public override void _Ready()
    {
        player = GetNode<AnimationPlayer>("AnimationPlayer");

        char letter = "ABC"[ObjectID.y - 17];

        string text = GDArea.Area.getExtraData($"Sign({letter})");
        texts.Add(preprocess(text));
        for (int i = 2; ; i++)
        {
            text = GDArea.Area.getExtraData($"Sign{i}({letter})");
            if (text == null) { break; }
            texts.Add(preprocess(text));
        }
        if (texts[0] == null && texts.Count == 1) { texts[0] = "<SIGN TEXT MISSING>"; }

        shiftMessageIndex = int.TryParse(GDArea.Area.getExtraData($"SignShift({letter})"), out var j) ? j : 0;
        triggerMessageIndex = int.TryParse(GDArea.Area.getExtraData($"SignTrig({letter})"), out j) ? j : 0;

        adjustSign();
    }

    private string preprocess(string msg)
    {
        if (msg == null) { return null; }
        if (msg.StartsWith("\"") && msg.EndsWith("\"")) { msg = msg.Substring(1, msg.Length - 2); }
        msg = msg.Replace("\\n", "\n");
        return msg;
    }

    protected void adjustSign()
    {
        var sign_rect = GetNode<Control>("SignRect");
        var x_pos = sign_rect.Position.X;
        var y_pos = sign_rect.Position.Y;
        var size = sign_rect.Size;

        // TODO: original game doesn't overlap object area when showing sign
        if (Position.X + x_pos < 0) { x_pos = -Position.X; }
        if (Position.Y + y_pos < 0) { y_pos = -Position.Y; }
        if (Position.X + x_pos + size.X > 600) { x_pos = 600 - Position.X - size.X; }
        if (Position.Y + y_pos + size.Y > 240) { y_pos = 240 - Position.Y - size.Y; }

        sign_rect.Position = new Vector2(x_pos, y_pos);
    }

    public void nextMessage(Juni juni)
    {
        messageIndex++;

        if (shiftMessageIndex > 0 && messageIndex == shiftMessageIndex)
        {
            Shift shift = GDArea.Objects.findObject(new KnyttPoint(0, ObjectID.y - 3)) as Shift;
            shift?.executeAnyway(juni);
        }

        if (triggerMessageIndex > 0 && messageIndex == triggerMessageIndex)
        {
            Trigger trigger = GDArea.Objects.findObject(new KnyttPoint(0, ObjectID.y + 15)) as Trigger;
            trigger?.executeAnyway(juni);
        }

        if (messageIndex >= texts.Count) { messageIndex = -1; }

        if (messageIndex != -1 && texts[messageIndex] != null)
        {
            GetNode<Label>("SignRect/Label").Text = texts[messageIndex];
            GetNode<Control>("SignRect/DownArrow").Visible = messageIndex < texts.Count - 1;
            if (!messageVisible) { player.Play("FadeIn"); messageVisible = true; }
        }
        else
        {
            if (messageVisible) { player.PlayBackwards("FadeIn"); messageVisible = false; }
        }
    }

    public void OnArea2DBodyEntered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        var signs = GDArea.Objects.findObjects(new KnyttPoint(0, 17))
            .Union(GDArea.Objects.findObjects(new KnyttPoint(0, 18)))
            .Union(GDArea.Objects.findObjects(new KnyttPoint(0, 19)));
        foreach (Sign sign in signs)
        {
            if (sign != this)
            {
                sign.OnArea2DBodyExited(body, exit_all: true);
            }
        }

        if (refCounter.ContainsKey(juni))
        {
            refCounter[juni]++;
        }
        else
        {
            if (refCounter.Count == 0) { messageIndex = -1; nextMessage(juni); }
            if (texts.Count > 1 || shiftMessageIndex > 0 || triggerMessageIndex > 0)
            {
                juni.DownEvent += nextMessage;
            }
            refCounter[juni] = 1;
        }
    }

    public void OnArea2DBodyExited(Node body, bool exit_all = false)
    {
        if (!(body is Juni juni) || !refCounter.ContainsKey(juni)) { return; }
        refCounter[juni]--;
        if (refCounter[juni] == 0 || exit_all)
        {
            if (texts.Count > 1) { juni.DownEvent -= nextMessage; }
            refCounter.Remove(juni);
            if (refCounter.Count == 0) { messageIndex = -2; nextMessage(juni); }
        }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        OnArea2DBodyEntered(body);
    }

    public void _on_Area2D_body_exited(Node body)
    {
        OnArea2DBodyExited(body);
    }
}
