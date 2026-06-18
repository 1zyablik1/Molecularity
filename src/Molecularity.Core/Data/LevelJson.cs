using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Molecularity.Core.Data {
    public static class LevelJson {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        public static LevelConfig Parse(string json) {
            LevelConfig? level = JsonSerializer.Deserialize<LevelConfig>(json, Options);
            if (level == null) {
                throw new InvalidOperationException("Failed to deserialize level: result was null.");
            }

            LevelValidationResult result = level.Validate();
            if (!result.IsValid) {
                string allErrors = string.Join("; ", result.Errors);
                throw new InvalidOperationException($"Level is invalid: {allErrors}");
            }

            return level;
        }
    }
}
