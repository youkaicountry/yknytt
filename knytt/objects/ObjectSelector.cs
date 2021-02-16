using System.Collections.Generic;
using YUtil.Random;

public class ObjectSelector
{
    private Dictionary<object, List<object>> allObjects = new Dictionary<object, List<object>>();
    private Dictionary<object, int> counters = new Dictionary<object, int>();
    private Dictionary<object, object> selections = new Dictionary<object, object>();
    public bool IsOpen { get; set; }

    private object getKey(GDKnyttBaseObject obj, bool by_type)
    {
        return by_type ? (object)obj.GetType() : obj.ObjectID;
    }

    // Do not call Register and Unregister in IsObjectSelected series!
    public void Register(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type))
        {
            allObjects[type] = new List<object>();
            counters[type] = 0;
            selections[type] = null;
        }
        allObjects[type].Add(obj);
    }

    public void Unregister(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type)) { return; }
        allObjects[type].Remove(obj);
        counters[type] = 0;
        selections[type] = null;
    }

    public bool IsObjectSelected(GDKnyttBaseObject obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        // Reset might be called sooner than an object is disposed, or object may be not registered
        if (!IsOpen || !allObjects.ContainsKey(type) || !allObjects[type].Contains(obj)) { return false; }

        if (selections[type] == null)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.NextElement(allObjects[type]);
        }

        counters[type]++;
        bool is_selected = selections[type] == obj;

        if (counters[type] >= allObjects[type].Count)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.NextElement(allObjects[type]);
        }

        return is_selected;
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
