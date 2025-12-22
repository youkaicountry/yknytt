using IniParser.Model;
using IniParser.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace YKnyttLib
{
    public class KnyttSave
    {
        public KnyttWorld World { get; }

        private IniData data;
        public int Slot { get; }

        public JuniValues SourcePowers { get; set; }

        public string SaveFileName => string.Format("{0} {1}.ini", World.WorldDirectoryName, Slot); 

        public KnyttSave(KnyttWorld world, string ini_data, int slot)
        {
            this.World = world;
            this.Slot = slot;
            var parser = new IniDataParser();
            this.data = parser.Parse(ini_data);
            this.setWorldDirectory(World.WorldDirectoryName);
        }

        public bool getPower(int power_id)
        {
            var presult = getValue("Powers", string.Format("Power{0}", power_id));
            return presult != null && int.TryParse(presult, out var p) && p != 0;
        }

        public void setPower(int power_id, bool value)
        {
            setValue("Powers", string.Format("Power{0}", power_id), value ? "1" : "0");
        }

        public void setFlag(int flag_id, bool value)
        {
            setValue("Flags", string.Format("Flag{0}", flag_id), value ? "1" : "0");
        }

        public bool getFlag(int flag_id)
        {
            var fresult = getValue("Flags", string.Format("Flag{0}", flag_id));
            return fresult != null && int.TryParse(fresult, out var f) && f != 0;
        }

        private readonly string[] keyNames = {"Creatures 1", "Creatures 2", "Coins 1", "Coins 2", "Coins 3", "Coins 4", "Artifacts 1", "Artifacts 2"};

        public void setCollectables(bool[] collection, int coinsSpent)
        {
            if (!collection.Contains(true) && coinsSpent == 0) { return; }
            int[] packed = new int[keyNames.Length];
            for (int i = 0; i < collection.Length - 1; i++)
            {
                packed[i / 25] |= (collection[i + 1] ? 1 : 0) << (i % 25);
            }
            packed[6] |= (packed[7] & 0b111) << 25;
            packed[7] >>= 3;
            for (int i = 0; i < keyNames.Length; i++)
            {
                setValue("Extras", keyNames[i], packed[i].ToString());
            }
            // TODO: duplicate code with JuniValues
            setValue("Extras", "Creature Count", collection.Skip(1).Take(50).Where(a => a).Count().ToString());
            setValue("Extras", "Coin Count", (collection.Skip(51).Take(100).Where(a => a).Count() - coinsSpent).ToString());
            setValue("Extras", "Coins Spent", coinsSpent.ToString());
            for (int i = 0; i < 7; i++)
            {
                setValue("Extras", $"Artifact Count {i+1}", collection.Skip(151 + i * 7).Take(7).Where(a => a).Count().ToString());
            }
        }

        public void getCollectables(out bool[] collection, out int coins_spent)
        {
            collection = new bool[200];
            int pos = 0;
            int[] capacity = {25, 25, 25, 25, 25, 25, 28, 21};
            for (int i = 0; i < keyNames.Length; i++)
            {
                int packed = int.Parse(getValue("Extras", keyNames[i]) ?? "0");
                for (int j = 0; j < capacity[i]; j++)
                {
                    collection[++pos] = (packed & (1 << j)) != 0;
                }
            }
            coins_spent = int.TryParse(getValue("Extras", "Coins Spent"), out var c) ? c : 0;
        }

        public static byte[] Compress(byte[] file)
        {
            var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                new MemoryStream(file).CopyTo(gzip);
            }
            var result = output.ToArray();
            return result.Length > file.Length ? file : result;
        }

        public static byte[] Decompress(byte[] file)
        {
            if (file[0] != 31 || file[1] != 139) { return file; }
            var output = new System.IO.MemoryStream();
            using (var gzip = new GZipStream(new System.IO.MemoryStream(file), CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
            }
            return output.ToArray();
        }

        public BitArray VisitedAreas
        {
            set
            {
                if (value == null) { return; }
                if (value.Length < 50_000)
                {
                    Int32[] packed = new Int32[(value.Length - 1) / 32 + 1];
                    value.CopyTo(packed, 0);
                    setValue("Extras", "Visited Areas", String.Join(",", packed));
                }
                else
                {
                    Byte[] packed = new Byte[(value.Length - 1) / 8 + 1];
                    value.CopyTo(packed, 0);
                    setValue("Extras", "Gzipped Visited Areas", Convert.ToBase64String(Compress(packed)));
                    setValue("Extras", "Visited Areas", "");
                }
            }

            get
            {
                var visited_save_gz = getValue("Extras", "Gzipped Visited Areas");
                if (visited_save_gz != null)
                {
                    byte[] packed = Decompress(Convert.FromBase64String(visited_save_gz));
                    return new BitArray(packed);
                }
                else
                {
                    var visited_save = getValue("Extras", "Visited Areas");
                    if (visited_save == null || visited_save == "") { return null; }
                    Int32[] packed = visited_save.Split(',').Select(v => int.Parse(v)).ToArray();
                    return new BitArray(packed);
                }
            }
        }

        public Dictionary<KnyttPoint, string> Marked
        {
            set
            {
                if (value == null || value.Count == 0) { return; }
                setValue("Extras", "Marked", String.Join(",", value.Select(kv => $"{kv.Value}x{kv.Key.x}y{kv.Key.y}")));
            }

            get
            {
                var marked_save = getValue("Extras", "Marked");
                if (marked_save == null || marked_save == "") { return null; }
                var marks = marked_save.Split(',');
                var result = new Dictionary<KnyttPoint, string>(marks.Length);
                foreach (string mark in marks)
                {
                    int xp = mark.IndexOf('x');
                    int yp = mark.IndexOf('y', xp);
                    int x = int.Parse(mark.Substring(xp + 1, yp - xp - 1));
                    int y = int.Parse(mark.Substring(yp + 1));
                    result[new KnyttPoint(x, y)] = mark.Substring(0, xp);
                }
                return result;
            }
        }

        public string Attachment
        {
            get { return getValue("Extras", "Attach"); }
            set { setValue("Extras", "Attach", value, "false"); }
        }

        public string Character
        {
            get { return getValue("Extras", "Character"); }
            set { setValue("Extras", "Character", value?.ToLower() ?? "juni"); }
        }

        public (string, string, string) Tint
        {
            get { return (getValue("Extras", "Tint"), getValue("Extras", "TintInk"), getValue("Extras", "TintTrans")); }
            set
            {
                setValue("Extras", "Tint", value.Item1);
                setValue("Extras", "TintInk", value.Item2);
                setValue("Extras", "TintTrans", value.Item3);
            }
        }

        public HashSet<string> Cutscenes
        {
            get { return getValue("Extras", "Cutscenes")?.Split(',').Where(s => s != "").ToHashSet() ?? new HashSet<string>(); }
            set { setValue("Extras", "Cutscenes", String.Join(",", value), ""); }
        }

        public HashSet<string> Endings
        {
            get { return getValue("Extras", "Endings")?.Split(',').Where(s => s != "").ToHashSet() ?? new HashSet<string>(); }
            set { setValue("Extras", "Endings", String.Join(",", value), ""); }
        }

        public int TotalDeaths
        {
            get { return int.TryParse(getValue("Extras", "Total Deaths"), out int i) ? i : 0; }
            set { setValue("Extras", "Total Deaths", value.ToString()); }
        }

        public int HardestPlaceDeaths
        {
            get { return int.TryParse(getValue("Extras", "Hardest Place Deaths"), out int i) ? i : 0; }
            set { setValue("Extras", "Hardest Place Deaths", value.ToString()); }
        }

        public string HardestPlace
        {
            get { return getValue("Extras", "Hardest Place"); }
            set { setValue("Extras", "Hardest Place", value); }
        }

        public int TotalTime
        {
            get { return int.TryParse(getValue("Extras", "Total Time"), out int i) ? i : 0; }
            set { setValue("Extras", "Total Time", value.ToString()); }
        }

        public bool KSSave => data["Extras"].ContainsKey("Show Edge");
        
        public void RemoveKSMark() { data["Extras"].RemoveKey("Show Edge"); }

        public KnyttPoint getArea()
        {
            return new KnyttPoint(
                int.TryParse(data["Positions"]["X Map"], out var x) ? x : 0, 
                int.TryParse(data["Positions"]["Y Map"], out var y) ? y : 0);
        }

        public void setArea(KnyttPoint location)
        {
            data["Positions"]["X Map"] = location.x.ToString();
            data["Positions"]["Y Map"] = location.y.ToString();
        }

        public KnyttPoint getAreaPosition()
        {
            return new KnyttPoint(
                int.TryParse(data["Positions"]["X Pos"], out var x) ? x : 0, 
                int.TryParse(data["Positions"]["Y Pos"], out var y) ? y : 0);
        }

        public void setAreaPosition(KnyttPoint location)
        {
            data["Positions"]["X Pos"] = location.x.ToString();
            data["Positions"]["Y Pos"] = location.y.ToString();
        }

        public void setWorldDirectory(string dir)
        {
            setValue("World", "World Folder", dir);
        }

        public string getWorldDirectory()
        {
            return getValue("World", "World Folder");
        }

        private string getValue(string section, string key)
        {
            return data[section][key];
        }

        private void setValue(string section, string key, string value, string empty_value = null)
        {
            if (!data.Sections.ContainsSection(section)) { data.Sections.AddSection(section); }
            if (value == empty_value) { data[section].RemoveKey(key); return; }
            data[section][key] = value;
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }
}
