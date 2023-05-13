using IniParser.Model;
using System.Collections.Generic;

namespace YKnyttLib
{
    public class KnyttWorldInfo
    {
        public string Name { get; }
        public string Author { get; }
        public string Description { get; }
        public string Size { get; }
        public List<string> Difficulties { get; }
        public List<string> Categories { get; }
        public int Format { get; }

        public int Clothes { get; }
        public int Skin { get; }

        public KnyttWorldInfo(KeyDataCollection world_data)
        {
            this.Difficulties = new List<string>();
            this.Categories = new List<string>();

            this.Name = world_data["Name"];
            this.Author = world_data["Author"];
            this.Description = world_data["Description"];
            this.Size = world_data["Size"];
            this.Format = int.Parse(world_data["Format"]);

            this.Clothes = KnyttUtil.parseBGRString(world_data["Clothes"], 0xEFEFEF);
            this.Skin = KnyttUtil.parseBGRString(world_data["Skin"], 0x9CB5D6);

            parseMultiCategory(world_data, "Category", Categories, new string[] { "A", "B" });
            parseMultiCategory(world_data, "Difficulty", Difficulties, new string[] { "A", "B", "C" });
        }

        

        private void parseMultiCategory(KeyDataCollection world_data, string base_name, List<string> target, string[] sub)
        {
            foreach (string s in sub)
            {
                string entry = string.Format("{0} {1}", base_name, s);
                if (!world_data.ContainsKey(entry)) { continue; }
                target.Add(world_data[entry]);
            }
        }
    }
}
