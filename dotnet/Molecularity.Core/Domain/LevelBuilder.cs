using System.Collections.Generic;
using Molecularity.Core.Data;

namespace Molecularity.Core.Domain {
    public class LevelBuilder {
        public MoleculeGraph Build(LevelConfig levelConfig) {
            var graph = new MoleculeGraph();

            foreach (MoleculeConfig moleculeConfig in levelConfig.Molecules) {
                graph.AddMolecule(MoleculeFactory.Create(moleculeConfig));
            }

            foreach (ConnectionConfig connection in levelConfig.Connections) {
                graph.AddConnection(connection.FromId, connection.ToId);
            }

            return graph;
        }
    }
}
