using System.Collections.Generic;

namespace Molecularity.Core.Domain.Abilities {
    /// <summary>
    /// Anchor on-click ability: heals every alive neighbor by <see cref="GameBalance.AnchorHeal"/>.
    /// Fires before the molecule is removed and before the global decrement.
    /// </summary>
    public class HealNeighborsAbility : IAbility {
        public IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph) {
            var events = new List<TurnEvent>();
            foreach (Molecule neighbor in graph.GetAliveNeighbors(source.Id)) {
                neighbor.ApplyDelta(GameBalance.AnchorHeal);
                events.Add(new ValueChangedEvent(neighbor.Id, GameBalance.AnchorHeal, neighbor.Value, neighbor.IsRevealed, ValueChangeReason.Ability));
            }

            return events;
        }
    }
}
