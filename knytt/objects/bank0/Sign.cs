using Godot;
using System.Collections.Generic;
using System.Linq;
using YKnyttLib;

public class Sign : GDKnyttBaseObject
{
    private List<string> texts = new List<string>();
    private int messageIndex;
    private bool messageVisible;
    private int shiftMessageIndex;
    private int triggerMessageIndex;
    private AnimationPlayer player;
    private int areaCount;

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
        var x_pos = sign_rect.RectPosition.x;
        var y_pos = sign_rect.RectPosition.y;
        var size = sign_rect.RectSize;

        // TODO: original game doesn't overlap object area when showing sign
        if (Position.x + x_pos < 0) { x_pos = -Position.x; }
        if (Position.y + y_pos < 0) { y_pos = -Position.y; }
        if (Position.x + x_pos + size.x > 600) { x_pos = 600 - Position.x - size.x; }
        if (Position.y + y_pos + size.y > 240) { y_pos = 240 - Position.y - size.y; }

        sign_rect.RectPosition = new Vector2(x_pos, y_pos);
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
        areaCount++;

        var signs = GDArea.Objects.findObjects(new KnyttPoint(0, 17))
            .Union(GDArea.Objects.findObjects(new KnyttPoint(0, 18)))
            .Union(GDArea.Objects.findObjects(new KnyttPoint(0, 19)))
            .Where(s => s != this);
        foreach (Sign sign in signs)
        {
            if (sign != this)
            {
                sign.OnArea2DBodyExited(body, force_exit: true);
            }
        }

        if (areaCount == 1)
        {
            messageIndex = -1;
            nextMessage(juni);
        }

        if (!juni.IsConnected(nameof(Juni.DownEvent), this, nameof(nextMessage)) && 
            (texts.Count > 1 || shiftMessageIndex > 0 || triggerMessageIndex > 0))
        {
            juni.Connect(nameof(Juni.DownEvent), this, nameof(nextMessage));
        }
    }

    public void OnArea2DBodyExited(Node body, bool force_exit = false)
    {
        if (!(body is Juni juni)) { return; }
        areaCount = areaCount <= 0 || force_exit ? 0 : areaCount - 1;

        if (areaCount == 0)
        {
            if (juni.IsConnected(nameof(Juni.DownEvent), this, nameof(nextMessage)))
            {
                juni.Disconnect(nameof(Juni.DownEvent), this, nameof(nextMessage));
            }
            messageIndex = -2;
            nextMessage(juni);
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
