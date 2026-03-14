using System.Collections.Generic;

namespace Molecularity.Core.Domain {
    public record TurnResult(int RemovedMoleculeId, List<MoleculeValueChange> Changes, int? CulpritId = null);

    public record MoleculeValueChange(int MoleculeId, int Delta, int NewValue);
}
