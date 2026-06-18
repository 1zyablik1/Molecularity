using System.Collections.Generic;
using Molecularity.Core.Data;

namespace Molecularity.Core.Domain {
    public static class LevelBuilder {
        public static MoleculeGraph Build(LevelConfig levelConfig) {
            BalanceConfig balance = levelConfig.Balance ?? BalanceConfig.Default;
            var graph = new MoleculeGraph();

            foreach (MoleculeConfig moleculeConfig in levelConfig.Molecules) {
                graph.AddMolecule(MoleculeFactory.Create(moleculeConfig, balance));
            }

            foreach (ConnectionConfig connection in levelConfig.Connections) {
                graph.AddConnection(connection.FromId, connection.ToId);
            }

            return graph;
        }
    }
}
