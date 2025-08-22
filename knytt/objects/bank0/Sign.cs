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
    private Label label;
    private int areaCount;

    public override void _Ready()
    {
        player = GetNode<AnimationPlayer>("AnimationPlayer");
        label = GetNode<Label>("Label");

        char letter = "ABC"[ObjectID.y - 17];

        string text = GDArea.Area.getExtraData($"Sign({letter})");
        texts.Add(preprocess(text));
        for (int i = 2; ; i++)
        {
            text = GDArea.Area.getExtraData($"Sign{i}({letter})");
            if (text == null) { break; }
            texts.Add(preprocess(text));
        }
        if (texts[0] == null && texts.Count == 1) { texts[0] = "[SILENCE]"; }

        shiftMessageIndex = KnyttUtil.parseIniInt(GDArea.Area.getExtraData($"SignShift({letter})")) ?? 0;
        triggerMessageIndex = KnyttUtil.parseIniInt(GDArea.Area.getExtraData($"SignTrig({letter})")) ?? 0;

        adjustSign();

        if (GDArea.GDWorld.Game.SignFont != null)
        {
            label.AddFontOverride("font", GDArea.GDWorld.Game.SignFont);
            label.RemoveColorOverride("font_color");
        }
    }

    private string preprocess(string msg)
    {
        if (msg == null) { return null; }
        if (msg.StartsWith("\"") && msg.EndsWith("\"")) { msg = msg.Substring(1, msg.Length - 2); }
        msg = msg.Replace("\\n", "\n");
        msg = msg.Replace('\x7f', '\n');
        for (int i = 22; i < msg.Length; i += 22)
        {
            if (msg[i-1] == ' ' && msg[i-2] == ' ') { msg = msg.Remove(i - 1, 1).Insert(i - 1, "\n"); }
        }
        return msg;
    }

    protected void adjustSign()
    {
        var sign_rect = GetNode<Control>("Label/SignRect");
        var x_pos = label.RectPosition.x;
        var y_pos = label.RectPosition.y;
        var x_diff = Position.x + sign_rect.RectPosition.x;
        var y_diff = Position.y + sign_rect.RectPosition.y;
        var size = sign_rect.RectSize;

        float left_limit = 0;
        float right_limit = 600;
        if (GDKnyttSettings.FullHeightScroll)
        {
            float camera_in_area = Juni.Game.Camera.GlobalPosition.x - GDArea.GlobalPosition.x;
            float x_viewport = GetViewport().GetVisibleRect().Size.x;
            left_limit = camera_in_area - x_viewport / 2;
            right_limit = camera_in_area + x_viewport / 2;
        }

        // TODO: original game doesn't overlap object area when showing sign
        if (x_diff + x_pos < left_limit) { x_pos = left_limit - x_diff; }
        if (y_diff + y_pos < 0) { y_pos = -y_diff; }
        if (x_diff + x_pos + size.x > right_limit) { x_pos = right_limit - x_diff - size.x; }
        if (y_diff + y_pos + size.y > 240) { y_pos = 240 - y_diff - size.y; }

        label.RectPosition = new Vector2(x_pos, y_pos);
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
            label.Text = texts[messageIndex];
            GetNode<Control>("Label/SignRect/DownArrow").Visible = messageIndex < texts.Count - 1;
            if (!messageVisible)
            {
                if (GDKnyttSettings.FullHeightScroll) { adjustSign(); }
                player.Play("FadeIn");
                messageVisible = true;
            }
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
