using System.Collections.Generic;
using YKnyttLib;

public class EditorHistory
{
    private interface IChange
    {
        void undo();
        void redo();
    }

    private class TileChange : IChange
    {
        public KnyttArea area;
        public int layer;
        public int index;
        public byte before;
        public byte after;

        public void undo() { area.TileLayers[layer].tiles[index] = before; }
        public void redo() { area.TileLayers[layer].tiles[index] = after; }
    }

    private class ObjectChange : IChange
    {
        public KnyttArea area;
        public int layer;
        public int index;
        public KnyttPoint before;
        public KnyttPoint after;

        public void undo() { set(before); }
        public void redo() { set(after); }

        private void set(KnyttPoint id)
        {
            area.ObjectLayers[layer].lo[index] = (byte)id.x;
            area.ObjectLayers[layer].hi[index] = (byte)id.y;
        }
    }

    private class AreaAdded : IChange
    {
        public KnyttWorld world;
        public KnyttArea area;

        public void undo() { world.removeArea(area.Position); }
        public void redo() { world.addArea(area); }
    }

    private readonly List<List<IChange>> done = new List<List<IChange>>();
    private readonly List<List<IChange>> undone = new List<List<IChange>>();
    private List<IChange> step;
    private int saved_at = 0;

    public bool Modified => saved_at != done.Count;

    public void begin()
    {
        if (step == null) { step = new List<IChange>(); }
    }

    public void end()
    {
        if (step == null) { return; }
        if (step.Count > 0)
        {
            if (saved_at > done.Count) { saved_at = -1; }
            done.Add(step);
            undone.Clear();
        }
        step = null;
    }

    public void setTile(KnyttArea area, int layer, int index, byte tile)
    {
        byte before = area.TileLayers[layer].tiles[index];
        if (before == tile) { return; }
        record(new TileChange() { area = area, layer = layer, index = index, before = before, after = tile });
    }

    public void setObject(KnyttArea area, int layer, int index, KnyttPoint id)
    {
        var before = new KnyttPoint(area.ObjectLayers[layer].lo[index], area.ObjectLayers[layer].hi[index]);
        if (before.Equals(id)) { return; }
        record(new ObjectChange() { area = area, layer = layer, index = index, before = before, after = id });
    }

    public void addArea(KnyttWorld world, KnyttArea area)
    {
        record(new AreaAdded() { world = world, area = area });
    }

    private void record(IChange change)
    {
        begin();
        change.redo();
        step.Add(change);
    }

    public bool undo()
    {
        end();
        if (done.Count == 0) { return false; }
        var last = done[done.Count - 1];
        done.RemoveAt(done.Count - 1);
        for (int i = last.Count - 1; i >= 0; i--) { last[i].undo(); }
        undone.Add(last);
        return true;
    }

    public bool redo()
    {
        end();
        if (undone.Count == 0) { return false; }
        var last = undone[undone.Count - 1];
        undone.RemoveAt(undone.Count - 1);
        foreach (var change in last) { change.redo(); }
        done.Add(last);
        return true;
    }

    public void markSaved()
    {
        end();
        saved_at = done.Count;
    }
}
