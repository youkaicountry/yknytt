using YKnyttLib;

// What a screen has besides its tiles and objects
public enum ScreenValue { Background, TilesetA, TilesetB, Music, AmbianceA, AmbianceB }

public static class ScreenValues
{
    public static readonly ScreenValue[] ALL = (ScreenValue[])System.Enum.GetValues(typeof(ScreenValue));

    // KnyttArea.TilesetB is the first byte, which KS calls A
    public static int get(KnyttArea area, ScreenValue value)
    {
        switch (value)
        {
            case ScreenValue.Background: return area.Background;
            case ScreenValue.TilesetA: return area.TilesetB;
            case ScreenValue.TilesetB: return area.TilesetA;
            case ScreenValue.Music: return area.Song;
            case ScreenValue.AmbianceA: return area.AtmosphereA;
            default: return area.AtmosphereB;
        }
    }

    public static void set(KnyttArea area, ScreenValue value, int number)
    {
        switch (value)
        {
            case ScreenValue.Background: area.Background = number; break;
            case ScreenValue.TilesetA: area.TilesetB = number; break;
            case ScreenValue.TilesetB: area.TilesetA = number; break;
            case ScreenValue.Music: area.Song = number; break;
            case ScreenValue.AmbianceA: area.AtmosphereA = number; break;
            case ScreenValue.AmbianceB: area.AtmosphereB = number; break;
        }
    }
}
