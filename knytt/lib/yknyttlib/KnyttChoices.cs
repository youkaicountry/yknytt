using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YKnyttLib
{
    // What a World.ini value can be, from a level's own files. A choice with no Value is a heading
    public class KnyttChoice
    {
        public string Text { get; }
        public string Value { get; }

        public KnyttChoice(string text, string value = null)
        {
            Text = text;
            Value = value;
        }
    }

    public static class KnyttChoices
    {
        private const string CUSTOM_OBJECTS = "Custom Objects/";
        private static readonly StringComparison NO_CASE = StringComparison.OrdinalIgnoreCase;

        public static readonly string[] POWERS =
            { "Run", "Climb", "Double jump", "High jump", "Eye", "Detector", "Umbrella", "Hologram",
              "Red key", "Yellow key", "Blue key", "Purple key", "Map" };

        // Images in Custom Objects, as World.ini names them: Tree.png
        public static List<string> images(IEnumerable<string> paths) =>
            paths.Where(p => p.StartsWith(CUSTOM_OBJECTS, NO_CASE) && p.EndsWith(".png", NO_CASE) && p.IndexOf('/', CUSTOM_OBJECTS.Length) < 0)
                 .Select(p => p.Substring(CUSTOM_OBJECTS.Length)).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

        // Folders with a Scene1.png, after the two every level has
        public static List<string> cutscenes(IEnumerable<string> paths)
        {
            var found = paths.Where(p => Regex.IsMatch(p, @"^[^/]+/Scene1\.png$", RegexOptions.IgnoreCase))
                             .Select(p => p.Substring(0, p.IndexOf('/')))
                             .Where(n => !n.Equals("Intro", NO_CASE) && !n.Equals("Ending", NO_CASE))
                             .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);
            return new[] { "Intro", "Ending" }.Concat(found).ToList();
        }

        // Characters declared in [Custom Character] (Image1=Name.png), as Shifts and [World] name them: Name
        public static List<string> characters(IniData ini)
        {
            var section = ini.Sections.GetSectionData("Custom Character");
            if (section == null) { return new List<string>(); }
            return section.Keys.Where(k => k.KeyName.StartsWith("Image", NO_CASE) && k.Value.Trim() != "")
                               .Select(k => withoutPng(k.Value.Trim())).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        // Juni, the declared characters, then the other images that could be one
        public static List<KnyttChoice> characterChoices(IniData ini, IEnumerable<string> paths)
        {
            var declared = characters(ini);
            var list = new List<KnyttChoice> { new KnyttChoice("Juni", "Juni") };
            list.AddRange(declared.Select(c => new KnyttChoice(c, c)));
            var others = images(paths).Select(withoutPng).Where(n => !declared.Contains(n, StringComparer.OrdinalIgnoreCase)).ToList();
            if (others.Count > 0)
            {
                list.Add(new KnyttChoice("Not declared yet"));
                list.AddRange(others.Select(c => new KnyttChoice(c, c)));
            }
            return list;
        }

        // Declares a character in [Custom Character] under the next free ImageN. Juni and declared ones change nothing
        public static string declareCharacter(string ini_text, string name)
        {
            if (name == "" || name.Equals("Juni", NO_CASE)) { return ini_text; }
            var ini = KnyttWorld.parseWorldIni(ini_text);
            if (characters(ini).Contains(name, StringComparer.OrdinalIgnoreCase)) { return ini_text; }
            int last = ini.Sections.GetSectionData("Custom Character")?.Keys
                .Select(k => Regex.Match(k.KeyName, @"^Image(\d+)$", RegexOptions.IgnoreCase))
                .Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).DefaultIfEmpty(0).Max() ?? 0;
            return KnyttUtil.setIniValue(ini_text, "Custom Character", $"Image{last + 1}", $"{name}.png");
        }

        // What a shift turns on or off: flags 0-9 and powers
        public static List<KnyttChoice> flags()
        {
            var list = new List<KnyttChoice> { new KnyttChoice("Flags") };
            list.AddRange(Enumerable.Range(0, 10).Select(i => new KnyttChoice($"Flag {i}", $"{i}")));
            list.Add(new KnyttChoice("Powers"));
            list.AddRange(POWERS.Select((p, i) => new KnyttChoice($"{p}  Power{i}", $"Power{i}")));
            return list;
        }

        // When a flag warp works: always, a flag or a power
        public static List<KnyttChoice> warpFlags() => new[] { new KnyttChoice("Always  True", "True") }.Concat(flags()).ToList();

        // Flag warp distances that count an artifact's pieces
        public static List<KnyttChoice> artifacts() =>
            Enumerable.Range(1, 7).Select(i => new KnyttChoice($"Artifact {i}'s count", $"Artifact{i}")).ToList();

        private static string withoutPng(string name) => name.EndsWith(".png", NO_CASE) ? name.Substring(0, name.Length - 4) : name;
    }
}
