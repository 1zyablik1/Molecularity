using System;
using System.Collections.Generic;

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
                throw new Exception($"Molecule with id {moleculeId} is already removed.");
            }

            molecule.UseAbility(_graph);
            _graph.RemoveMolecule(molecule.Id);

            List<MoleculeValueChange> changes = new();
            foreach (Molecule alive in _graph.GetAliveAll()) {
                int delta = alive.GetModifiedDelta(DeltaPerTurn, _graph);
                alive.ApplyDelta(delta);
                alive.TickPassives(_graph);

                changes.Add(new MoleculeValueChange(alive.Id, delta, alive.Value, alive.IsRevealed));
            }

            return new TurnResult(moleculeId, changes);
        }
    }
}
