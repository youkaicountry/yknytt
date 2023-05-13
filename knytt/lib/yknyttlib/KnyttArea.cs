using IniParser.Model;
using System.IO;
using System.Text;
using System;

namespace YKnyttLib
{
    public class KnyttArea
    {
        public const int AREA_WIDTH = 25;
        public const int AREA_HEIGHT = 10;
        public const int AREA_TILE_LAYERS = 4;
        public const int AREA_SPRITE_LAYERS = 4;

        public KnyttWorld World { get; }

        public KnyttPoint Position { get; protected set; }

        public int TilesetA { get; protected set; }
        public int TilesetB { get; protected set; }

        public int Song { get; protected set; }

        public int AtmosphereA { get; protected set; }
        public int AtmosphereB { get; protected set; }

        public int Background { get; protected set; }

        public KeyDataCollection ExtraData { get; set; }

        public bool Empty { get; private set; }

        public TileLayer[] TileLayers { get; protected set; }
        public ObjectLayer[] ObjectLayers { get; protected set; }

        public KnyttWarp Warp { get; protected set; }
        
        public FlagWarp[] FlagWarps { get; protected set; } = new FlagWarp[4];

        public enum FlagWarpID { A, B, C, All }

        public class FlagWarp
        {
            public JuniValues.Flag flag;
            public int x;
            public int y;
            public bool xArtifactMode;
            public bool yArtifactMode;
        }

        public struct TileLayer
        {
            public byte[] tiles;

            public TileLayer(byte[] tiles)
            {
                this.tiles = tiles;
            }

            public byte getTile(int x, int y)
            {
                return this.tiles[y * AREA_WIDTH + x];
            }
        }

        public struct ObjectLayer
        {
            public byte[] hi;
            public byte[] lo;

            public ObjectLayer(byte[] hi, byte[] lo)
            {
                this.hi = hi;
                this.lo = lo;
            }

            public byte getObjectHi(int x, int y)
            {
                return this.hi[y * AREA_WIDTH + x];
            }

            public byte getObjectLo(int x, int y)
            {
                return this.lo[y * AREA_WIDTH + x];
            }

            public KnyttPoint getObjectID(int x, int y)
            {
                int index = y * AREA_WIDTH + x;
                return new KnyttPoint(this.lo[index], this.hi[index]);
            }
        }

        // Reads the next area in the map stream
        public KnyttArea(Stream map, KnyttWorld world)
        {
            Warp = new KnyttWarp(this);
            this.World = world;
            this.loadFromStream(map);
            this.fetchAreaExtraData();
            this.fetchFlagWarpData();
        }

        public KnyttArea(KnyttPoint position, KnyttWorld world)
        {
            Warp = new KnyttWarp(this);
            this.World = world;
            this.Empty = true;
            this.Position = position;
            this.fetchAreaExtraData();
            this.fetchFlagWarpData();
        }

        private void fetchAreaExtraData()
        {
            string index = string.Format("x{0}y{1}", Position.x, Position.y);
            if (!this.World.INIData.Sections.ContainsSection(index)) { return; }
            this.ExtraData = World.INIData[index];
        }

        public string getExtraData(string key)
        {
            if (ExtraData == null) { return null; }
            if (!ExtraData.ContainsKey(key)) { return null; }
            return ExtraData[key];
        }

        private void fetchFlagWarpData()
        {
            if (ExtraData == null) { return; }
            foreach (var id in Enum.GetValues(typeof(FlagWarpID)))
            {
                var flag_key = $"Flag({id})";
                if (ExtraData.ContainsKey(flag_key))
                {
                    var x_key = $"FlagWarpX({id})";
                    var y_key = $"FlagWarpY({id})";

                    FlagWarps[(int)id] = new FlagWarp();
                    FlagWarps[(int)id].flag = JuniValues.Flag.Parse(ExtraData[flag_key]);
                    
                    int x, y;
                    FlagWarps[(int)id].xArtifactMode = ExtraData.ContainsKey(x_key) && ExtraData[x_key].ToLower().StartsWith("artifact");
                    FlagWarps[(int)id].x = !ExtraData.ContainsKey(x_key) ? 0 :
                        FlagWarps[(int)id].xArtifactMode ? 
                            (int.TryParse(ExtraData[x_key].Substring("artifact".Length), out x) ? x : 0) :
                            (int.TryParse(ExtraData[x_key], out x) ? x : 0);

                    FlagWarps[(int)id].yArtifactMode = ExtraData.ContainsKey(y_key) && ExtraData[y_key].ToLower().StartsWith("artifact");
                    FlagWarps[(int)id].y = !ExtraData.ContainsKey(y_key) ? 0 :
                        FlagWarps[(int)id].yArtifactMode ? 
                            (int.TryParse(ExtraData[y_key].Substring("artifact".Length), out y) ? y : 0) :
                            (int.TryParse(ExtraData[y_key], out y) ? y : 0);
                }
            }
        }

        private void loadFromStream(Stream map)
        {
            this.Empty = false;

            this.parseSize(map);

            // Skip misc header stuff
            // TODO: Verify it
            skipBytes(map, 4);

            // Parse layer data
            this.parseTileLayers(map);
            this.parseSpriteLayers(map);

            // Parse tilesets
            this.TilesetB = map.ReadByte();
            this.TilesetA = map.ReadByte();

            // Parse atmosphere
            this.AtmosphereA = map.ReadByte();
            this.AtmosphereB = map.ReadByte();
            this.Song = map.ReadByte();
            this.Background = map.ReadByte();

            //Console.WriteLine(string.Format("Pos: ({0}, {1}), T: ({2}, {3})", this.Position.x, this.Position.y, this.TilesetB, this.TilesetA));
        }

        private void parseSize(Stream map)
        {
            this.Position = new KnyttPoint(parseNumber(map), parseNumber(map));
        }

        private void parseTileLayers(Stream map)
        {
            this.TileLayers = new TileLayer[AREA_TILE_LAYERS];

            for (int i = 0; i < AREA_TILE_LAYERS; i++)
            {
                byte[] tiles = parseByteArray(map, AREA_WIDTH * AREA_HEIGHT);
                this.TileLayers[i] = new TileLayer(tiles);
            }
        }

        private void parseSpriteLayers(Stream map)
        {
            this.ObjectLayers = new ObjectLayer[AREA_SPRITE_LAYERS];

            for (int i = 0; i < AREA_SPRITE_LAYERS; i++)
            {
                byte[] hi = parseByteArray(map, AREA_WIDTH * AREA_HEIGHT);
                byte[] lo = parseByteArray(map, AREA_WIDTH * AREA_HEIGHT);
                this.ObjectLayers[i] = new ObjectLayer(hi, lo);
            }
        }

        private static byte[] parseByteArray(Stream map, int size)
        {
            byte[] data = new byte[size];
            map.Read(data, 0, size);
            return data;
        }

        private static int parseNumber(Stream map)
        {
            var sb = new StringBuilder();
            char c = (char)map.ReadByte();
            while (c >= 48 && c < 58)
            {
                sb.Append(c);
                c = (char)map.ReadByte();
            }

            return int.Parse(sb.ToString());
        }

        private static void skipBytes(Stream map, int size)
        {
            for (int i = 0; i < size; i++)
            {
                map.ReadByte();
            }
        }

        public override string ToString()
        {
            return string.Format("[Area {0}]", this.Position);
        }
    }
}
