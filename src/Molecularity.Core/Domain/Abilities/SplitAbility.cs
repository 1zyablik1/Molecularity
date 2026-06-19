using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;

namespace Molecularity.Core.Domain.Abilities {
    /// <summary>
    /// Splitter on-click ability: spawns 2 new Simple child molecules with value 2,
    /// each connected to every currently-alive neighbour of the splitter.
    /// The children are NOT connected to each other.
    /// The splitter itself is removed by TurnExecutor after the ability (not here).
    /// </summary>
    public class SplitAbility : IAbility {
        private const int ChildValue = 2;
        private const int ChildCount = 2;

        public IReadOnlyList<TurnEvent> Execute(Molecule source, MoleculeGraph graph) {
            // Materialise alive-neighbour ids BEFORE any mutation so the list is stable.
            List<int> neighbourIds = graph.GetAliveNeighbors(source.Id)
                .Select(m => m.Id)
                .ToList();

            var events = new List<TurnEvent>();

            for (int i = 0; i < ChildCount; i++) {
                // Compute the next free id AFTER each add so the two children get distinct ids.
                int childId = graph.NextId();

                Molecule child = MoleculeFactory.Create(
                    new MoleculeConfig(childId, MoleculeType.Simple, ChildValue, IsInitiallyRevealed: true));

                graph.AddMolecule(child);

                foreach (int neighbourId in neighbourIds) {
                    graph.AddConnection(childId, neighbourId);
                }

                events.Add(new MoleculeSpawnedEvent(childId, MoleculeType.Simple, ChildValue));
            }

            return events;
        }
    }
}
