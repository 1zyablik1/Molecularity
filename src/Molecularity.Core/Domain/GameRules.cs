namespace Molecularity.Core.Domain {
    public static class GameRules {
        public static bool IsWin(MoleculeGraph graph) {
            return graph.IsEmpty();
        }

        public static (bool IsLoss, int? CulpritId) IsLose(MoleculeGraph graph) {
            foreach (Molecule molecule in graph.GetAliveAll()) {
                if (molecule.Value <= 0) {
                    return (true, molecule.Id);
                }
            }

            return (false, null);
        }
    }
}
