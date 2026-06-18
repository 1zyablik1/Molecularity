using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Molecularity.Core.Data {
    public class JsonLevelRepository : ILevelRepository {
        private readonly Dictionary<int, LevelConfig> _levels = new Dictionary<int, LevelConfig>();

        public JsonLevelRepository(string levelsDirectory) {
            if (!Directory.Exists(levelsDirectory)) {
                throw new DirectoryNotFoundException($"Levels directory not found: {levelsDirectory}");
            }

            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                Converters = { new JsonStringEnumConverter() }
            };

            foreach (string filePath in Directory.GetFiles(levelsDirectory, "*.json")) {
                string json = File.ReadAllText(filePath);
                LevelConfig? level = JsonSerializer.Deserialize<LevelConfig>(json, options);
                if (level == null) {
                    throw new InvalidOperationException($"Failed to deserialize level from file: {Path.GetFileName(filePath)}");
                }

                LevelValidationResult result = level.Validate();
                if (!result.IsValid) {
                    string allErrors = string.Join("; ", result.Errors);
                    throw new InvalidOperationException(
                        $"Level file '{Path.GetFileName(filePath)}' is invalid: {allErrors}");
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
