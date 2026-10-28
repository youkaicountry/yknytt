using Godot;
using System.Collections.Generic;
using System.Linq;
using YKnyttLib;

public class ObjectPalette : PopupPanel
{
    [Signal] public delegate void ObjectChosen(int bank, int obj);

    private const int TOUCH = -1;
    private const int RECENT_COUNT = 16;
    private static readonly Color MISSING_COLOR = new Color(1, 1, 1, 0.25f);
    private static readonly Color PLUS_COLOR = new Color(0.95f, 0.6f, 0.6f);

    private static readonly List<KnyttPoint> recent = new List<KnyttPoint>();

    private EditorAssets assets;
    private ObjectCatalog catalog;
    private KnyttPoint selected;
    private KnyttPoint? shown_info;

    private readonly List<int> bank_numbers = new List<int>();
    private readonly List<KnyttPoint> shown = new List<KnyttPoint>();

    private LineEdit search;
    private ItemList banks;
    private ItemList objects;
    private ItemList recent_list;
    private TextureRect info_icon;
    private Label info_name;
    private Label info_text;

    public override void _Ready()
    {
        search = GetNode<LineEdit>("%Search");
        banks = GetNode<ItemList>("%Banks");
        objects = GetNode<ItemList>("%Objects");
        recent_list = GetNode<ItemList>("%Recent");
        info_icon = GetNode<TextureRect>("%InfoIcon");
        info_name = GetNode<Label>("%InfoName");
        info_text = GetNode<Label>("%InfoText");
    }

    public void init(EditorAssets assets, ObjectCatalog catalog)
    {
        this.assets = assets;
        this.catalog = catalog;
        banks.Clear();
        bank_numbers.Clear();
        foreach (int bank in catalog.Banks)
        {
            banks.AddItem($"{bank}  {catalog.bankName(bank)}", assets.getBankSwatch(bank));
            bank_numbers.Add(bank);
        }
    }

    public void open(KnyttPoint selected)
    {
        this.selected = selected;
        search.Text = "";
        showBank(selected.x);
        showRecent();
        showInfo(selected);
        Popup_();
        search.GrabFocus();
    }

    public void addRecent(KnyttPoint id)
    {
        recent.Remove(id);
        recent.Insert(0, id);
        if (recent.Count > RECENT_COUNT) { recent.RemoveAt(RECENT_COUNT); }
    }

    private void showBank(int bank)
    {
        int index = bank_numbers.IndexOf(bank);
        if (index >= 0)
        {
            banks.Select(index);
            banks.EnsureCurrentIsVisible();
        }
        else { banks.UnselectAll(); }

        var ids = catalog.objects(bank).Select(obj => new KnyttPoint(bank, obj));
        fill(new[] { new KnyttPoint(bank, 0) }.Concat(ids), with_bank: false);
    }

    private void fill(IEnumerable<KnyttPoint> ids, bool with_bank)
    {
        objects.Clear();
        shown.Clear();
        foreach (var id in ids)
        {
            objects.AddItem(with_bank ? $"{id.x}:{id.y}" : $"{id.y}", id.y == 0 ? null : assets.getObject(id));
            int i = shown.Count;
            shown.Add(id);
            objects.SetItemTooltipEnabled(i, false);
            if (ObjectCatalog.isPlus(id)) { objects.SetItemCustomFgColor(i, PLUS_COLOR); }
            if (!catalog.exists(id) && id.y != 0)
            {
                objects.SetItemIconModulate(i, MISSING_COLOR);
                objects.SetItemCustomFgColor(i, MISSING_COLOR);
            }
            if (id.Equals(selected)) { objects.Select(i); }
        }
        objects.EnsureCurrentIsVisible();
    }

    private void showRecent()
    {
        recent_list.Clear();
        foreach (var id in recent)
        {
            recent_list.AddItem("", assets.getObject(id));
            recent_list.SetItemTooltipEnabled(recent_list.GetItemCount() - 1, false);
        }
        recent_list.Visible = recent.Count > 0;
    }

    private void showInfo(KnyttPoint id)
    {
        shown_info = id;
        info_icon.Texture = id.y == 0 ? null : assets.getObject(id);
        info_name.Text = $"{catalog.title(id)}    Bank {id.x}: {catalog.bankName(id.x)}";
        info_text.Text = catalog.details(id);
    }

    private void choose(KnyttPoint id)
    {
        Hide();
        EmitSignal(nameof(ObjectChosen), id.x, id.y);
    }

    // Hovering shows what it is, a click takes it. On touch the first tap shows, the second takes
    private void listInput(ItemList list, IList<KnyttPoint> ids, InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            int i = list.GetItemAtPosition(motion.Position, exact: true);
            showInfo(i >= 0 ? ids[i] : selected);
        }
        else if (@event is InputEventMouseButton button && button.Pressed && button.ButtonIndex == (int)ButtonList.Left)
        {
            int i = list.GetItemAtPosition(button.Position, exact: true);
            if (i < 0) { return; }
            if (button.Device == TOUCH && !ids[i].Equals(shown_info)) { showInfo(ids[i]); return; }
            choose(ids[i]);
        }
    }

    private void _on_Objects_gui_input(InputEvent @event)
    {
        listInput(objects, shown, @event);
    }

    private void _on_Recent_gui_input(InputEvent @event)
    {
        listInput(recent_list, recent, @event);
    }

    private void _on_List_mouse_exited()
    {
        showInfo(selected);
    }

    private void _on_Objects_item_selected(int index)
    {
        showInfo(shown[index]);
    }

    private void _on_Objects_item_activated(int index)
    {
        choose(shown[index]);
    }

    private void _on_Banks_item_selected(int index)
    {
        search.Text = "";
        showBank(bank_numbers[index]);
    }

    private void _on_Search_text_changed(string text)
    {
        if (text.Trim() == "")
        {
            showBank(selected.x);
            return;
        }
        banks.UnselectAll();
        fill(catalog.search(text), with_bank: true);
    }

    private void _on_Search_text_entered(string text)
    {
        if (text.Trim() != "" && shown.Count > 0) { choose(shown[0]); }
    }

    // Down moves from the search box into the objects
    private void _on_Search_gui_input(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_down") || shown.Count == 0) { return; }
        search.AcceptEvent();
        objects.GrabFocus();
        if (objects.GetSelectedItems().Length == 0) { objects.Select(0); }
        showInfo(shown[objects.GetSelectedItems()[0]]);
    }
}
