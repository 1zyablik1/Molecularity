using System.Collections.Generic;

namespace Molecularity.Core.Domain {
    public record TurnResult(int RemovedMoleculeId, IReadOnlyList<TurnEvent> Events, int? CulpritId = null);
}
