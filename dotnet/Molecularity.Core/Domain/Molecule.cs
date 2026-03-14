using System.Diagnostics.CodeAnalysis;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Domain {
    public class Molecule {
        public int Id { get; private set; }
        public MoleculeType Type { get; private set; }

        public int Value { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public bool IsRevealed { get; private set; }

        public IAbility Ability { get; private set; }

        public IPassiveProperty PassiveProperty { get; private set; }

        public Molecule(MoleculeConfig config, [NotNull] IAbility ability, [NotNull] IPassiveProperty passiveProperty) {
            Id = config.Id;
            Type = config.Type;
            Value = config.InitialValue;
            IsRevealed = config.IsInitiallyRevealed;

            Ability = ability;
            PassiveProperty = passiveProperty;
        }

        public void ApplyDelta(int delta) {
            Value += delta;
        }

        public void Remove() {
            IsAlive = false;
        }

        public void Reveal() {
            IsRevealed = true;
        }

        public void UseAbility(MoleculeGraph graph) {
            Ability.Execute(this, graph);
        }
    }
}
