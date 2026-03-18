using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace YKnytt.Services
{
    public static class LevelMetadataCache
    {
        // runtime source of truth
        private static Dictionary<string, LevelMetadata> _cache = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };
        private static string CachePath =>
            ProjectSettings.GlobalizePath("user://level_metadata_cache.json");
        public static DateTime LastSyncTime { get; private set; }
        public static void Load()
        {
            if (!File.Exists(CachePath))
            {
                _cache = new();
                return;
            }
            try
            {
                var json = File.ReadAllText(CachePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, LevelMetadata>>(json)
                         ?? new Dictionary<string, LevelMetadata>();
            }
            catch
            {
                _cache = new();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_cache, _jsonOptions);
                File.WriteAllText(CachePath, json);
                LastSyncTime = DateTime.UtcNow;
            }
            catch
            {
                // swallow – metadata persistence is non-critical
            }
        }

        // merges server state into local cache (server wins)
        public static void UpdateFromServer(IEnumerable<LevelMetadata> serverLevels)
        {
            if (serverLevels == null) return;

            foreach (var level in serverLevels)
            {
                if (level == null || string.IsNullOrWhiteSpace(level.LevelId))
                    continue;

                _cache[level.LevelId] = level;
            }
            Save();
        }
        public static LevelMetadata Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _cache.TryGetValue(id, out var level) ? level : null;
        }
        public static IEnumerable<LevelMetadata> GetAll() => _cache.Values;
        public static bool Contains(string id) =>
            !string.IsNullOrWhiteSpace(id) && _cache.ContainsKey(id);
        public static void Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (_cache.Remove(id))
                Save();
        }
        public static void Clear(bool persist = true)
        {
            _cache.Clear();
            if (persist) Save();
        }
    }
}
