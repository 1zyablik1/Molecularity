using System;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Domain {
    public static class MoleculeFactory {
        public static Molecule Create(MoleculeConfig config) {
            return config.Type switch {
                MoleculeType.Simple  => new Molecule(config, new NoAbility(), new NoPassive()),
                MoleculeType.Shield  => new Molecule(config, new NoAbility(), new ShieldPassive(2)),
                // MoleculeType.Anchor  => new Molecule(config, new HealNeighborsAbility(), new NoPassive()),
                // MoleculeType.Parasite=> new Molecule(config, new NoAbility(), new ParasitePassive()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
