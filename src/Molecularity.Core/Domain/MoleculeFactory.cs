using System;
using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;
using Molecularity.Core.Domain.Exceptions;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Domain {
    public static class MoleculeFactory {
        public static Molecule Create(MoleculeConfig config, BalanceConfig? balance = null) {
            balance ??= BalanceConfig.Default;
            var molecule = new Molecule(config, CreateAbility(config.Type, balance));
            foreach (IPassiveProperty? passive in CreatePassives(config.Type, balance)) {
                molecule.AddPassive(passive);
            }

            return molecule;
        }

        private static IAbility CreateAbility(MoleculeType type, BalanceConfig balance) => type switch {
            MoleculeType.Simple => new NoAbility(),
            MoleculeType.Lazy => new NoAbility(),
            MoleculeType.Shield => new NoAbility(),
            MoleculeType.Lock => new NoAbility(),
            MoleculeType.Parasite => new NoAbility(),
            MoleculeType.Anchor => new HealNeighborsAbility(balance.AnchorHeal),
            _ => throw new UnknownMoleculeTypeException(type)
        };

        private static IEnumerable<IPassiveProperty> CreatePassives(MoleculeType type, BalanceConfig balance) => type switch {
            MoleculeType.Simple => Array.Empty<IPassiveProperty>(),
            MoleculeType.Lazy => new IPassiveProperty[] { new LazyPassive(balance.LazyStep) },
            MoleculeType.Shield => new IPassiveProperty[] { new ShieldPassive(balance.ShieldTurns) },
            MoleculeType.Lock => new IPassiveProperty[] { new LockPassive(balance.LockTurns) },
            MoleculeType.Parasite => new IPassiveProperty[] { new NeighborCountDecrementPassive() },
            MoleculeType.Anchor => new IPassiveProperty[] { new FlatDecrementPassive(balance.AnchorDecrement) },
            _ => throw new UnknownMoleculeTypeException(type)
        };
    }
}
