using System.Collections.Generic;

namespace Molecularity.Core.Data {
    public sealed class LevelValidationResult {
        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;

        public LevelValidationResult(List<string> errors) {
            Errors = errors;
        }
    }
}
