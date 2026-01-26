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

            // Beginning of a map might be empty
            int start_byte = gz.ReadByte();
            if (start_byte == 0)
            {
                uint offset = (uint)gz.ReadByte() + ((uint)gz.ReadByte() << 8) + ((uint)gz.ReadByte() << 16) + ((uint)gz.ReadByte() << 24);
                KnyttArea.skipBytes(gz, offset);
                start_byte = gz.ReadByte();
            }

            while (start_byte != 'x' && start_byte != -1) { start_byte = gz.ReadByte(); }

            this.Map = new Dictionary<KnyttPoint, KnyttArea>();

            // Area definition starts with an 'x' character
            while (start_byte == 'x')
            {
                var area = new KnyttArea(gz, this);
                if (area.Position.X == int.MinValue || area.Position.Y == int.MinValue) { start_byte = gz.ReadByte(); continue; }

                this.Map.Add(area.Position, area);

                this.MinBounds = this.MinBounds.min(area.Position);
                this.MaxBounds = this.MaxBounds.max(area.Position);

                start_byte = gz.ReadByte();
            }

            // Set the map
            this.Size = new KnyttPoint((MaxBounds.X - MinBounds.X) + 1, (MaxBounds.Y - MinBounds.Y) + 1);
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

        public object getWorldTexture2D(string filepath)
        {
            if (BinMode) 
            { 
                var data = BinLoader.GetFile(filepath);
                if (data != null) { return bytesToTexture2D(data); }
            } 
            else 
            { 
                var t = getExternalTexture2D(filepath);
                if (t != null) { return t; }
            }
            return getSystemTexture2D(filepath);
        }

        protected abstract object bytesToTexture2D(byte[] data);
        protected abstract object getExternalTexture2D(string filepath);
        protected abstract object getSystemTexture2D(string filepath);

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
                INIData.Sections.ContainsSection($"x{coords.X}y{coords.Y}") ? new KnyttArea(coords, this) : null;
        }

        public int getMapIndex(KnyttPoint coords)
        {
            return (coords.Y - MinBounds.Y) * Size.X + (coords.X - MinBounds.X);
        }

        public int getMapLength() => Size.Area3D;

        public void setDirectory(string full_dir, string dir_name)
        {
            this.WorldDirectory = full_dir;
            this.WorldDirectoryName = dir_name;
        }
    }
}
