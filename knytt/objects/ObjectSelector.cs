using System.Collections.Generic;
using YUtil.Random;
using YKnyttLib;

public class ObjectSelector
{
    private Dictionary<KnyttPoint, List<object>> allObjects = new Dictionary<KnyttPoint, List<object>>();
    private Dictionary<KnyttPoint, int> counters = new Dictionary<KnyttPoint, int>();
    private Dictionary<KnyttPoint, int> selections = new Dictionary<KnyttPoint, int>();

    public void Register(GDKnyttBaseObject obj)
    {
        var type = obj.ObjectID;
        if (!allObjects.ContainsKey(type))
        {
            allObjects[type] = new List<object>();
            counters[type] = 0;
            selections[type] = -1;
        }
        allObjects[type].Add(obj);
    }

    public void Unregister(GDKnyttBaseObject obj)
    {
        var type = obj.ObjectID;
        if (!allObjects.ContainsKey(type)) { return; }
        allObjects[type].Remove(obj);
        // TODO: looks not reliable
        counters[type] = 0;
        selections[type] = -1;
    }

    public bool IsObjectSelected(GDKnyttBaseObject obj)
    {
        var type = obj.ObjectID;
        if (!allObjects.ContainsKey(type)) { return false; } // Reset might be called sooner than an object is disposed

        if (counters[type] >= allObjects[type].Count || selections[type] == -1)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.Next(allObjects[type].Count);
        }
        counters[type]++;
        return allObjects[type][selections[type]] == obj;
    }

    private Dictionary<KnyttPoint, float> randomValues = new Dictionary<KnyttPoint, float>();

    public float GetRandomValue(GDKnyttBaseObject obj, float maxValue)
    {
        var type = obj.ObjectID;
        if (!randomValues.ContainsKey(type))
        {
            randomValues.Add(type, GDKnyttDataStore.random.NextFloat(maxValue));
        }
        return randomValues[type];
    }

    public int GetIndex(GDKnyttBaseObject obj)
    {
        var type = obj.ObjectID;
        if (!allObjects.ContainsKey(type)) { return 0; }
        return allObjects[type].IndexOf(obj);
    }

    public void Reset()
    {
        allObjects.Clear();
        counters.Clear();
        selections.Clear();
        randomValues.Clear();
    }
}
