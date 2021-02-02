using Godot.Collections;

public class HTTPUtil
{
    public static T jsonValue<T>(object obj, string attr) where T : class
    {
        return obj is Dictionary dict && dict.Contains(attr) ? dict[attr] as T : null;
    }

    public static int jsonInt(object obj, string attr)
    {
        return obj is Dictionary dict && dict.Contains(attr) && dict[attr] is float ? (int)(float)dict[attr] : 0;
    }

    public static bool jsonBool(object obj, string attr)
    {
        return obj is Dictionary dict && dict.Contains(attr) && dict[attr] is bool ? (bool)dict[attr] : false;
    }
}
