using IniParser.Model;
using IniParser.Parser;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace YKnyttLib
{
    public abstract class KnyttWorld
    {
        public KnyttWorldInfo Info { get; private set; }

        public KnyttPoint MinBounds { get; protected set; }
        public KnyttPoint MaxBounds { get; protected set; }

        public KnyttPoint Size { get; protected set; }

        public List<KnyttArea> Areas { get; protected set; }
        public Dictionary<KnyttPoint, KnyttArea> Map { get; protected set; }

        public IniData INIData { get; private set; }

        public string WorldDirectory { get; private set; } // Full path

        string _dir_name;
        public string WorldDirectoryName // Formatted name
        { 
            get { return _dir_name; } 
            
            private set
            {
                if (_save != null) { _save.setWorldDirectory(value); }
                _dir_name = value;
            }
        }

        public bool BinMode { get; protected set; }
        public KnyttBinWorldLoader BinLoader { get; protected set;}

        KnyttSave _save;
        public KnyttSave CurrentSave
        {
            get { return _save; }

            set
            {
                value.setWorldDirectory(WorldDirectoryName);
                _save = value;
            }
        }

        public KnyttWorld()
        {
            this.MinBounds = new KnyttPoint(int.MaxValue, int.MaxValue);
            this.MaxBounds = new KnyttPoint(int.MinValue, int.MinValue);
        }

        public void loadWorldConfig(string world_ini)
        {
            var parser = new IniDataParser();
            parser.Configuration.AllowDuplicateKeys = true;
            parser.Configuration.OverrideDuplicateKeys = true;
            parser.Configuration.CaseInsensitive = true;
            parser.Configuration.AllowDuplicateSections = true;
            parser.Configuration.SkipInvalidLines = true;
            this.INIData = parser.Parse(world_ini);
            this.Info = new KnyttWorldInfo(INIData["World"]);
        }

        public void setBinMode(KnyttBinWorldLoader loader)
        {
            this.BinMode = true;
            if (loader == null) { return; }
            this.BinLoader = loader;
        }

        public void purgeBinFile()
        {
            this.BinLoader = null;
        }

        public void loadWorldMap(Stream map)
        {
            GZipStream gz = new GZipStream(map, CompressionMode.Decompress);
            int start_byte = readMapHeader(gz, null);

            this.Map = new Dictionary<KnyttPoint, KnyttArea>();

            // Area definition starts with an 'x' character
            while (start_byte == 'x')
            {
                var area = new KnyttArea(gz, this);
                if (area.Position.x == int.MinValue || area.Position.y == int.MinValue) { start_byte = gz.ReadByte(); continue; }

                this.Map.Add(area.Position, area);

                this.MinBounds = this.MinBounds.min(area.Position);
                this.MaxBounds = this.MaxBounds.max(area.Position);

                start_byte = gz.ReadByte();
            }

            // Set the map
            this.Size = new KnyttPoint((MaxBounds.x - MinBounds.x) + 1, (MaxBounds.y - MinBounds.y) + 1);
        }

        // Reads what comes before the first area, copying it if asked. Returns the first byte of the first area.
        private static int readMapHeader(Stream map, Stream copy)
        {
            // Beginning of a map might be empty
            int start_byte = map.ReadByte();
            if (start_byte == 0)
            {
                var size = new byte[4];
                for (int i = 0; i < 4; i++) { size[i] = (byte)map.ReadByte(); }
                uint offset = size[0] + ((uint)size[1] << 8) + ((uint)size[2] << 16) + ((uint)size[3] << 24);
                if (copy == null) { KnyttArea.skipBytes(map, offset); }
                else
                {
                    copy.WriteByte(0);
                    copy.Write(size, 0, 4);
                    copyBytes(map, copy, offset);
                }
                start_byte = map.ReadByte();
            }

            while (start_byte != 'x' && start_byte != -1)
            {
                copy?.WriteByte((byte)start_byte);
                start_byte = map.ReadByte();
            }
            return start_byte;
        }

        private static void copyBytes(Stream from, Stream to, uint count)
        {
            var buffer = new byte[65536];
            while (count > 0)
            {
                int read = from.Read(buffer, 0, (int)System.Math.Min(count, (uint)buffer.Length));
                if (read == 0) { return; }
                to.Write(buffer, 0, read);
                count -= (uint)read;
            }
        }

        public byte[] saveWorldMap()
        {
            var old_map = getWorldData("Map.bin");
            using (var stream = new MemoryStream())
            {
                using (var gz = new GZipStream(stream, CompressionLevel.Optimal, leaveOpen: true))
                {
                    if (old_map != null) { readMapHeader(new GZipStream(new MemoryStream(old_map), CompressionMode.Decompress), gz); }
                    foreach (var area in Map.Values) { area.writeToStream(gz); }
                }
                return stream.ToArray();
            }
        }

        public void addArea(KnyttArea area)
        {
            Map[area.Position] = area;
            updateBounds();
        }

        public void removeArea(KnyttPoint position)
        {
            Map.Remove(position);
            updateBounds();
        }

        private void updateBounds()
        {
            MinBounds = new KnyttPoint(int.MaxValue, int.MaxValue);
            MaxBounds = new KnyttPoint(int.MinValue, int.MinValue);
            foreach (var position in Map.Keys)
            {
                MinBounds = MinBounds.min(position);
                MaxBounds = MaxBounds.max(position);
            }
            Size = new KnyttPoint((MaxBounds.x - MinBounds.x) + 1, (MaxBounds.y - MinBounds.y) + 1);
        }

        public byte[] getWorldData(string filepath)
        {
            byte[] data;
            // If in bin mode, load from the bin loader
            if (BinMode) { data = this.BinLoader.GetFile(filepath); }
            else { data = getExternalWorldData(filepath); }

            return data == null ? getSystemWorldData(filepath) : data;
        }

        protected abstract byte[] getExternalWorldData(string filepath);
        protected abstract byte[] getSystemWorldData(string filepath);

        public bool worldFileExists(string filepath)
        {
            if (BinMode) 
            { 
                var data = this.BinLoader.GetFile(filepath);
                return data != null;
            }

            return externalFileExists(filepath);
        }

        protected abstract bool externalFileExists(string filepath);

        public object getWorldTexture(string filepath)
        {
            if (BinMode) 
            { 
                var data = BinLoader.GetFile(filepath);
                if (data != null) { return bytesToTexture(data); }
            } 
            else 
            { 
                var t = getExternalTexture(filepath);
                if (t != null) { return t; }
            }
            return getSystemTexture(filepath);
        }

        protected abstract object bytesToTexture(byte[] data);
        protected abstract object getExternalTexture(string filepath);
        protected abstract object getSystemTexture(string filepath);

        public object getWorldSound(string filepath, bool loop)
        {
            if (BinMode)
            {
                var data = BinLoader.GetFile(filepath);
                if (data != null) { return bytesToSound(data, loop); }
            }
            else
            {
                var t = getExternalSound(filepath, loop);
                if (t != null) { return t; }
            }
            return getSystemSound(filepath, loop);
        }

        protected abstract object bytesToSound(byte[] data, bool loop);
        protected abstract object getExternalSound(string filepath, bool loop);
        protected abstract object getSystemSound(string filepath, bool loop);


        public KnyttArea getArea(KnyttPoint coords)
        {
            return Map.ContainsKey(coords) ? Map[coords] : 
                INIData.Sections.ContainsSection($"x{coords.x}y{coords.y}") ? new KnyttArea(coords, this) : null;
        }

        public int getMapIndex(KnyttPoint coords)
        {
            return (coords.y - MinBounds.y) * Size.x + (coords.x - MinBounds.x);
        }

        public int getMapLength() => Size.Area;

        public void setDirectory(string full_dir, string dir_name)
        {
            this.WorldDirectory = full_dir;
            this.WorldDirectoryName = dir_name;
        }
    }
}
