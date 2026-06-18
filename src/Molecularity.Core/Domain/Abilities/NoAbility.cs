using System;
using System.Collections.Generic;

namespace Molecularity.Core.Domain.Abilities {
    public class NoAbility : IAbility {
        public IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph) {
            return Array.Empty<TurnEvent>();
        }
    }
}
