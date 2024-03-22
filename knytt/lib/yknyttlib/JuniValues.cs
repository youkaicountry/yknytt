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

        public enum Collectable { None, User, Powerup, Key, Door, Creature, Coin, Artifact, Multiple }
        private static string[] COLLECT_CHAR = { "", "m", "p", "k", "d", "#", "c", "a", "+" };
        public static Dictionary<string, Collectable> COLLECT_ENUM = new Dictionary<string, Collectable>() {
            [""] = Collectable.None, ["m"] = Collectable.User, ["+"] = Collectable.Multiple, ["p"] = Collectable.Powerup,
            ["k"] = Collectable.Key, ["d"] = Collectable.Door, ["#"] = Collectable.Creature, ["c"] = Collectable.Coin,
            ["a"] = Collectable.Artifact};

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

        public class Flag
        {
            public bool power;
            public int number;
            public bool true_flag;

            public static Flag Parse(string str)
            {
                if (str == null) { return null; }
                Flag flag = new Flag();
                if (str.ToLower() == "true") { flag.true_flag = true; return flag; }
                flag.power = str.ToLower().StartsWith("power");
                if (!int.TryParse(flag.power ? str.Substring(5) : str, out flag.number)) { return null; }
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

        public JuniValues(JuniValues src)
        {
            Powers = (bool[])src.Powers.Clone();
            Flags = (bool[])src.Flags.Clone();
            Collectables = (bool[])src.Collectables.Clone();
            CoinsSpent = src.CoinsSpent;
            VisitedAreas = src.VisitedAreas != null ? (BitArray)src.VisitedAreas.Clone() : null;
            Marked = src.Marked != null ? new Dictionary<KnyttPoint, string>(src.Marked) : null;
            Attachment = src.Attachment;
            Character = src.Character;
            Tint = src.Tint;
            Cutscenes = new HashSet<string>(src.Cutscenes, StringComparer.OrdinalIgnoreCase);
            Endings = new HashSet<string>(src.Endings, StringComparer.OrdinalIgnoreCase);
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
                   flag.power ? Powers[flag.number] : Flags[flag.number];
        }

        public void setCollectable(int index, bool val) { Collectables[index] = val; }
        public bool getCollectable(int index) { return Collectables[index]; }

        public int getCreaturesCount() { return Collectables.Skip(1).Take(50).Where(a => a).Count(); }
        public int getCoinCount() { return Collectables.Skip(51).Take(100).Where(a => a).Count() - CoinsSpent; }
        public int getArtifactsCount() { return Collectables.Skip(151).Take(49).Where(a => a).Count(); }
        public int getArtifactsCount(int i) { return Collectables.Skip(151 + i * 7).Take(7).Where(a => a).Count(); }

        public void setVisited(KnyttArea area)
        {
            if (area.Position.x < area.World.MinBounds.x || area.Position.x > area.World.MaxBounds.x ||
                area.Position.y < area.World.MinBounds.y || area.Position.y > area.World.MaxBounds.y) { return; }
            if (VisitedAreas == null) { VisitedAreas = new BitArray(area.World.Map.Length, false); }
            VisitedAreas.Set(area.World.getMapIndex(area.Position), true);
        }

        public bool isVisited(KnyttArea area)
        {
            return VisitedAreas.Get(area.World.getMapIndex(area.Position));
        }

        public void setMark(KnyttPoint pos, Collectable c)
        {
            if (Marked == null) { Marked = new Dictionary<KnyttPoint, string>(); }

            if (!Marked.ContainsKey(pos) || Marked[pos] == "") { Marked[pos] = COLLECT_CHAR[(int)c]; return; }
            if (Marked[pos].IndexOf(COLLECT_CHAR[(int)c]) != -1) { return; }
            
            Collectable mark1 = COLLECT_ENUM[Marked[pos][0].ToString()];
            Collectable mark2 = COLLECT_ENUM[Marked[pos].Substring(1)];
            if (mark2 == Collectable.None) { mark2 = c; }
            else
            {
                mark1 = c < mark1 && c < mark2 ? c : mark1 < mark2 ? mark1 : mark2;
                mark2 = Collectable.Multiple;
            }
            Marked[pos] = COLLECT_CHAR[(int)mark1] + COLLECT_CHAR[(int)mark2];
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
            Collectable mark1 = COLLECT_ENUM[Marked[pos][0].ToString()];
            Collectable mark2 = COLLECT_ENUM[Marked[pos].Substring(1)];
            if (mark1 == Collectable.User || mark2 == Collectable.User) { Marked[pos] = COLLECT_CHAR[(int)Collectable.User]; }
            else { Marked.Remove(pos); }
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
        }
    }
}
