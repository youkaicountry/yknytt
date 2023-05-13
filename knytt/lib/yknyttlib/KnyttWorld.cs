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
        public KnyttArea[] Map { get; protected set; }

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

            var areas = new List<KnyttArea>();

            // Area definition starts with an 'x' character
            while (gz.ReadByte() == 'x')
            {
                var area = new KnyttArea(gz, this);
                areas.Add(area);

                this.MinBounds = this.MinBounds.min(area.Position);
                this.MaxBounds = this.MaxBounds.max(area.Position);
            }

            // Set the map
            this.Size = new KnyttPoint((MaxBounds.x - MinBounds.x) + 1, (MaxBounds.y - MinBounds.y) + 1);
            this.Map = new KnyttArea[this.Size.Area];

            foreach (var area in areas)
            {
                this.Map[getMapIndex(area.Position)] = area;
            }
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


        // TODO: This logic needs refactoring when things are fleshed out
        public KnyttArea getArea(KnyttPoint coords)
        {
            bool out_of_bounds = coords.x < MinBounds.x || coords.x > MaxBounds.x || coords.y < MinBounds.y || coords.y > MaxBounds.y;
            return out_of_bounds ? null : this.Map[getMapIndex(coords)];
        }

        public int getMapIndex(KnyttPoint coords)
        {
            return (coords.y - MinBounds.y) * Size.x + (coords.x - MinBounds.x);
        }

        public void setDirectory(string full_dir, string dir_name)
        {
            this.WorldDirectory = full_dir;
            this.WorldDirectoryName = dir_name;
        }
    }
}
