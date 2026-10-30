using Godot;
using System;
using System.Collections.Generic;
using YKnyttLib;

// Collectables as a grid of cells. A click flips one, dragging sets the rest the same way
public class CollectGrid : Control
{
    [Signal] public delegate void Changed();

    [Export] public int First { get; set; } = 1;
    [Export] public int Count { get; set; } = 50;
    [Export] public int Columns { get; set; } = 25;

    private const int CELL = 10;
    private const int STEP = CELL + 1;
    private static readonly Color EMPTY_COLOR = new Color(0.32f, 0.32f, 0.32f);
    private static readonly Color COLLECTED_COLOR = Color.Color8(247, 195, 79);
    private static readonly Color PLACED_COLOR = new Color(0.75f, 0.75f, 0.75f);

    public bool[] Collected { get; set; }
    public Dictionary<int, KnyttPoint> Placed { get; set; }
    public Func<int, string> Describe { get; set; }

    private bool? setting;

    public override void _Ready()
    {
        RectMinSize = new Vector2(Columns, (Count + Columns - 1) / Columns) * STEP;
    }

    public override void _Draw()
    {
        if (Collected == null) { return; }
        for (int i = 0; i < Count; i++)
        {
            var rect = new Rect2(new Vector2(i % Columns, i / Columns) * STEP, CELL, CELL);
            if (Placed.ContainsKey(First + i)) { DrawRect(rect.Grow(1), PLACED_COLOR, filled: false); }
            DrawRect(rect, Collected[First + i] ? COLLECTED_COLOR : EMPTY_COLOR);
        }
    }

    private int? idAt(Vector2 position)
    {
        var cell = (position / STEP).Floor();
        int i = (int)cell.y * Columns + (int)cell.x;
        return cell.x >= 0 && cell.x < Columns && cell.y >= 0 && i < Count ? First + i : (int?)null;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button && button.ButtonIndex == (int)ButtonList.Left)
        {
            setting = null;
            if (button.Pressed && idAt(button.Position) is int id)
            {
                setting = !Collected[id];
                set(id);
            }
        }
        else if (@event is InputEventMouseMotion motion && idAt(motion.Position) is int id)
        {
            HintTooltip = Describe?.Invoke(id) ?? "";
            if (setting != null) { set(id); }
        }
    }

    private void set(int id)
    {
        if (Collected[id] == setting) { return; }
        Collected[id] = setting.Value;
        Update();
        EmitSignal(nameof(Changed));
    }
}
