using System.Collections.Generic;
using Molecularity.Core.Data;

namespace Molecularity.Core.Domain.Abilities {
    /// <summary>
    /// Anchor on-click ability: heals every alive neighbor by <paramref name="healAmount"/>.
    /// Fires before the molecule is removed and before the global decrement.
    /// </summary>
    public class HealNeighborsAbility : IAbility {
        private readonly int _healAmount;

        public HealNeighborsAbility(int healAmount = GameBalance.AnchorHeal) {
            _healAmount = healAmount;
        }

        public IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph) {
            var events = new List<TurnEvent>();
            foreach (Molecule neighbor in graph.GetAliveNeighbors(source.Id)) {
                neighbor.ApplyDelta(_healAmount);
                events.Add(new ValueChangedEvent(neighbor.Id, _healAmount, neighbor.Value, neighbor.IsRevealed, ValueChangeReason.Ability));
            }

            return events;
        }
    }
}
