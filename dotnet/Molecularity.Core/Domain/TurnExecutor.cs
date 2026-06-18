using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Domain.Exceptions;

namespace Molecularity.Core.Domain {
    public class TurnExecutor {
        private const int DeltaPerTurn = GameBalance.BaseDecrement;

        private readonly MoleculeGraph _graph;

        public TurnExecutor(MoleculeGraph graph) {
            _graph = graph;
        }

        public TurnResult Execute(int moleculeId) {
            Molecule molecule = _graph.GetMolecule(moleculeId);
            if (!molecule.IsAlive) {
                throw new MoleculeAlreadyRemovedException(moleculeId);
            }

            var events = new List<TurnEvent>();

            events.AddRange(molecule.UseAbility(_graph));

            List<int> hiddenBefore = _graph.GetAliveNeighbors(molecule.Id)
                .Where(m => !m.IsRevealed)
                .Select(m => m.Id)
                .ToList();

            _graph.RemoveMolecule(molecule.Id);
            events.Add(new MoleculeRemovedEvent(molecule.Id));

            foreach (int id in hiddenBefore) {
                events.Add(new MoleculeRevealedEvent(id));
            }

            foreach (Molecule alive in _graph.GetAliveAll()) {
                int delta = alive.GetModifiedDelta(DeltaPerTurn, _graph);
                alive.ApplyDelta(delta);
                alive.TickPassives(_graph);
                events.Add(new ValueChangedEvent(alive.Id, delta, alive.Value, alive.IsRevealed, ValueChangeReason.Decrement));
            }

            return new TurnResult(moleculeId, events);
        }
    }
}
