using IniParser.Model;
using System;
using System.Linq;

namespace YKnyttLib
{
    public class KnyttWarp
    {
        // TODO: These should be structs with flag info

        public KnyttPoint WarpUp { get; private set; }
        public KnyttPoint WarpDown { get; private set; }
        public KnyttPoint WarpLeft { get; private set; }
        public KnyttPoint WarpRight { get; private set; }

        public bool LoadedWarp { get; private set; }

        public KnyttArea Area { get; }

        public KnyttWarp(KnyttArea area)
        {
            Area = area;
            WarpUp = KnyttPoint.Zero;
            WarpDown = KnyttPoint.Zero;
            WarpLeft = KnyttPoint.Zero;
            WarpRight = KnyttPoint.Zero;
            LoadedWarp = false;
        }

        public void loadWarpData()
        {
            if (LoadedWarp) { return; }
            LoadedWarp = true;

            var data = Area.ExtraData;
            WarpUp = getINIWarp(data, "U");
            WarpDown = getINIWarp(data, "D");
            WarpLeft = getINIWarp(data, "L");
            WarpRight = getINIWarp(data, "R");
        }

        public KnyttPoint getWarpCoords(KnyttPoint new_area, KnyttPoint current_area)
        {
            if (new_area.X < current_area.X) { return WarpLeft; }
            if (new_area.X > current_area.X) { return WarpRight; }
            if (new_area.Y > current_area.Y) { return WarpDown; }
            if (new_area.Y < current_area.Y) { return WarpUp; }
            throw (new SystemException("Invalid warp!"));
        }

        private KnyttPoint getINIWarp(KeyDataCollection data, string dir)
        {
            return new KnyttPoint(getDirWarp(data, "X", dir), getDirWarp(data, "Y", dir));
        }

        private int getDirWarp(KeyDataCollection data, string axis, string dir)
        {
            string key = string.Format("Warp{0}({1})", axis, dir);
            if (!data.ContainsKey(key)) { return 0; }
            return KnyttUtil.parseIniInt(data[key]) ?? 0;
        }
    }
}
