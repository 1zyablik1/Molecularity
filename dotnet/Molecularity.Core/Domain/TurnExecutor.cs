using System;
using System.Collections.Generic;

namespace Molecularity.Core.Domain {
    public class TurnExecutor {
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
                //TODO : update delta calculation
                int delta = -1;
                alive.ApplyDelta(delta);
                changes.Add(new MoleculeValueChange(alive.Id, delta, alive.Value));
            }

            return new TurnResult(moleculeId, changes);
        }
    }
}
