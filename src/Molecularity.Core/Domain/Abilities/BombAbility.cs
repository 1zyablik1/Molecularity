using System.Collections.Generic;
using System.Linq;

namespace Molecularity.Core.Domain.Abilities {
    /// <summary>
    /// Bomb on-click ability: removes all currently-alive neighbours (explosion).
    /// The removal ignores protection (calls graph.RemoveMolecule which bypasses IsRemovable).
    /// Removed neighbours do NOT trigger their own abilities (no cascade).
    /// </summary>
    public class BombAbility : IAbility {
        public IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph) {
            // Materialise neighbour ids BEFORE any mutation so the list is stable.
            List<int> neighbourIds = graph.GetAliveNeighbors(source.Id)
                .Select(m => m.Id)
                .ToList();

            var events = new List<TurnEvent>();
            foreach (int id in neighbourIds) {
                graph.RemoveMolecule(id);
                events.Add(new MoleculeRemovedEvent(id));
            }

            return events;
        }
    }
}
