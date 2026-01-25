using System.Collections.Generic;
using YUtil.Random;

public class ObjectSelector
{
    private Dictionary<object, List<object>> allObjects = new Dictionary<object, List<object>>();
    private Dictionary<object, int> counters = new Dictionary<object, int>();
    private Dictionary<object, object> selections = new Dictionary<object, object>();
    public bool IsOpen { get; set; } // possibly obsolete, area deactivation now is immediate, without any timer

    private object getKey(object obj, bool by_type)
    {
        return by_type ? (object)obj.GetType() : (obj as GDKnyttBaseObject).ObjectID;
    }

    // Do not call Register and Unregister in IsObjectSelected series!
    public void Register(object obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type))
        {
            allObjects[type] = new List<object>();
            counters[type] = 0;
            selections[type] = null;
        }
        if (!allObjects[type].Contains(obj)) { allObjects[type].Add(obj); }
    }

    public void Unregister(object obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type)) { return; }
        allObjects[type].Remove(obj);
        counters[type] = 0;
        selections[type] = null;
    }

    public bool IsObjectSelected(object obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        // Reset might be called sooner than an object is disposed, or object may be not registered
        if (!IsOpen || !allObjects.ContainsKey(type) || !allObjects[type].Contains(obj)) { return false; }

        if (selections[type] == null)
        {
            counters[type] = 0;
            selections[type] = GDKnyttDataStore.random.NextElement(allObjects[type]);
        }

        bool is_selected = selections[type] == obj;

        counters[type]++;
        if (counters[type] >= allObjects[type].Count)
        {
            selections[type] = null;
        }

        return is_selected;
    }

    public int GetIndex(object obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        if (!allObjects.ContainsKey(type)) { return 0; }
        return allObjects[type].IndexOf(obj);
    }

    public int GetSize(object obj, bool by_type = false)
    {
        var type = getKey(obj, by_type);
        return allObjects.ContainsKey(type) ? allObjects[type].Count : 0;
    }

    private Dictionary<object, float> randomValues = new Dictionary<object, float>();

    public float GetRandomValue(object obj, float maxValue, bool by_type = false)
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
