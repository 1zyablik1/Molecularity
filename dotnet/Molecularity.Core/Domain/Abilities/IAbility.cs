using System.Collections.Generic;

namespace Molecularity.Core.Domain.Abilities {
    public interface IAbility {
        IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph);
    }
}
