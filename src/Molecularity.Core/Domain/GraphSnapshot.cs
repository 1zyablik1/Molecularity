using System.Collections.Generic;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Domain {
    public record MoleculeSnapshot(int Id, int Value, bool IsAlive, bool IsRevealed, IEnumerable<IPassiveProperty> Passives);
    public record MoleculeConnectionSnapshot(int FromId, int ToId);

    public record GraphSnapshot(List<MoleculeSnapshot> Molecules, List<MoleculeConnectionSnapshot> Connections);
}
