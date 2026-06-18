using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private IAbility Ability { get; set; }

        private readonly List<IPassiveProperty> _passives = new();

        public Molecule(MoleculeConfig config, [NotNull] IAbility ability) {
            Id = config.Id;
            Type = config.Type;
            Value = config.InitialValue;
            IsRevealed = config.IsInitiallyRevealed;

            Ability = ability;
        }

        public void ApplyDelta(int delta) {
            Value += delta;
        }

        public int GetModifiedDelta(int baseDelta, MoleculeGraph graph) {
            int delta = baseDelta;
            foreach (IPassiveProperty? passive in _passives) {
                delta = passive.ModifyDelta(delta, this, graph);
            }

            return delta;
        }

        public void Remove() {
            IsAlive = false;
        }

        public void Reveal() {
            IsRevealed = true;
        }

        public IReadOnlyList<TurnEvent> UseAbility(MoleculeGraph graph) => Ability.Execute(this, graph);

        public void AddPassive(IPassiveProperty passive) {
            _passives.Add(passive);
        }

        public void TickPassives(MoleculeGraph graph) {
            foreach (IPassiveProperty? passive in _passives) {
                passive.OnPassiveApply(this, graph);
            }

            _passives.RemoveAll(p => p.IsExpired);
        }

        public void SetFromSnapshot(MoleculeSnapshot moleculeSnapshot) {
            Id = moleculeSnapshot.Id;
            Value = moleculeSnapshot.Value;
            IsAlive = moleculeSnapshot.IsAlive;
            IsRevealed = moleculeSnapshot.IsRevealed;

            _passives.Clear();
            _passives.AddRange(moleculeSnapshot.Passives.Select(p => p.Clone()));
        }

        //TODO: maybe move to some helper class?
        public IEnumerable<IPassiveProperty> ClonePassives() {
            return _passives.Select(passive => passive.Clone()).ToList();
        }
    }
}
