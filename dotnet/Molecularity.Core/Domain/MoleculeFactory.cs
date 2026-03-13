using System;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;

namespace Molecularity.Core.Domain {
    public static class MoleculeFactory {
        public static Molecule Create(MoleculeConfig config) {
            return new Molecule(config, new NoAbility());
        }
    }
}
