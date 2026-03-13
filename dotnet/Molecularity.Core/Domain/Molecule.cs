using System.Diagnostics.CodeAnalysis;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;

namespace Molecularity.Core.Domain {
    public class Molecule {
        public readonly int Id;
        public readonly MoleculeType Type;

        public int Value { get; private set; }
        public bool IsAlive { get; private set; } = true;

        public IAbility Ability { get; private set; }

        public Molecule(MoleculeConfig config, [NotNull] IAbility ability) {
            Id = config.Id;
            Type = config.Type;
            Value = config.InitialValue;
            Ability = ability;
        }

        public void ApplyDelta(int delta) {
            Value += delta;
        }

        public void Remove() {
            IsAlive = false;
        }

        public void UseAbility(MoleculeGraph graph) {
            Ability.Execute(this, graph);
        }
    }
}
