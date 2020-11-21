using Godot;
using System;
using System.Collections.Generic;

public class ObjectSelector
{
    private Dictionary<Type, List<object>> allObjects = new Dictionary<Type, List<object>>();
    private Dictionary<Type, int> counters = new Dictionary<Type, int>();
    private Dictionary<Type, int> selections = new Dictionary<Type, int>();

    public void Register(object obj)
    {
        var type = obj.GetType();
        if (!allObjects.ContainsKey(type))
        {
            allObjects[type] = new List<object>();
            counters[type] = 0;
            selections[type] = -1;
        }
        allObjects[type].Add(obj);
    }

    public void Unregister(object obj)
    {
        var type = obj.GetType();
        if (!allObjects.ContainsKey(type)) { return; }
        allObjects[type].Remove(obj);
        // TODO: looks not reliable
        counters[type] = 0;
        selections[type] = -1;
    }

    public bool IsObjectSelected(object obj)
    {
        var type = obj.GetType();
        if (!allObjects.ContainsKey(type)) { return false; } // Reset might be called sooner than an object is disposed

        if (counters[type] >= allObjects[type].Count || selections[type] == -1)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.Next(allObjects[type].Count);
        }
        counters[type]++;
        return allObjects[type][selections[type]] == obj;
    }

    public void Reset()
    {
        allObjects.Clear();
        counters.Clear();
        selections.Clear();
    }
}
