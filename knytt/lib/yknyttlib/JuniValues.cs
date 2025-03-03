using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YKnyttLib
{
    public class JuniValues
    {
        public enum PowerNames
        {
            Run = 0,
            Climb = 1,
            DoubleJump = 2,
            HighJump = 3,
            Eye = 4,
            EnemyDetector = 5,
            Umbrella = 6,
            Hologram = 7,
            RedKey = 8,
            YellowKey = 9,
            BlueKey = 10,
            PurpleKey = 11,
            Map = 12
        }

        public enum Collectable { None, User, Powerup, Creature, Coin, Artifact, Multiple,
            Key, RedKey, YellowKey, BlueKey, PurpleKey, Door, RedDoor, YellowDoor, BlueDoor, PurpleDoor }
        private static string[] COLLECT_CHAR = { "", "m", "p", "#", "c", "a", "+", 
            "k", "r", "y", "b", "u", "d", "R", "Y", "B", "U" };
        public static Dictionary<string, Collectable> COLLECT_ENUM = new Dictionary<string, Collectable>();
        static JuniValues()
        {
            for (int i = 0; i < COLLECT_CHAR.Length; i++)
            {
                COLLECT_ENUM.Add(COLLECT_CHAR[i], (Collectable)i);
            }
        }

        public bool[] Powers { get; }
        public bool[] Flags { get; }
        public bool[] Collectables { get; private set; }
        public int CoinsSpent { get; set; }
        public BitArray VisitedAreas { get; private set; }
        public Dictionary<KnyttPoint, string> Marked { get; private set; }
        public string Attachment { get; set; }
        public string Character { get; set; }
        public (string, string, string) Tint { get; set; }
        public HashSet<string> Cutscenes { get; private set; }
        public HashSet<string> Endings { get; private set; }
        public int TotalDeaths { get; set; }
        private int current_place_deaths;
        private KnyttPoint respawn_area, respawn_position;
        public int HardestPlaceDeaths { get; set; }
        public string HardestPlace { get; set; }
        public int TotalTime { get; set; }
        public int TotalTimeNow { get { return TotalTime + (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - current_time_start); } }
        private long current_time_start;
        public void adjustPlayingTime(int delay) { current_time_start += delay; }

        private static readonly int VISITED_LIMIT = 160_000_000; // TODO: visited areas don't work on large levels // max save file size ~= 10MB

        public class Flag
        {
            public bool power;
            public bool coin;
            public bool true_flag;
            public int number;

            public static Flag Parse(string str)
            {
                if (str == null) { return null; }
                Flag flag = new Flag();
                if (str.ToLower() == "true") { flag.true_flag = true; return flag; }
                flag.power = str.ToLower().StartsWith("power");
                flag.coin = str.ToLower().StartsWith("coin");
                if (!int.TryParse(flag.power ? str.Substring(5) : flag.coin ? str.Substring(4) : str, out flag.number)) { return null; }
                return flag;
            }
        }

        public JuniValues()
        {
            Powers = new bool[13];
            Flags = new bool[10];
            Collectables = new bool[200];
            Cutscenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Endings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public JuniValues(JuniValues @base, JuniValues failed = null)
        {
            Powers = (bool[])@base.Powers.Clone();
            Flags = (bool[])@base.Flags.Clone();
            Collectables = (bool[])@base.Collectables.Clone();
            CoinsSpent = @base.CoinsSpent;
            Marked = @base.Marked != null ? new Dictionary<KnyttPoint, string>(@base.Marked) : null;
            // same reference for current and for saved visited areas to preserve them between respawns
            VisitedAreas = @base.VisitedAreas; // should already be filled (changeArea executes before writeToSave at start)
            Attachment = @base.Attachment;
            Character = @base.Character;
            Tint = @base.Tint;
            Cutscenes = new HashSet<string>(@base.Cutscenes, StringComparer.OrdinalIgnoreCase);
            Endings = new HashSet<string>(@base.Endings, StringComparer.OrdinalIgnoreCase);
            
            if (failed != null)
            {
                TotalDeaths = failed.TotalDeaths;
                current_place_deaths = failed.current_place_deaths;
                respawn_area = failed.respawn_area;
                respawn_position = failed.respawn_position;
                HardestPlaceDeaths = failed.HardestPlaceDeaths;
                HardestPlace = failed.HardestPlace;
                current_time_start = failed.current_time_start;
                TotalTime = failed.TotalTime;
            }
        }

        public void setPower(int id, bool val) { this.Powers[id] = val; }
        public void setPower(PowerNames name, bool val) { this.Powers[(int)name] = val; }

        public bool getPower(int id) { return this.Powers[id]; }
        public bool getPower(PowerNames name) { return this.Powers[(int)name]; }

        public void setFlag(int index, bool val) { Flags[index] = val; }
        public bool getFlag(int index) { return Flags[index]; }

        public bool check(Flag flag)
        {
            return flag == null ? false :
                   flag.true_flag ? true :
                   flag.power ? Powers[flag.number] : 
                   flag.coin ? Collectables[flag.number + 50] : Flags[flag.number];
        }

        public void setCollectable(int index, bool val) { Collectables[index] = val; }
        public bool getCollectable(int index) { return Collectables[index]; }

        public int getCreaturesCount() { return Collectables.Skip(1).Take(50).Where(a => a).Count(); }
        public int getCoinCount() { return Collectables.Skip(51).Take(100).Where(a => a).Count() - CoinsSpent; }
        public int getArtifactsCount() { return Collectables.Skip(151).Take(49).Where(a => a).Count(); }
        public int getArtifactsCount(int i) { return Collectables.Skip(151 + i * 7).Take(7).Where(a => a).Count(); }

        public void setVisited(KnyttArea area)
        {
            if (area.World.getMapLength() > VISITED_LIMIT) { return; }
            if (area.MapPosition.x < area.World.MinBounds.x || area.MapPosition.x > area.World.MaxBounds.x ||
                area.MapPosition.y < area.World.MinBounds.y || area.MapPosition.y > area.World.MaxBounds.y) { return; }
            if (VisitedAreas == null || VisitedAreas.Length < area.World.getMapLength()) { VisitedAreas = new BitArray(area.World.getMapLength(), false); }
            VisitedAreas.Set(area.World.getMapIndex(area.MapPosition), true);
        }

        public bool isVisited(KnyttArea area)
        {
            if (VisitedAreas == null) { return false; }
            int map_position = area.World.getMapIndex(area.MapPosition);
            if (map_position < 0 || map_position >= VisitedAreas.Length) { return false; }
            return VisitedAreas.Get(map_position);
        }

        public void setMark(KnyttPoint pos, Collectable c)
        {
            if (Marked == null) { Marked = new Dictionary<KnyttPoint, string>(); }
            if (!Marked.ContainsKey(pos) || Marked[pos] == "") { Marked[pos] = COLLECT_CHAR[(int)c]; return; }
            if (Marked[pos].IndexOf(COLLECT_CHAR[(int)c]) != -1) { return; }
            Marked[pos] += COLLECT_CHAR[(int)c];
        }

        public void unsetMark(KnyttPoint pos, Collectable c)
        {
            if (Marked == null || !Marked.ContainsKey(pos)) { return; }
            Marked[pos] = Marked[pos].Replace(COLLECT_CHAR[(int)c], "");
            if (Marked[pos].Length == 0) { Marked.Remove(pos); }
        }

        public bool hasMark(KnyttPoint pos, Collectable c)
        {
            return Marked != null && Marked.ContainsKey(pos) && Marked[pos].Contains(COLLECT_CHAR[(int)c]);
        }

        public void resetMarks(KnyttPoint pos)
        {
            if (Marked == null || !Marked.ContainsKey(pos)) { return; }
            if (Marked[pos].IndexOf(COLLECT_CHAR[(int)Collectable.User]) != -1) { Marked[pos] = COLLECT_CHAR[(int)Collectable.User]; }
            else { Marked.Remove(pos); }
        }

        public void respawn(KnyttPoint new_respawn_area, KnyttPoint new_respawn_position)
        {
            TotalDeaths++;
            if (!(new_respawn_area.Equals(respawn_area) && new_respawn_position.Equals(respawn_position)))
            {
                current_place_deaths = 0;
                respawn_area = new_respawn_area;
                respawn_position = new_respawn_position;
            }
            current_place_deaths++;
            if (current_place_deaths > HardestPlaceDeaths)
            {
                HardestPlace = $"x{respawn_area.x}y{respawn_area.y}:x{respawn_position.x}y{respawn_position.y}";
                HardestPlaceDeaths = current_place_deaths;
            }
        }

        public void writeToSave(KnyttSave save)
        {
            for (int i = 0; i < Powers.Length; i++) { save.setPower(i, Powers[i]); }
            for (int i = 0; i < Flags.Length; i++) { save.setFlag(i, Flags[i]); }
            save.setCollectables(Collectables, CoinsSpent);
            save.VisitedAreas = VisitedAreas;
            save.Marked = Marked;
            save.Attachment = Attachment;
            save.Character = Character;
            save.Tint = Tint;
            save.Cutscenes = Cutscenes;
            save.Endings = Endings;
            save.TotalDeaths = TotalDeaths;
            save.HardestPlaceDeaths = HardestPlaceDeaths;
            save.HardestPlace = HardestPlace;

            TotalTime = TotalTimeNow;
            current_time_start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            save.TotalTime = TotalTime;
            
            save.SourcePowers = new JuniValues(this);
        }

        public void readFromSave(KnyttSave save)
        {
            for (int i = 0; i < Powers.Length; i++) { Powers[i] = save.getPower(i); }
            for (int i = 0; i < Flags.Length; i++) { Flags[i] = save.getFlag(i); }
            save.getCollectables(out var collectables, out var coins_spent);
            Collectables = collectables;
            CoinsSpent = coins_spent;
            VisitedAreas = save.VisitedAreas;
            Marked = save.Marked;
            Attachment = save.Attachment;
            Character = save.Character;
            Tint = save.Tint;
            Cutscenes = save.Cutscenes;
            Endings = save.Endings;
            TotalDeaths = save.TotalDeaths;
            current_place_deaths = 0;
            respawn_area = default;
            respawn_position = default;
            HardestPlaceDeaths = save.HardestPlaceDeaths;
            HardestPlace = save.HardestPlace;
            TotalTime = save.TotalTime;
            current_time_start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
