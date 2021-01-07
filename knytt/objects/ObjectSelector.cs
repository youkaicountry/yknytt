using System.Collections.Generic;
using YUtil.Random;
using YKnyttLib;

public class ObjectSelector
{
    private Dictionary<object, List<object>> allObjects = new Dictionary<object, List<object>>();
    private Dictionary<object, int> counters = new Dictionary<object, int>();
    private Dictionary<object, int> selections = new Dictionary<object, int>();
    public bool IsOpen { get; set; }

    private object getKey(GDKnyttBaseObject obj, bool by_type)
    {
        return by_type ? (object)obj.GetType() : obj.ObjectID;
    }

    public void Register(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type))
        {
            allObjects[type] = new List<object>();
            counters[type] = 0;
            selections[type] = -1;
        }
        allObjects[type].Add(obj);
    }

    public void Unregister(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type)) { return; }
        allObjects[type].Remove(obj);
        // TODO: looks not reliable
        counters[type] = 0;
        selections[type] = -1;
    }

    public bool IsObjectSelected(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!IsOpen || !allObjects.ContainsKey(type)) { return false; } // Reset might be called sooner than an object is disposed

        if (counters[type] >= allObjects[type].Count || selections[type] == -1)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.Next(allObjects[type].Count);
        }
        counters[type]++;
        return allObjects[type][selections[type]] == obj;
    }

    public int GetIndex(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type)) { return 0; }
        return allObjects[type].IndexOf(obj);
    }

    public int GetSize(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        return allObjects.ContainsKey(type) ? allObjects[type].Count : 0;
    }

    private Dictionary<object, float> randomValues = new Dictionary<object, float>();

    public float GetRandomValue(GDKnyttBaseObject obj, float maxValue, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!randomValues.ContainsKey(type))
        {
            randomValues.Add(type, GDKnyttDataStore.random.NextFloat(maxValue));
        }
        return randomValues[type];
    }

    public void Reset()
    {
        allObjects.Clear();
        counters.Clear();
        selections.Clear();
        randomValues.Clear();
    }
}
