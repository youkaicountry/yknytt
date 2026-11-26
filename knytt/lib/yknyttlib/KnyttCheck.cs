using System.Collections.Generic;
using System.Linq;

namespace YKnyttLib
{
    // Where a level's files are found: in the level, or in the game's own data
    public interface KnyttFiles
    {
        bool level(string path);
        bool game(string path);
        // A level image's width and height, null when it isn't a PNG
        (int width, int height)? size(string path);
    }

    // Something wrong with a level, and the screens it's on (none when it's the level's own)
    public class KnyttProblem
    {
        public string Text { get; }
        public List<KnyttPoint> Screens { get; } = new List<KnyttPoint>();
        // The [World] key that fixes it, or null
        public string Key { get; }

        public KnyttProblem(string text, string key = null)
        {
            Text = text;
            Key = key;
        }
    }

    // What a level needs (KS+ checks the same): each screen's music, ambiance, background and tilesets are there and the right size,
    // the level's file is named Author - Name, and the level list can file it
    public static class KnyttCheck
    {
        public const int GRADIENT_HEIGHT = 240, TILESET_WIDTH = 384, TILESET_HEIGHT = 192;

        // What a screen uses. Tileset A is KnyttArea.TilesetB: it holds tiles 0-127
        public struct Screen
        {
            public KnyttPoint Position;
            public int Song, AmbianceA, AmbianceB, Background, TilesetA, TilesetB;

            public static Screen of(KnyttArea area) => new Screen
            {
                Position = area.Position, Song = area.Song, AmbianceA = area.AtmosphereA, AmbianceB = area.AtmosphereB,
                Background = area.Background, TilesetA = area.TilesetB, TilesetB = area.TilesetA
            };
        }

        // One screen's problems. 0 is no music or ambiance; tilesets and backgrounds start at 0
        public static List<string> screen(Screen s, KnyttFiles files)
        {
            var problems = new List<string>();
            if (s.Song != 0 && !found($"Music/Song{s.Song}.ogg", files)) { problems.Add($"Music {s.Song} not found"); }
            if (s.AmbianceA != 0 && !found($"Ambiance/Ambi{s.AmbianceA}.ogg", files)) { problems.Add($"Ambiance A {s.AmbianceA} not found"); }
            if (s.AmbianceB != 0 && !found($"Ambiance/Ambi{s.AmbianceB}.ogg", files)) { problems.Add($"Ambiance B {s.AmbianceB} not found"); }

            string background = image($"Gradients/Gradient{s.Background}.png", files, (w, h) => h == GRADIENT_HEIGHT,
                                      (w, h) => $"is {h} tall, the game draws {GRADIENT_HEIGHT}");
            if (background != null) { problems.Add($"Background {s.Background} {background}"); }
            foreach (var (letter, num) in new[] { ("A", s.TilesetA), ("B", s.TilesetB) })
            {
                string tileset = image($"Tilesets/Tileset{num}.png", files, (w, h) => w == TILESET_WIDTH && h >= TILESET_HEIGHT,
                                       (w, h) => $"is {w}x{h}, tilesets are {TILESET_WIDTH}x{TILESET_HEIGHT}");
                if (tileset != null) { problems.Add($"Tileset {letter} {num} {tileset}"); }
            }
            return problems;
        }

        // The level's own problems. file_name: its file or folder without .knytt.bin, null when it has none yet
        public static List<KnyttProblem> level(KnyttWorldInfo info, string file_name)
        {
            var problems = new List<KnyttProblem>();
            string name = $"{info.Author} - {info.Name}";
            if (file_name != null && file_name != name) { problems.Add(new KnyttProblem($"Its file is \"{file_name}\", World.ini makes it \"{name}\"", "Name")); }
            if (info.Categories.Count == 0) { problems.Add(new KnyttProblem("No category", "Category A")); }
            if (info.Difficulties.Count == 0) { problems.Add(new KnyttProblem("No difficulty", "Difficulty A")); }
            if (string.IsNullOrWhiteSpace(info.Size)) { problems.Add(new KnyttProblem("No size", "Size")); }
            return problems;
        }

        // Everything: the level's own first, then each screen problem with the screens it's on, most screens first
        public static List<KnyttProblem> all(KnyttWorldInfo info, string file_name, IEnumerable<Screen> screens, KnyttFiles files)
        {
            var found = new Dictionary<string, KnyttProblem>();
            foreach (var s in screens.OrderBy(s => s.Position.y).ThenBy(s => s.Position.x))
            {
                foreach (string text in screen(s, files))
                {
                    if (!found.ContainsKey(text)) { found[text] = new KnyttProblem(text); }
                    found[text].Screens.Add(s.Position);
                }
            }
            return level(info, file_name).Concat(found.Values.OrderByDescending(p => p.Screens.Count)).ToList();
        }

        // The screen after here that has a problem, in reading order and round again; null when only here has it
        public static KnyttPoint? nextScreen(List<KnyttPoint> screens, KnyttPoint here)
        {
            var others = screens.Where(s => !s.Equals(here)).ToList();
            if (others.Count == 0) { return null; }
            bool after(KnyttPoint s) => s.y > here.y || (s.y == here.y && s.x > here.x);
            return others.Where(after).Cast<KnyttPoint?>().FirstOrDefault() ?? others[0];
        }

        // A PNG's width and height from its first 24 bytes, null when it isn't one
        public static (int width, int height)? pngSize(byte[] data)
        {
            if (data == null || data.Length < 24 || data[0] != 0x89 || data[1] != 'P' || data[2] != 'N' || data[3] != 'G') { return null; }
            int number(int at) => (data[at] << 24) | (data[at + 1] << 16) | (data[at + 2] << 8) | data[at + 3];
            return (number(16), number(20));
        }

        private static bool found(string path, KnyttFiles files) => files.level(path) || files.game(path);

        // Why an image is wrong: not found, or the level's own is the wrong size. The game's own are always right
        private static string image(string path, KnyttFiles files, System.Func<int, int, bool> fits, System.Func<int, int, string> why)
        {
            if (files.level(path))
            {
                var size = files.size(path);
                return size == null ? "isn't a PNG" : fits(size.Value.width, size.Value.height) ? null : why(size.Value.width, size.Value.height);
            }
            return files.game(path) ? null : "not found";
        }
    }
}
