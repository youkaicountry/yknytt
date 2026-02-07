using Godot;
using Godot.Collections;

public partial class HTTPUtil
{
    public static T jsonValue<T>(object obj, string attr) where T : class
    {
        if (obj is Dictionary dict && dict.ContainsKey(attr))
        {
            Variant val = dict[attr];
            if (typeof(T) == typeof(string)) return val.AsString() as T;
            if (typeof(T) == typeof(Godot.Collections.Array)) return val.AsGodotArray() as T;
            return null;
        }
        return null;
    }

    public static int jsonInt(object obj, string attr)
    {
        return obj is Dictionary dict && dict.ContainsKey(attr) ? dict[attr].AsInt32() : 0;
    }

    public static bool jsonBool(object obj, string attr)
    {
        return obj is Dictionary dict && dict.ContainsKey(attr) && dict[attr].AsBool();
    }

    public static float jsonFloat(object obj, string attr)
    {
        return obj is Dictionary dict && dict.ContainsKey(attr) ? (float)dict[attr].AsDouble() : 0;
    }
}
