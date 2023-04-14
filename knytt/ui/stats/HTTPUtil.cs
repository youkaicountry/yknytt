using Godot;
using Godot.Collections;

public partial class HTTPUtil
{
    public static Array jsonArray(Variant obj, string attr)
    {
        var dict = obj.AsGodotDictionary();
        return dict != null && dict.ContainsKey(attr) ? dict[attr].AsGodotArray() : null;
    }

    public static string jsonString(Variant obj, string attr)
    {
        var dict = obj.AsGodotDictionary();
        return dict != null && dict.ContainsKey(attr) ? dict[attr].AsString() : null;
    }

    public static int jsonInt(Variant obj, string attr)
    {
        var dict = obj.AsGodotDictionary();
        return dict != null && dict.ContainsKey(attr) && dict[attr].VariantType == Variant.Type.Float ? dict[attr].AsInt32() : 0;
    }

    public static bool jsonBool(Variant obj, string attr)
    {
        var dict = obj.AsGodotDictionary();
        return dict != null && dict.ContainsKey(attr) && dict[attr].VariantType == Variant.Type.Bool ? dict[attr].AsBool() : false;
    }
}
