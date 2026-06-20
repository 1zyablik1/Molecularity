using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Domain {
    public record MoleculeSnapshot(int Id, int Value, bool IsAlive, bool IsRevealed, IEnumerable<IPassiveProperty> Passives, MoleculeType Type, IAbility Ability);
    public record MoleculeConnectionSnapshot(int FromId, int ToId);

    public record GraphSnapshot(List<MoleculeSnapshot> Molecules, List<MoleculeConnectionSnapshot> Connections);
}
