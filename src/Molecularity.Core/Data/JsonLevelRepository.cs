using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Molecularity.Core.Data {
    public class JsonLevelRepository : ILevelRepository {
        private readonly Dictionary<int, LevelConfig> _levels = new Dictionary<int, LevelConfig>();

        public JsonLevelRepository(string levelsDirectory) {
            if (!Directory.Exists(levelsDirectory)) {
                throw new DirectoryNotFoundException($"Levels directory not found: {levelsDirectory}");
            }

            foreach (string filePath in Directory.GetFiles(levelsDirectory, "*.json")) {
                string json = File.ReadAllText(filePath);
                LevelConfig level;
                try {
                    level = LevelJson.Parse(json);
                }
                catch (InvalidOperationException ex) {
                    string fileName = Path.GetFileName(filePath);
                    throw new InvalidOperationException(
                        $"Level file '{fileName}' is invalid: {ex.Message}", ex);
                }

                if (_levels.ContainsKey(level.LevelId)) {
                    throw new InvalidOperationException(
                        $"Duplicate levelId {level.LevelId} found in file '{Path.GetFileName(filePath)}'.");
                }

                _levels[level.LevelId] = level;
            }
        }

        public LevelConfig Get(int levelId) {
            if (!_levels.TryGetValue(levelId, out LevelConfig? level)) {
                throw new KeyNotFoundException($"Level with id {levelId} not found.");
            }
            return level;
        }

        public IEnumerable<int> GetAll() {
            return _levels.Keys.OrderBy(k => k);
        }
    }
}
